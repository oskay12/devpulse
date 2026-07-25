using DevPulse.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Data;

/// <summary>
/// EF Core database context for DevPulse, backed by AWS RDS PostgreSQL.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectToken> ProjectTokens => Set<ProjectToken>();
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<RepositoryContributor> RepositoryContributors => Set<RepositoryContributor>();
    public DbSet<Commit> Commits => Set<Commit>();
    public DbSet<CommitFile> CommitFiles => Set<CommitFile>();
    public DbSet<PullRequest> PullRequests => Set<PullRequest>();
    public DbSet<PullRequestReview> PullRequestReviews => Set<PullRequestReview>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<DeveloperMetric> DeveloperMetrics => Set<DeveloperMetric>();
    public DbSet<CodeHealthScore> CodeHealthScores => Set<CodeHealthScore>();
    public DbSet<ArchitecturalPattern> ArchitecturalPatterns => Set<ArchitecturalPattern>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureProjectToken(modelBuilder);
        ConfigureRepository(modelBuilder);
        ConfigureRepositoryContributor(modelBuilder);
        ConfigureCommit(modelBuilder);
        ConfigureCommitFile(modelBuilder);
        ConfigurePullRequest(modelBuilder);
        ConfigurePullRequestReview(modelBuilder);
        ConfigureReviewComment(modelBuilder);
        ConfigureDeveloperMetric(modelBuilder);
        ConfigureCodeHealthScore(modelBuilder);
        ConfigureArchitecturalPattern(modelBuilder);
        ConfigureMediaAsset(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }

    private static void ConfigureProjectToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectToken>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.HasIndex(t => t.RepositoryId);

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(t => t.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRepository(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Repository>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.FullName).IsUnique();
            entity.HasIndex(r => new { r.Provider, r.ExternalId }).IsUnique();
            entity.HasIndex(r => r.OwnerId);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRepositoryContributor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepositoryContributor>(entity =>
        {
            // Composite primary key: a user has exactly one contributor role per repository.
            entity.HasKey(rc => new { rc.RepositoryId, rc.UserId });

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(rc => rc.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(rc => rc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCommit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commit>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => new { c.RepositoryId, c.Sha }).IsUnique();
            entity.HasIndex(c => c.AuthorId);
            entity.HasIndex(c => c.CommittedAt);

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(c => c.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureCommitFile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommitFile>(entity =>
        {
            entity.HasKey(cf => cf.Id);
            entity.HasIndex(cf => cf.CommitId);

            entity.HasOne<Commit>()
                .WithMany()
                .HasForeignKey(cf => cf.CommitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePullRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PullRequest>(entity =>
        {
            entity.HasKey(pr => pr.Id);
            entity.HasIndex(pr => new { pr.RepositoryId, pr.PrNumber }).IsUnique();
            entity.HasIndex(pr => pr.AuthorId);
            entity.HasIndex(pr => pr.State);

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(pr => pr.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(pr => pr.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(pr => pr.MergedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigurePullRequestReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PullRequestReview>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.PullRequestId);
            entity.HasIndex(r => r.ReviewerId);

            entity.HasOne<PullRequest>()
                .WithMany()
                .HasForeignKey(r => r.PullRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReviewComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReviewComment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.PullRequestId);
            entity.HasIndex(c => c.ReviewId);

            entity.HasOne<PullRequest>()
                .WithMany()
                .HasForeignKey(c => c.PullRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict here (instead of Cascade) to avoid a second cascade path into
            // PullRequest via Review -> PullRequest; deletion is driven by PullRequestId above.
            entity.HasOne<PullRequestReview>()
                .WithMany()
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDeveloperMetric(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeveloperMetric>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => new { m.UserId, m.RepositoryId, m.PeriodType, m.PeriodStart }).IsUnique();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(m => m.RepositoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureCodeHealthScore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CodeHealthScore>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.RepositoryId, s.CalculatedAt });

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(s => s.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureArchitecturalPattern(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchitecturalPattern>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.RepositoryId);

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(p => p.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMediaAsset(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.RepositoryId);
            entity.HasIndex(a => a.PullRequestId);

            entity.HasOne<Repository>()
                .WithMany()
                .HasForeignKey(a => a.RepositoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<PullRequest>()
                .WithMany()
                .HasForeignKey(a => a.PullRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
