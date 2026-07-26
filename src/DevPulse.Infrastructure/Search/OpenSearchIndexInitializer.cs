using DevPulse.Core.SearchDocuments;
using DevPulse.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;

namespace DevPulse.Infrastructure.Search;

/// <summary>
/// Creates the DevPulse indices with explicit mappings.
/// </summary>
/// <remarks>
/// Mappings are declared rather than left to dynamic inference because the
/// distinction matters: analysed <c>text</c> fields are what free-text search
/// matches on, while <c>keyword</c> fields are what filters and aggregations need.
/// Dynamic mapping would make identifiers analysed, and filtering on them would
/// then match the wrong documents.
/// </remarks>
internal sealed class OpenSearchIndexInitializer
{
    private readonly IOpenSearchClient _client;
    private readonly OpenSearchSettings _settings;
    private readonly ILogger<OpenSearchIndexInitializer> _logger;

    public OpenSearchIndexInitializer(
        IOpenSearchClient client,
        IOptions<OpenSearchSettings> settings,
        ILogger<OpenSearchIndexInitializer> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Ensures all three indices exist. Idempotent: safe to call on every startup
    /// and safe if two instances race.
    /// </summary>
    public async Task EnsureIndicesAsync(CancellationToken cancellationToken = default)
    {
        await CreateIfMissingAsync(
            _settings.CommitsIndex,
            descriptor => descriptor.Map<CommitSearchDocument>(m => m.Properties(p => p
                .Keyword(k => k.Name(d => d.Id))
                .Keyword(k => k.Name(d => d.RepositoryId))
                .Keyword(k => k.Name(d => d.RepositoryName))
                .Keyword(k => k.Name(d => d.AuthorName))
                .Keyword(k => k.Name(d => d.AuthorEmail))
                .Text(t => t.Name(d => d.Message))
                .Keyword(k => k.Name(d => d.Branch))
                .Text(t => t.Name(d => d.FilePaths))
                .Text(t => t.Name(d => d.DiffSnippets))
                .Date(d2 => d2.Name(d => d.CommittedAt))
                .Number(n => n.Name(d => d.Additions).Type(NumberType.Integer))
                .Number(n => n.Name(d => d.Deletions).Type(NumberType.Integer)))),
            cancellationToken);

        await CreateIfMissingAsync(
            _settings.PullRequestsIndex,
            descriptor => descriptor.Map<PullRequestSearchDocument>(m => m.Properties(p => p
                .Keyword(k => k.Name(d => d.Id))
                .Keyword(k => k.Name(d => d.RepositoryId))
                .Keyword(k => k.Name(d => d.RepositoryName))
                .Number(n => n.Name(d => d.PrNumber).Type(NumberType.Integer))
                .Text(t => t.Name(d => d.Title))
                .Text(t => t.Name(d => d.Description))
                .Keyword(k => k.Name(d => d.AuthorName))
                .Text(t => t.Name(d => d.ReviewComments))
                .Keyword(k => k.Name(d => d.Reviewers))
                .Keyword(k => k.Name(d => d.State))
                .Date(d2 => d2.Name(d => d.CreatedAt))
                .Date(d2 => d2.Name(d => d.MergedAt)))),
            cancellationToken);

        await CreateIfMissingAsync(
            _settings.ReviewsIndex,
            descriptor => descriptor.Map<CodeReviewSearchDocument>(m => m.Properties(p => p
                .Keyword(k => k.Name(d => d.Id))
                .Keyword(k => k.Name(d => d.PullRequestId))
                .Number(n => n.Name(d => d.PrNumber).Type(NumberType.Integer))
                .Keyword(k => k.Name(d => d.RepositoryName))
                .Keyword(k => k.Name(d => d.AuthorName))
                .Text(t => t.Name(d => d.CommentBody))
                .Keyword(k => k.Name(d => d.FilePath))
                .Number(n => n.Name(d => d.LineNumber).Type(NumberType.Integer))
                .Date(d2 => d2.Name(d => d.CreatedAt)))),
            cancellationToken);
    }

    private async Task CreateIfMissingAsync(
        string indexName,
        Func<CreateIndexDescriptor, CreateIndexDescriptor> configureMapping,
        CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(indexName, ct: cancellationToken);

        if (exists.Exists)
        {
            _logger.LogDebug("OpenSearch index {Index} already exists.", indexName);
            return;
        }

        var response = await _client.Indices.CreateAsync(
            indexName,
            descriptor => configureMapping(descriptor),
            cancellationToken);

        if (response.IsValid)
        {
            _logger.LogInformation("Created OpenSearch index {Index}.", indexName);
            return;
        }

        // Two instances starting together both pass the existence check; the loser
        // gets this error and that is a success, not a failure.
        var alreadyExists = response.ServerError?.Error?.Type
            ?.Contains("resource_already_exists_exception", StringComparison.OrdinalIgnoreCase) == true;

        if (alreadyExists)
        {
            _logger.LogDebug("OpenSearch index {Index} was created concurrently.", indexName);
            return;
        }

        throw new InvalidOperationException(
            $"Failed to create OpenSearch index '{indexName}': {response.DebugInformation}");
    }
}
