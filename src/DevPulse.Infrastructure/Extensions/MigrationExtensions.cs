using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevPulse.Infrastructure.Extensions;

/// <summary>
/// Startup migration helper.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Arbitrary but stable key identifying the DevPulse schema lock. Any value
    /// works as long as every replica uses the same one.
    /// </summary>
    private const long MigrationLockId = 5171130404;

    /// <summary>
    /// Applies pending migrations while holding a PostgreSQL advisory lock.
    /// </summary>
    /// <remarks>
    /// The API runs two replicas and migrates on startup. Without the lock, both
    /// pods can call <c>MigrateAsync</c> at the same time and the loser fails with
    /// "relation already exists", crash-looping the rollout. The advisory lock
    /// serialises them: the second pod waits, then finds nothing to apply.
    ///
    /// The connection is opened explicitly so the lock — which is session-scoped —
    /// is held by the same session that runs the migration.
    /// </remarks>
    public static async Task MigrateDevPulseDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(MigrationExtensions));

        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            logger.LogInformation("Acquiring migration advisory lock {LockId}.", MigrationLockId);
            await dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_lock({0})", [MigrationLockId], cancellationToken);

            try
            {
                var pending = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                var pendingList = pending.ToList();

                if (pendingList.Count == 0)
                {
                    logger.LogInformation("Database schema is up to date; no migrations to apply.");
                }
                else
                {
                    logger.LogInformation(
                        "Applying {Count} pending migration(s): {Migrations}",
                        pendingList.Count,
                        string.Join(", ", pendingList));

                    await dbContext.Database.MigrateAsync(cancellationToken);

                    logger.LogInformation("Migrations applied successfully.");
                }
            }
            finally
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_unlock({0})", [MigrationLockId], CancellationToken.None);
            }
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }
}
