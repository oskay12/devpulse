using DevPulse.Core.Dtos;
using DevPulse.Core.Entities;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Webhook token issuance and verification.
/// </summary>
public interface IProjectTokenService
{
    /// <summary>Lists a repository's tokens (metadata only).</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<List<ProjectTokenDto>> ListAsync(Guid repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a token. The plaintext value is returned once and only its hash is
    /// stored.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<CreateProjectTokenResponse> CreateAsync(
        Guid repositoryId,
        CreateProjectTokenRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes a token.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such token for this repository.</exception>
    Task RevokeAsync(Guid repositoryId, Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a presented token to its stored record.
    /// </summary>
    /// <remarks>
    /// Looks the token up by hash against the unique index, which also identifies
    /// the repository. Authentication therefore never has to trust the repository
    /// named in the request payload.
    /// </remarks>
    /// <returns>
    /// The token if it exists and is neither revoked nor expired; otherwise
    /// <see langword="null"/>.
    /// </returns>
    Task<ProjectToken?> FindActiveByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>Records that a token successfully authenticated a request.</summary>
    Task TouchAsync(Guid tokenId, CancellationToken cancellationToken = default);
}
