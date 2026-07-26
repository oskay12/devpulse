using DevPulse.Core.Dtos;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Core.SearchDocuments;
using DevPulse.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;

namespace DevPulse.Infrastructure.Search;

/// <inheritdoc cref="ISearchService"/>
internal sealed class SearchService : ISearchService
{
    private const string DependencyName = "OpenSearch";

    private readonly IOpenSearchClient _client;
    private readonly OpenSearchSettings _settings;
    private readonly ILogger<SearchService> _logger;

    public SearchService(
        IOpenSearchClient client,
        IOptions<OpenSearchSettings> settings,
        ILogger<SearchService> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<SearchResultDto<CommitSearchResultDto>> SearchCommitsAsync(
        string query,
        PagedQuery paging,
        Guid? repositoryId = null,
        string? author = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync<CommitSearchDocument>(
            _settings.CommitsIndex,
            descriptor => descriptor
                .Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .MultiMatch(mm => mm
                                .Query(query)
                                .Fields(f => f
                                    .Field(d => d.Message, boost: 3)
                                    .Field(d => d.FilePaths)
                                    .Field(d => d.DiffSnippets))))
                        .Filter(BuildCommitFilters(repositoryId, author, from, to))))
                .Highlight(h => h
                    .Fields(
                        f => f.Field(d => d.Message),
                        f => f.Field(d => d.DiffSnippets))),
            paging,
            cancellationToken);

        return Map(
            response,
            paging,
            hit => new CommitSearchResultDto
            {
                Sha = hit.Source.Id,
                RepositoryName = hit.Source.RepositoryName,
                AuthorName = hit.Source.AuthorName,
                Message = hit.Source.Message,
                CommittedAt = hit.Source.CommittedAt,
                HighlightSnippets = Snippets(hit)
            });
    }

    public async Task<SearchResultDto<PullRequestSearchResultDto>> SearchPullRequestsAsync(
        string query,
        PagedQuery paging,
        Guid? repositoryId = null,
        string? state = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync<PullRequestSearchDocument>(
            _settings.PullRequestsIndex,
            descriptor => descriptor
                .Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .MultiMatch(mm => mm
                                .Query(query)
                                .Fields(f => f
                                    .Field(d => d.Title, boost: 3)
                                    .Field(d => d.Description)
                                    .Field(d => d.ReviewComments))))
                        .Filter(BuildPullRequestFilters(repositoryId, state, from, to))))
                .Highlight(h => h
                    .Fields(
                        f => f.Field(d => d.Title),
                        f => f.Field(d => d.Description),
                        f => f.Field(d => d.ReviewComments))),
            paging,
            cancellationToken);

        return Map(
            response,
            paging,
            hit => new PullRequestSearchResultDto
            {
                Id = Guid.TryParse(hit.Source.Id, out var id) ? id : Guid.Empty,
                RepositoryName = hit.Source.RepositoryName,
                PrNumber = hit.Source.PrNumber,
                Title = hit.Source.Title,
                AuthorName = hit.Source.AuthorName,
                State = hit.Source.State,
                CreatedAt = hit.Source.CreatedAt,
                MergedAt = hit.Source.MergedAt,
                HighlightSnippets = Snippets(hit)
            });
    }

    public async Task<SearchResultDto<ReviewSearchResultDto>> SearchReviewsAsync(
        string query,
        PagedQuery paging,
        string? repositoryName = null,
        string? author = null,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync<CodeReviewSearchDocument>(
            _settings.ReviewsIndex,
            descriptor => descriptor
                .Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .MultiMatch(mm => mm
                                .Query(query)
                                .Fields(f => f.Field(d => d.CommentBody))))
                        .Filter(BuildReviewFilters(repositoryName, author))))
                .Highlight(h => h.Fields(f => f.Field(d => d.CommentBody))),
            paging,
            cancellationToken);

        return Map(
            response,
            paging,
            hit => new ReviewSearchResultDto
            {
                Id = Guid.TryParse(hit.Source.Id, out var id) ? id : Guid.Empty,
                PullRequestId = hit.Source.PullRequestId,
                PrNumber = hit.Source.PrNumber,
                RepositoryName = hit.Source.RepositoryName,
                AuthorName = hit.Source.AuthorName,
                CommentBody = hit.Source.CommentBody,
                FilePath = hit.Source.FilePath,
                LineNumber = hit.Source.LineNumber,
                CreatedAt = hit.Source.CreatedAt,
                HighlightSnippets = Snippets(hit)
            });
    }

    private static Func<QueryContainerDescriptor<CommitSearchDocument>, QueryContainer>[] BuildCommitFilters(
        Guid? repositoryId,
        string? author,
        DateTime? from,
        DateTime? to)
    {
        var filters = new List<Func<QueryContainerDescriptor<CommitSearchDocument>, QueryContainer>>();

        if (repositoryId.HasValue)
        {
            filters.Add(f => f.Term(t => t.Field(d => d.RepositoryId).Value(repositoryId.Value)));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            filters.Add(f => f.Term(t => t.Field(d => d.AuthorName).Value(author)));
        }

        if (from.HasValue || to.HasValue)
        {
            filters.Add(f => f.DateRange(r =>
            {
                r = r.Field(d => d.CommittedAt);
                if (from.HasValue)
                {
                    r = r.GreaterThanOrEquals(from.Value);
                }

                if (to.HasValue)
                {
                    r = r.LessThanOrEquals(to.Value);
                }

                return r;
            }));
        }

        return filters.ToArray();
    }

    private static Func<QueryContainerDescriptor<PullRequestSearchDocument>, QueryContainer>[] BuildPullRequestFilters(
        Guid? repositoryId,
        string? state,
        DateTime? from,
        DateTime? to)
    {
        var filters = new List<Func<QueryContainerDescriptor<PullRequestSearchDocument>, QueryContainer>>();

        if (repositoryId.HasValue)
        {
            filters.Add(f => f.Term(t => t.Field(d => d.RepositoryId).Value(repositoryId.Value)));
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            filters.Add(f => f.Term(t => t.Field(d => d.State).Value(state)));
        }

        if (from.HasValue || to.HasValue)
        {
            filters.Add(f => f.DateRange(r =>
            {
                r = r.Field(d => d.CreatedAt);
                if (from.HasValue)
                {
                    r = r.GreaterThanOrEquals(from.Value);
                }

                if (to.HasValue)
                {
                    r = r.LessThanOrEquals(to.Value);
                }

                return r;
            }));
        }

        return filters.ToArray();
    }

    private static Func<QueryContainerDescriptor<CodeReviewSearchDocument>, QueryContainer>[] BuildReviewFilters(
        string? repositoryName,
        string? author)
    {
        var filters = new List<Func<QueryContainerDescriptor<CodeReviewSearchDocument>, QueryContainer>>();

        if (!string.IsNullOrWhiteSpace(repositoryName))
        {
            filters.Add(f => f.Term(t => t.Field(d => d.RepositoryName).Value(repositoryName)));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            filters.Add(f => f.Term(t => t.Field(d => d.AuthorName).Value(author)));
        }

        return filters.ToArray();
    }

    private async Task<ISearchResponse<TDocument>> ExecuteAsync<TDocument>(
        string indexName,
        Func<SearchDescriptor<TDocument>, SearchDescriptor<TDocument>> configure,
        PagedQuery paging,
        CancellationToken cancellationToken)
        where TDocument : class
    {
        ISearchResponse<TDocument> response;

        try
        {
            response = await _client.SearchAsync<TDocument>(
                descriptor => configure(descriptor)
                    .Index(indexName)
                    .From(paging.Skip)
                    .Size(paging.PageSize)
                    .TrackTotalHits(true),
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new DependencyUnavailableException(DependencyName, "OpenSearch request failed.", ex);
        }

        if (response.IsValid)
        {
            return response;
        }

        // A missing index means nothing has been indexed yet — an empty result is
        // the honest answer, not a 503.
        if (response.ServerError?.Error?.Type
                ?.Contains("index_not_found_exception", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning("OpenSearch index {Index} does not exist yet; returning no results.", indexName);
            return response;
        }

        throw new DependencyUnavailableException(
            DependencyName,
            $"OpenSearch query against '{indexName}' failed: {response.DebugInformation}",
            response.OriginalException);
    }

    private static SearchResultDto<TResult> Map<TDocument, TResult>(
        ISearchResponse<TDocument> response,
        PagedQuery paging,
        Func<IHit<TDocument>, TResult> map)
        where TDocument : class
        => new()
        {
            TotalHits = (int)response.Total,
            Page = paging.Page,
            PageSize = paging.PageSize,
            SearchTimeMs = response.Took,
            Results = response.Hits.Select(map).ToList()
        };

    private static List<string> Snippets<TDocument>(IHit<TDocument> hit)
        where TDocument : class
        => hit.Highlight.SelectMany(entry => entry.Value).ToList();
}
