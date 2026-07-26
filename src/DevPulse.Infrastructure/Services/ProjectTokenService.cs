using System.Security.Cryptography;
using System.Text;
using DevPulse.Core.Dtos;
using DevPulse.Core.Entities;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="IProjectTokenService"/>
internal sealed class ProjectTokenService : IProjectTokenService
{
    private const int TokenByteLength = 32;

    private readonly ApplicationDbContext _dbContext;

    public ProjectTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Hashes a token for storage and comparison.
    /// </summary>
    /// <remarks>
    /// Plain SHA-256 rather than a slow KDF, deliberately: these tokens are 256
    /// bits of CSPRNG output, so there is no low-entropy guess space for an
    /// attacker to grind through, and webhook verification must stay cheap enough
    /// to run on every inbound request.
    /// </remarks>
    public static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public async Task<List<ProjectTokenDto>> ListAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRepositoryExistsAsync(repositoryId, cancellationToken);

        return await _dbContext.ProjectTokens
            .AsNoTracking()
            .Where(t => t.RepositoryId == repositoryId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ProjectTokenDto
            {
                Id = t.Id,
                RepositoryId = t.RepositoryId,
                Name = t.Name,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                LastUsedAt = t.LastUsedAt,
                IsRevoked = t.IsRevoked,
                Permissions = t.Permissions
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CreateProjectTokenResponse> CreateAsync(
        Guid repositoryId,
        CreateProjectTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureRepositoryExistsAsync(repositoryId, cancellationToken);

        var token = GenerateToken();

        var entity = new ProjectToken
        {
            Id = Guid.CreateVersion7(),
            RepositoryId = repositoryId,
            TokenHash = HashToken(token),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsRevoked = false,
            Permissions = request.Permissions
        };

        _dbContext.ProjectTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectTokenResponse
        {
            // Returned once. Only the hash is persisted, so this cannot be re-issued.
            Token = token,
            TokenInfo = new ProjectTokenDto
            {
                Id = entity.Id,
                RepositoryId = entity.RepositoryId,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                ExpiresAt = entity.ExpiresAt,
                LastUsedAt = entity.LastUsedAt,
                IsRevoked = entity.IsRevoked,
                Permissions = entity.Permissions
            }
        };
    }

    public async Task RevokeAsync(
        Guid repositoryId,
        Guid tokenId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProjectTokens
            .FirstOrDefaultAsync(
                t => t.Id == tokenId && t.RepositoryId == repositoryId,
                cancellationToken)
            ?? throw new NotFoundException("ProjectToken", tokenId);

        entity.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProjectToken?> FindActiveByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var hash = HashToken(token);
        var now = DateTime.UtcNow;

        // Single lookup against the unique TokenHash index — no need to load a
        // repository's tokens and compare them one by one.
        return await _dbContext.ProjectTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.TokenHash == hash
                     && !t.IsRevoked
                     && (t.ExpiresAt == null || t.ExpiresAt > now),
                cancellationToken);
    }

    public async Task TouchAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        // Bypasses change tracking: this fires on every accepted webhook and the
        // value is advisory, so loading the entity first would be wasted work.
        await _dbContext.ProjectTokens
            .Where(t => t.Id == tokenId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(t => t.LastUsedAt, DateTime.UtcNow),
                cancellationToken);
    }

    private static string GenerateToken()
    {
        var buffer = RandomNumberGenerator.GetBytes(TokenByteLength);

        // URL-safe: these values get pasted into provider webhook configuration.
        return Convert.ToBase64String(buffer)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private async Task EnsureRepositoryExistsAsync(Guid repositoryId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Repositories
            .AsNoTracking()
            .AnyAsync(r => r.Id == repositoryId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("Repository", repositoryId);
        }
    }
}
