# 📊 DevPulse - Complete Data Models & Type Definitions

> **Auto-generated Documentation**
> This file contains all data structures, DTOs, and type definitions for the DevPulse platform.
> Language: C# (.NET Core)
> Last Updated: 2026-07-25

---

## 1. Core Domain Entities

### 1.1 User & Authentication

```csharp
/// <summary>
/// Represents a user account in the DevPulse platform.
/// Stores authentication credentials and profile information.
/// </summary>
public class User
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Unique username for login</summary>
    [JsonPropertyName("username")]
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }

    /// <summary>User email address (unique)</summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>BCrypt hashed password</summary>
    [JsonPropertyName("password_hash")]
    [Required]
    public string PasswordHash { get; set; }

    /// <summary>S3 URL to user avatar image (nullable)</summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>Account creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last successful login timestamp (UTC, nullable)</summary>
    [JsonPropertyName("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>User role for authorization</summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    /// <summary>Account active status (soft delete flag)</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

/// <summary>
/// User role enumeration for RBAC (Role-Based Access Control)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    /// <summary>Standard developer access</summary>
    [JsonPropertyName("developer")]
    Developer = 0,

    /// <summary>Team lead with elevated permissions</summary>
    [JsonPropertyName("team_lead")]
    TeamLead = 1,

    /// <summary>Platform administrator (full access)</summary>
    [JsonPropertyName("admin")]
    Admin = 2
}

/// <summary>
/// API access token for repository webhook integrations.
/// Used for authentication of GitHub/GitLab webhook requests.
/// </summary>
public class ProjectToken
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>SHA-256 hashed token value</summary>
    [JsonPropertyName("token_hash")]
    [Required]
    public string TokenHash { get; set; }

    /// <summary>Human-readable token name/description</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    /// <summary>Token creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Token expiration timestamp (UTC, nullable for no expiry)</summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Last usage timestamp for audit trail (UTC, nullable)</summary>
    [JsonPropertyName("last_used_at")]
    public DateTime? LastUsedAt { get; set; }

    /// <summary>Manual revocation flag</summary>
    [JsonPropertyName("is_revoked")]
    public bool IsRevoked { get; set; }

    /// <summary>Bitwise permission flags</summary>
    [JsonPropertyName("permissions")]
    public TokenPermission Permissions { get; set; }
}

/// <summary>
/// Bitwise flags for granular token permissions.
/// Allows combining multiple permissions using | operator.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenPermission
{
    /// <summary>Read metrics and analytics data</summary>
    [JsonPropertyName("read_metrics")]
    ReadMetrics = 1,

    /// <summary>Receive and process webhook events</summary>
    [JsonPropertyName("write_webhooks")]
    WriteWebhooks = 2,

    /// <summary>Read repository metadata</summary>
    [JsonPropertyName("read_repository")]
    ReadRepository = 4,

    /// <summary>Modify repository settings</summary>
    [JsonPropertyName("write_repository")]
    WriteRepository = 8
}
```

### 1.2 Repository & Project Management

```csharp
/// <summary>
/// Represents a Git repository registered in DevPulse.
/// Tracks metadata from GitHub/GitLab and sync status.
/// </summary>
public class Repository
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Repository name (e.g., "devpulse")</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    /// <summary>Full repository path (e.g., "organization/repo-name")</summary>
    [JsonPropertyName("full_name")]
    [Required]
    [StringLength(200)]
    public string FullName { get; set; }

    /// <summary>Repository description (nullable)</summary>
    [JsonPropertyName("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Git clone URL (HTTPS)</summary>
    [JsonPropertyName("clone_url")]
    [Required]
    [Url]
    public string CloneUrl { get; set; }

    /// <summary>Default branch name (e.g., "main", "master")</summary>
    [JsonPropertyName("default_branch")]
    [Required]
    [StringLength(100)]
    public string DefaultBranch { get; set; }

    /// <summary>Source provider (GitHub, GitLab, etc.)</summary>
    [JsonPropertyName("provider")]
    public RepositoryProvider Provider { get; set; }

    /// <summary>External repository ID from provider API</summary>
    [JsonPropertyName("external_id")]
    [Required]
    public string ExternalId { get; set; }

    /// <summary>Foreign key to User (repository owner)</summary>
    [JsonPropertyName("owner_id")]
    [Required]
    public Guid OwnerId { get; set; }

    /// <summary>Repository creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last webhook sync timestamp (UTC, nullable)</summary>
    [JsonPropertyName("last_synced_at")]
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>Repository visibility flag</summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>Repository monitoring status</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    /// <summary>GitHub/GitLab star count (cached)</summary>
    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }

    /// <summary>Fork count (cached)</summary>
    [JsonPropertyName("fork_count")]
    public int ForkCount { get; set; }
}

/// <summary>
/// Supported Git hosting providers
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RepositoryProvider
{
    /// <summary>GitHub.com or GitHub Enterprise</summary>
    [JsonPropertyName("github")]
    GitHub = 0,

    /// <summary>GitLab.com or self-hosted GitLab</summary>
    [JsonPropertyName("gitlab")]
    GitLab = 1,

    /// <summary>Bitbucket Cloud or Server</summary>
    [JsonPropertyName("bitbucket")]
    Bitbucket = 2
}

/// <summary>
/// Many-to-many join table for repository contributors.
/// Tracks user access and contribution statistics.
/// </summary>
public class RepositoryContributor
{
    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Foreign key to User</summary>
    [JsonPropertyName("user_id")]
    [Required]
    public Guid UserId { get; set; }

    /// <summary>Access role in repository</summary>
    [JsonPropertyName("role")]
    public ContributorRole Role { get; set; }

    /// <summary>When user was added to repository (UTC)</summary>
    [JsonPropertyName("joined_at")]
    public DateTime JoinedAt { get; set; }

    /// <summary>Cached commit count for this user in this repo</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }
}

/// <summary>
/// Repository access roles
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContributorRole
{
    /// <summary>Read-only access</summary>
    [JsonPropertyName("viewer")]
    Viewer = 0,

    /// <summary>Can commit and create PRs</summary>
    [JsonPropertyName("contributor")]
    Contributor = 1,

    /// <summary>Can merge PRs and manage settings</summary>
    [JsonPropertyName("maintainer")]
    Maintainer = 2,

    /// <summary>Full administrative access</summary>
    [JsonPropertyName("owner")]
    Owner = 3
}
```

---

## 2. Git Activity Entities

### 2.1 Commits & Changes

```csharp
/// <summary>
/// Represents a single Git commit.
/// Stores metadata and statistics for analytical processing.
/// </summary>
public class Commit
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Git commit SHA-1 hash (40 characters)</summary>
    [JsonPropertyName("sha")]
    [Required]
    [StringLength(40, MinimumLength = 40)]
    public string Sha { get; set; }

    /// <summary>Foreign key to User (commit author, nullable if not registered)</summary>
    [JsonPropertyName("author_id")]
    public Guid? AuthorId { get; set; }

    /// <summary>Git author name from commit metadata</summary>
    [JsonPropertyName("author_name")]
    [Required]
    [StringLength(200)]
    public string AuthorName { get; set; }

    /// <summary>Git author email from commit metadata</summary>
    [JsonPropertyName("author_email")]
    [Required]
    [EmailAddress]
    public string AuthorEmail { get; set; }

    /// <summary>Commit message (full text)</summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; }

    /// <summary>Branch name where commit was pushed</summary>
    [JsonPropertyName("branch")]
    [Required]
    [StringLength(200)]
    public string Branch { get; set; }

    /// <summary>Git commit timestamp (UTC)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>When commit was indexed in DevPulse (UTC)</summary>
    [JsonPropertyName("indexed_at")]
    public DateTime IndexedAt { get; set; }

    /// <summary>Number of files modified in commit</summary>
    [JsonPropertyName("files_changed")]
    public int FilesChanged { get; set; }

    /// <summary>Total lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Total lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Parent commit SHA (nullable for initial commit)</summary>
    [JsonPropertyName("parent_sha")]
    [StringLength(40)]
    public string? ParentSha { get; set; }
}

/// <summary>
/// Represents a single file changed in a commit.
/// Stores diff statistics for code churn analysis.
/// </summary>
public class CommitFile
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Commit</summary>
    [JsonPropertyName("commit_id")]
    [Required]
    public Guid CommitId { get; set; }

    /// <summary>Relative file path in repository</summary>
    [JsonPropertyName("file_path")]
    [Required]
    public string FilePath { get; set; }

    /// <summary>Type of file change</summary>
    [JsonPropertyName("change_type")]
    public FileChangeType ChangeType { get; set; }

    /// <summary>Lines added in this file</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted in this file</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Truncated diff snippet for OpenSearch indexing (nullable)</summary>
    [JsonPropertyName("diff_snippet")]
    [StringLength(5000)]
    public string? DiffSnippet { get; set; }
}

/// <summary>
/// Git file change operation types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileChangeType
{
    /// <summary>New file created</summary>
    [JsonPropertyName("added")]
    Added = 0,

    /// <summary>Existing file modified</summary>
    [JsonPropertyName("modified")]
    Modified = 1,

    /// <summary>File removed</summary>
    [JsonPropertyName("deleted")]
    Deleted = 2,

    /// <summary>File moved/renamed</summary>
    [JsonPropertyName("renamed")]
    Renamed = 3,

    /// <summary>File copied to new location</summary>
    [JsonPropertyName("copied")]
    Copied = 4
}
```

### 2.2 Pull Requests & Code Reviews

```csharp
/// <summary>
/// Represents a pull request (or merge request in GitLab).
/// Tracks lifecycle, changes, and merge statistics.
/// </summary>
public class PullRequest
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Sequential PR number from provider (e.g., #123)</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>Pull request title</summary>
    [JsonPropertyName("title")]
    [Required]
    [StringLength(300)]
    public string Title { get; set; }

    /// <summary>Full PR description (Markdown, nullable)</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Foreign key to User (PR author)</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Source/head branch name</summary>
    [JsonPropertyName("source_branch")]
    [Required]
    [StringLength(200)]
    public string SourceBranch { get; set; }

    /// <summary>Target/base branch name</summary>
    [JsonPropertyName("target_branch")]
    [Required]
    [StringLength(200)]
    public string TargetBranch { get; set; }

    /// <summary>Current PR state</summary>
    [JsonPropertyName("state")]
    public PullRequestState State { get; set; }

    /// <summary>PR creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last update timestamp (UTC, nullable)</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Merge timestamp (UTC, nullable)</summary>
    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }

    /// <summary>Close timestamp (UTC, nullable)</summary>
    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    /// <summary>Foreign key to User who merged PR (nullable)</summary>
    [JsonPropertyName("merged_by_id")]
    public Guid? MergedById { get; set; }

    /// <summary>Number of commits in PR</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }

    /// <summary>Number of files changed</summary>
    [JsonPropertyName("files_changed")]
    public int FilesChanged { get; set; }

    /// <summary>Total lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Total lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Draft/WIP status flag</summary>
    [JsonPropertyName("is_draft")]
    public bool IsDraft { get; set; }
}

/// <summary>
/// Pull request lifecycle states
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullRequestState
{
    /// <summary>Currently open and awaiting review</summary>
    [JsonPropertyName("open")]
    Open = 0,

    /// <summary>Closed without merging</summary>
    [JsonPropertyName("closed")]
    Closed = 1,

    /// <summary>Successfully merged</summary>
    [JsonPropertyName("merged")]
    Merged = 2,

    /// <summary>Work in progress (draft)</summary>
    [JsonPropertyName("draft")]
    Draft = 3
}

/// <summary>
/// Represents a high-level code review submission.
/// Can contain multiple inline comments.
/// </summary>
public class PullRequestReview
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to PullRequest</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>Foreign key to User (reviewer)</summary>
    [JsonPropertyName("reviewer_id")]
    [Required]
    public Guid ReviewerId { get; set; }

    /// <summary>Review decision/state</summary>
    [JsonPropertyName("state")]
    public ReviewState State { get; set; }

    /// <summary>Overall review comment (nullable)</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>Review submission timestamp (UTC)</summary>
    [JsonPropertyName("submitted_at")]
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// Code review approval states
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewState
{
    /// <summary>Review requested but not submitted</summary>
    [JsonPropertyName("pending")]
    Pending = 0,

    /// <summary>Changes approved</summary>
    [JsonPropertyName("approved")]
    Approved = 1,

    /// <summary>Changes must be made</summary>
    [JsonPropertyName("changes_requested")]
    ChangesRequested = 2,

    /// <summary>General feedback without approval</summary>
    [JsonPropertyName("commented")]
    Commented = 3,

    /// <summary>Review dismissed/invalidated</summary>
    [JsonPropertyName("dismissed")]
    Dismissed = 4
}

/// <summary>
/// Represents an inline code review comment.
/// Can be associated with specific file/line or general PR comment.
/// </summary>
public class ReviewComment
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to PullRequest</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>Foreign key to PullRequestReview (nullable for standalone comments)</summary>
    [JsonPropertyName("review_id")]
    public Guid? ReviewId { get; set; }

    /// <summary>Foreign key to User (comment author)</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Comment body (Markdown)</summary>
    [JsonPropertyName("body")]
    [Required]
    public string Body { get; set; }

    /// <summary>File path for inline comments (nullable for general comments)</summary>
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }

    /// <summary>Line number for inline comments (nullable)</summary>
    [JsonPropertyName("line_number")]
    public int? LineNumber { get; set; }

    /// <summary>Comment creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last edit timestamp (UTC, nullable)</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
```

---

## 3. Analytics & Metrics Entities

### 3.1 Developer Metrics

```csharp
/// <summary>
/// Aggregated developer performance metrics for a time period.
/// Calculated asynchronously by worker services.
/// </summary>
public class DeveloperMetric
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to User</summary>
    [JsonPropertyName("user_id")]
    [Required]
    public Guid UserId { get; set; }

    /// <summary>Foreign key to Repository (nullable for global metrics)</summary>
    [JsonPropertyName("repository_id")]
    public Guid? RepositoryId { get; set; }

    /// <summary>Period start timestamp (UTC)</summary>
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; set; }

    /// <summary>Period end timestamp (UTC)</summary>
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; set; }

    /// <summary>Aggregation period type</summary>
    [JsonPropertyName("period_type")]
    public MetricPeriodType PeriodType { get; set; }

    /// <summary>Total commits authored in period</summary>
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; }

    /// <summary>Total pull requests created</summary>
    [JsonPropertyName("total_pull_requests")]
    public int TotalPullRequests { get; set; }

    /// <summary>Number of PRs reviewed</summary>
    [JsonPropertyName("pull_requests_reviewed")]
    public int PullRequestsReviewed { get; set; }

    /// <summary>Total lines added</summary>
    [JsonPropertyName("lines_added")]
    public int LinesAdded { get; set; }

    /// <summary>Total lines deleted</summary>
    [JsonPropertyName("lines_deleted")]
    public int LinesDeleted { get; set; }

    /// <summary>Number of issues closed</summary>
    [JsonPropertyName("issues_closed")]
    public int IssuesClosed { get; set; }

    /// <summary>Average time to review PRs (in hours)</summary>
    [JsonPropertyName("average_review_time")]
    public decimal AverageReviewTime { get; set; }

    /// <summary>Average time for own PRs to be merged (in hours)</summary>
    [JsonPropertyName("average_pr_merge_time")]
    public decimal AveragePrMergeTime { get; set; }

    /// <summary>Code churn rate: (Added + Deleted) / Total LOC</summary>
    [JsonPropertyName("code_churn_rate")]
    public decimal CodeChurnRate { get; set; }

    /// <summary>When metrics were calculated (UTC)</summary>
    [JsonPropertyName("calculated_at")]
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Time period types for metric aggregation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetricPeriodType
{
    /// <summary>24-hour period</summary>
    [JsonPropertyName("daily")]
    Daily = 0,

    /// <summary>7-day period</summary>
    [JsonPropertyName("weekly")]
    Weekly = 1,

    /// <summary>30-day period</summary>
    [JsonPropertyName("monthly")]
    Monthly = 2,

    /// <summary>90-day period</summary>
    [JsonPropertyName("quarterly")]
    Quarterly = 3,

    /// <summary>365-day period</summary>
    [JsonPropertyName("yearly")]
    Yearly = 4
}
```

### 3.2 Code Health & Quality Metrics

```csharp
/// <summary>
/// Repository code health score calculated by static analysis.
/// Inspired by SonarQube quality gates.
/// </summary>
public class CodeHealthScore
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Calculation timestamp (UTC)</summary>
    [JsonPropertyName("calculated_at")]
    public DateTime CalculatedAt { get; set; }

    /// <summary>Composite score (0-100, higher is better)</summary>
    [JsonPropertyName("overall_score")]
    [Range(0, 100)]
    public decimal OverallScore { get; set; }

    /// <summary>Maintainability index (0-100)</summary>
    [JsonPropertyName("maintainability_score")]
    [Range(0, 100)]
    public decimal MaintainabilityScore { get; set; }

    /// <summary>Test coverage percentage (0-100)</summary>
    [JsonPropertyName("test_coverage_score")]
    [Range(0, 100)]
    public decimal TestCoverageScore { get; set; }

    /// <summary>Documentation completeness (0-100)</summary>
    [JsonPropertyName("documentation_score")]
    [Range(0, 100)]
    public decimal DocumentationScore { get; set; }

    /// <summary>Estimated technical debt in minutes</summary>
    [JsonPropertyName("technical_debt_minutes")]
    public int TechnicalDebtMinutes { get; set; }

    /// <summary>Number of code smells detected</summary>
    [JsonPropertyName("code_smell_count")]
    public int CodeSmellCount { get; set; }

    /// <summary>Code duplication percentage</summary>
    [JsonPropertyName("duplication_percentage")]
    [Range(0, 100)]
    public int DuplicationPercentage { get; set; }

    /// <summary>Cyclomatic complexity score</summary>
    [JsonPropertyName("complexity_score")]
    public int ComplexityScore { get; set; }
}

/// <summary>
/// Detected architectural patterns and component metrics.
/// Used for dependency analysis and architecture visualization.
/// </summary>
public class ArchitecturalPattern
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Pattern type (e.g., "MVC", "Microservices", "Layered")</summary>
    [JsonPropertyName("pattern_type")]
    [Required]
    [StringLength(50)]
    public string PatternType { get; set; }

    /// <summary>Component/module name</summary>
    [JsonPropertyName("component_name")]
    [Required]
    [StringLength(200)]
    public string ComponentName { get; set; }

    /// <summary>Primary file path for component</summary>
    [JsonPropertyName("file_path")]
    [Required]
    public string FilePath { get; set; }

    /// <summary>Total lines of code in component</summary>
    [JsonPropertyName("line_count")]
    public int LineCount { get; set; }

    /// <summary>Coupling score (0-100, lower is better)</summary>
    [JsonPropertyName("coupling_score")]
    [Range(0, 100)]
    public decimal CouplingScore { get; set; }

    /// <summary>Cohesion score (0-100, higher is better)</summary>
    [JsonPropertyName("cohesion_score")]
    [Range(0, 100)]
    public decimal CohesionScore { get; set; }

    /// <summary>Pattern detection timestamp (UTC)</summary>
    [JsonPropertyName("detected_at")]
    public DateTime DetectedAt { get; set; }
}
```

---

## 4. Media & Documentation Entities

### 4.1 S3 Media Assets

```csharp
/// <summary>
/// Represents a file uploaded to S3 bucket.
/// Tracks diagrams, screenshots, charts, and generated reports.
/// </summary>
public class MediaAsset
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository (nullable for user-uploaded assets)</summary>
    [JsonPropertyName("repository_id")]
    public Guid? RepositoryId { get; set; }

    /// <summary>Foreign key to PullRequest (nullable if not PR-related)</summary>
    [JsonPropertyName("pull_request_id")]
    public Guid? PullRequestId { get; set; }

    /// <summary>Foreign key to User (uploader)</summary>
    [JsonPropertyName("uploaded_by_id")]
    [Required]
    public Guid UploadedById { get; set; }

    /// <summary>Original filename</summary>
    [JsonPropertyName("file_name")]
    [Required]
    [StringLength(255)]
    public string FileName { get; set; }

    /// <summary>S3 object key (path in bucket)</summary>
    [JsonPropertyName("s3_key")]
    [Required]
    public string S3Key { get; set; }

    /// <summary>Public CloudFront/S3 URL</summary>
    [JsonPropertyName("s3_url")]
    [Required]
    [Url]
    public string S3Url { get; set; }

    /// <summary>Asset type category</summary>
    [JsonPropertyName("type")]
    public MediaAssetType Type { get; set; }

    /// <summary>File size in bytes</summary>
    [JsonPropertyName("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    /// <summary>MIME type (e.g., "image/png", "application/pdf")</summary>
    [JsonPropertyName("mime_type")]
    [Required]
    [StringLength(100)]
    public string MimeType { get; set; }

    /// <summary>Upload timestamp (UTC)</summary>
    [JsonPropertyName("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    /// <summary>Lambda optimization completion flag</summary>
    [JsonPropertyName("is_optimized")]
    public bool IsOptimized { get; set; }

    /// <summary>S3 key for optimized version (nullable)</summary>
    [JsonPropertyName("optimized_s3_key")]
    public string? OptimizedS3Key { get; set; }
}

/// <summary>
/// Media asset classification
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaAssetType
{
    /// <summary>System architecture diagram (SVG, PNG, etc.)</summary>
    [JsonPropertyName("architecture_diagram")]
    ArchitectureDiagram = 0,

    /// <summary>Test result screenshot</summary>
    [JsonPropertyName("screenshot")]
    Screenshot = 1,

    /// <summary>Analytics chart/graph</summary>
    [JsonPropertyName("chart")]
    Chart = 2,

    /// <summary>Generated PDF report</summary>
    [JsonPropertyName("report")]
    Report = 3,

    /// <summary>Uncategorized media</summary>
    [JsonPropertyName("other")]
    Other = 4
}
```

---

## 5. OpenSearch Index Documents

### 5.1 Search Index Models

```csharp
/// <summary>
/// OpenSearch document for commit full-text search.
/// Indexed with commit messages, file paths, and diff snippets.
/// </summary>
public class CommitSearchDocument
{
    /// <summary>Document ID (commit SHA-1)</summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; }

    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name (for display)</summary>
    [JsonPropertyName("repository_name")]
    [Required]
    public string RepositoryName { get; set; }

    /// <summary>Commit author name</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; }

    /// <summary>Commit author email</summary>
    [JsonPropertyName("author_email")]
    [Required]
    public string AuthorEmail { get; set; }

    /// <summary>Full commit message (indexed for search)</summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; }

    /// <summary>Branch name</summary>
    [JsonPropertyName("branch")]
    [Required]
    public string Branch { get; set; }

    /// <summary>List of changed file paths (indexed)</summary>
    [JsonPropertyName("file_paths")]
    public List<string> FilePaths { get; set; } = new();

    /// <summary>Code diff snippets for content search</summary>
    [JsonPropertyName("diff_snippets")]
    public List<string> DiffSnippets { get; set; } = new();

    /// <summary>Commit timestamp (UTC, for sorting)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>Lines added (for filtering)</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted (for filtering)</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }
}

/// <summary>
/// OpenSearch document for pull request and review search.
/// Indexed with title, description, and review comments.
/// </summary>
public class PullRequestSearchDocument
{
    /// <summary>Document ID (PR UUID as string)</summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; }

    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name</summary>
    [JsonPropertyName("repository_name")]
    [Required]
    public string RepositoryName { get; set; }

    /// <summary>PR number (e.g., 123)</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>PR title (indexed)</summary>
    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; }

    /// <summary>PR description (indexed)</summary>
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; }

    /// <summary>PR author username</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; }

    /// <summary>All review comment bodies (indexed)</summary>
    [JsonPropertyName("review_comments")]
    public List<string> ReviewComments { get; set; } = new();

    /// <summary>Reviewer usernames</summary>
    [JsonPropertyName("reviewers")]
    public List<string> Reviewers { get; set; } = new();

    /// <summary>PR state (for filtering)</summary>
    [JsonPropertyName("state")]
    [Required]
    public string State { get; set; }

    /// <summary>Creation timestamp (UTC, for sorting)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Merge timestamp (UTC, nullable)</summary>
    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }
}

/// <summary>
/// OpenSearch document for code review comments search.
/// Enables searching across all review discussions.
/// </summary>
public class CodeReviewSearchDocument
{
    /// <summary>Document ID (review comment UUID as string)</summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; }

    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>PR number (for display)</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>Repository full name</summary>
    [JsonPropertyName("repository_name")]
    [Required]
    public string RepositoryName { get; set; }

    /// <summary>Comment author username</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; }

    /// <summary>Comment body (indexed)</summary>
    [JsonPropertyName("comment_body")]
    [Required]
    public string CommentBody { get; set; }

    /// <summary>File path for inline comments (nullable)</summary>
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }

    /// <summary>Line number for inline comments (nullable)</summary>
    [JsonPropertyName("line_number")]
    public int? LineNumber { get; set; }

    /// <summary>Comment creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
```

---

## 6. Event-Driven Messages (RabbitMQ)

### 6.1 Webhook Event Messages

```csharp
/// <summary>
/// Base class for all webhook events received from Git providers.
/// Contains common metadata for event processing.
/// </summary>
public abstract class WebhookEvent
{
    /// <summary>Unique event ID for deduplication</summary>
    [JsonPropertyName("event_id")]
    [Required]
    public Guid EventId { get; set; }

    /// <summary>Event type identifier (e.g., "push", "pull_request")</summary>
    [JsonPropertyName("event_type")]
    [Required]
    public string EventType { get; set; }

    /// <summary>Target repository UUID</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Source provider</summary>
    [JsonPropertyName("provider")]
    public RepositoryProvider Provider { get; set; }

    /// <summary>When webhook was received (UTC)</summary>
    [JsonPropertyName("received_at")]
    public DateTime ReceivedAt { get; set; }

    /// <summary>Original JSON payload (for audit/debugging)</summary>
    [JsonPropertyName("raw_payload")]
    [Required]
    public string RawPayload { get; set; }
}

/// <summary>
/// Webhook event for Git push containing new commits.
/// Triggers commit indexing and metrics calculation.
/// </summary>
public class PushWebhookEvent : WebhookEvent
{
    /// <summary>Branch that was pushed to</summary>
    [JsonPropertyName("branch")]
    [Required]
    public string Branch { get; set; }

    /// <summary>List of commits in push</summary>
    [JsonPropertyName("commits")]
    [Required]
    public List<CommitPayload> Commits { get; set; } = new();

    /// <summary>User who performed the push</summary>
    [JsonPropertyName("pushed_by_id")]
    [Required]
    public Guid PushedById { get; set; }
}

/// <summary>
/// Individual commit data from webhook payload.
/// Mapped to Commit entity during processing.
/// </summary>
public class CommitPayload
{
    /// <summary>Git commit SHA-1 hash</summary>
    [JsonPropertyName("sha")]
    [Required]
    public string Sha { get; set; }

    /// <summary>Commit message</summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; }

    /// <summary>Author name from Git</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; }

    /// <summary>Author email from Git</summary>
    [JsonPropertyName("author_email")]
    [Required]
    public string AuthorEmail { get; set; }

    /// <summary>Commit timestamp</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>Newly created files</summary>
    [JsonPropertyName("added_files")]
    public List<string> AddedFiles { get; set; } = new();

    /// <summary>Modified existing files</summary>
    [JsonPropertyName("modified_files")]
    public List<string> ModifiedFiles { get; set; } = new();

    /// <summary>Deleted files</summary>
    [JsonPropertyName("removed_files")]
    public List<string> RemovedFiles { get; set; } = new();
}

/// <summary>
/// Webhook event for pull request lifecycle changes.
/// Triggers PR entity updates and review workflows.
/// </summary>
public class PullRequestWebhookEvent : WebhookEvent
{
    /// <summary>Action type (e.g., "opened", "closed", "merged", "reopened")</summary>
    [JsonPropertyName("action")]
    [Required]
    public string Action { get; set; }

    /// <summary>PR number</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>PR title</summary>
    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; }

    /// <summary>PR description</summary>
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; }

    /// <summary>PR author UUID</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Source branch</summary>
    [JsonPropertyName("source_branch")]
    [Required]
    public string SourceBranch { get; set; }

    /// <summary>Target branch</summary>
    [JsonPropertyName("target_branch")]
    [Required]
    public string TargetBranch { get; set; }
}

/// <summary>
/// Webhook event for code review submissions.
/// Triggers review indexing and notification workflows.
/// </summary>
public class ReviewWebhookEvent : WebhookEvent
{
    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>PR number</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>Reviewer UUID</summary>
    [JsonPropertyName("reviewer_id")]
    [Required]
    public Guid ReviewerId { get; set; }

    /// <summary>Review state (e.g., "approved", "changes_requested")</summary>
    [JsonPropertyName("state")]
    [Required]
    public string State { get; set; }

    /// <summary>Review comment (nullable)</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
```

### 6.2 Worker Job Messages

```csharp
/// <summary>
/// RabbitMQ message to trigger metrics calculation job.
/// Processed by DevPulse.Worker service.
/// </summary>
public class CalculateMetricsJob
{
    /// <summary>Unique job ID for tracking</summary>
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    /// <summary>Target user (nullable for all users)</summary>
    [JsonPropertyName("user_id")]
    public Guid? UserId { get; set; }

    /// <summary>Target repository (nullable for all repos)</summary>
    [JsonPropertyName("repository_id")]
    public Guid? RepositoryId { get; set; }

    /// <summary>Aggregation period type</summary>
    [JsonPropertyName("period_type")]
    public MetricPeriodType PeriodType { get; set; }

    /// <summary>Period start date (UTC)</summary>
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; set; }

    /// <summary>Period end date (UTC)</summary>
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; set; }

    /// <summary>When job was queued (UTC)</summary>
    [JsonPropertyName("queued_at")]
    public DateTime QueuedAt { get; set; }
}

/// <summary>
/// RabbitMQ message to index content in OpenSearch.
/// Processed asynchronously to avoid blocking API responses.
/// </summary>
public class IndexContentJob
{
    /// <summary>Unique job ID</summary>
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    /// <summary>Type of content to index</summary>
    [JsonPropertyName("content_type")]
    public IndexContentType ContentType { get; set; }

    /// <summary>Entity UUID (commit, PR, or comment)</summary>
    [JsonPropertyName("entity_id")]
    [Required]
    public Guid EntityId { get; set; }

    /// <summary>Queue timestamp (UTC)</summary>
    [JsonPropertyName("queued_at")]
    public DateTime QueuedAt { get; set; }
}

/// <summary>
/// Content types for OpenSearch indexing
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexContentType
{
    /// <summary>Index a commit document</summary>
    [JsonPropertyName("commit")]
    Commit = 0,

    /// <summary>Index a pull request document</summary>
    [JsonPropertyName("pull_request")]
    PullRequest = 1,

    /// <summary>Index a review comment document</summary>
    [JsonPropertyName("review_comment")]
    ReviewComment = 2
}

/// <summary>
/// RabbitMQ message to trigger Lambda image optimization.
/// Alternative to S3 event trigger for manual optimization.
/// </summary>
public class OptimizeImageJob
{
    /// <summary>Unique job ID</summary>
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    /// <summary>Media asset UUID</summary>
    [JsonPropertyName("media_asset_id")]
    [Required]
    public Guid MediaAssetId { get; set; }

    /// <summary>S3 object key</summary>
    [JsonPropertyName("s3_key")]
    [Required]
    public string S3Key { get; set; }

    /// <summary>Queue timestamp (UTC)</summary>
    [JsonPropertyName("queued_at")]
    public DateTime QueuedAt { get; set; }
}
```

---

## 7. API DTOs (Request/Response)

### 7.1 Webhook Receivers

```csharp
/// <summary>
/// Generic DTO for receiving webhook requests from Git providers.
/// Validates signature and deserializes provider-specific payloads.
/// </summary>
public class WebhookRequestDto
{
    /// <summary>Event type header (e.g., "push", "pull_request")</summary>
    [JsonPropertyName("event")]
    [Required]
    public string Event { get; set; }

    /// <summary>HMAC signature for validation</summary>
    [JsonPropertyName("signature")]
    [Required]
    public string Signature { get; set; }

    /// <summary>Raw JSON payload (provider-specific schema)</summary>
    [JsonPropertyName("payload")]
    [Required]
    public object Payload { get; set; }
}
```

### 7.2 API Responses

```csharp
/// <summary>
/// Repository analytics response DTO.
/// Aggregates key metrics and trends for dashboard display.
/// </summary>
public class RepositoryMetricsDto
{
    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name</summary>
    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; }

    /// <summary>Total commit count</summary>
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; }

    /// <summary>Total pull request count</summary>
    [JsonPropertyName("total_pull_requests")]
    public int TotalPullRequests { get; set; }

    /// <summary>Number of active contributors</summary>
    [JsonPropertyName("active_contributors")]
    public int ActiveContributors { get; set; }

    /// <summary>Overall code health score (0-100)</summary>
    [JsonPropertyName("code_health_score")]
    public decimal CodeHealthScore { get; set; }

    /// <summary>Top contributors list</summary>
    [JsonPropertyName("top_contributors")]
    public List<TopContributorDto> TopContributors { get; set; } = new();

    /// <summary>Commit trend over time</summary>
    [JsonPropertyName("commit_trend")]
    public List<MetricDataPointDto> CommitTrend { get; set; } = new();
}

/// <summary>
/// Top contributor summary for leaderboard display.
/// </summary>
public class TopContributorDto
{
    /// <summary>User UUID</summary>
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    /// <summary>Username</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }

    /// <summary>Total commits</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }

    /// <summary>Total pull requests</summary>
    [JsonPropertyName("pull_request_count")]
    public int PullRequestCount { get; set; }
}

/// <summary>
/// Time-series data point for charts.
/// </summary>
public class MetricDataPointDto
{
    /// <summary>Data point timestamp (UTC)</summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    /// <summary>Metric value</summary>
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

/// <summary>
/// Developer profile response DTO.
/// Contains user info, metrics, and repository list.
/// </summary>
public class DeveloperProfileDto
{
    /// <summary>User UUID</summary>
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    /// <summary>Username</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }

    /// <summary>Email address</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>Avatar URL (nullable)</summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>Aggregated developer metrics</summary>
    [JsonPropertyName("metrics")]
    public DeveloperMetricsDto Metrics { get; set; }

    /// <summary>List of repositories user contributes to</summary>
    [JsonPropertyName("repositories")]
    public List<RepositorySummaryDto> Repositories { get; set; } = new();
}

/// <summary>
/// Developer metrics summary.
/// </summary>
public class DeveloperMetricsDto
{
    /// <summary>Total commits across all repositories</summary>
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; }

    /// <summary>Total pull requests created</summary>
    [JsonPropertyName("total_pull_requests")]
    public int TotalPullRequests { get; set; }

    /// <summary>Number of code reviews performed</summary>
    [JsonPropertyName("code_reviews")]
    public int CodeReviews { get; set; }

    /// <summary>Average PR review time in hours</summary>
    [JsonPropertyName("average_review_time_hours")]
    public decimal AverageReviewTimeHours { get; set; }

    /// <summary>Productivity score (0-100)</summary>
    [JsonPropertyName("productivity_score")]
    public decimal ProductivityScore { get; set; }
}

/// <summary>
/// Repository summary for user profile.
/// </summary>
public class RepositorySummaryDto
{
    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository name</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>User's role in repository</summary>
    [JsonPropertyName("role")]
    public ContributorRole Role { get; set; }

    /// <summary>User's commit count in this repo</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }
}

/// <summary>
/// Generic paginated search results wrapper.
/// </summary>
public class SearchResultDto<T>
{
    /// <summary>Total number of hits</summary>
    [JsonPropertyName("total_hits")]
    public int TotalHits { get; set; }

    /// <summary>Current page number (1-indexed)</summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>Results per page</summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    /// <summary>Result items</summary>
    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = new();

    /// <summary>Search execution time in milliseconds</summary>
    [JsonPropertyName("search_time_ms")]
    public double SearchTimeMs { get; set; }
}

/// <summary>
/// Commit search result item with highlights.
/// </summary>
public class CommitSearchResultDto
{
    /// <summary>Commit SHA</summary>
    [JsonPropertyName("sha")]
    public string Sha { get; set; }

    /// <summary>Repository name</summary>
    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; }

    /// <summary>Author name</summary>
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; }

    /// <summary>Commit message</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>Commit timestamp (UTC)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>OpenSearch highlight snippets</summary>
    [JsonPropertyName("highlight_snippets")]
    public List<string> HighlightSnippets { get; set; } = new();
}
```

---

## 8. Configuration & Settings

```csharp
/// <summary>
/// PostgreSQL database connection settings.
/// Loaded from appsettings.json or environment variables.
/// </summary>
public class DatabaseSettings
{
    /// <summary>PostgreSQL connection string</summary>
    [JsonPropertyName("connection_string")]
    [Required]
    public string ConnectionString { get; set; }

    /// <summary>Max retry attempts for transient failures</summary>
    [JsonPropertyName("max_retry_attempts")]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Command timeout in seconds</summary>
    [JsonPropertyName("command_timeout_seconds")]
    public int CommandTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// RabbitMQ message broker configuration.
/// </summary>
public class RabbitMqSettings
{
    /// <summary>RabbitMQ hostname (K8s service name)</summary>
    [JsonPropertyName("host_name")]
    [Required]
    public string HostName { get; set; }

    /// <summary>RabbitMQ port</summary>
    [JsonPropertyName("port")]
    public int Port { get; set; } = 5672;

    /// <summary>Authentication username</summary>
    [JsonPropertyName("username")]
    [Required]
    public string Username { get; set; }

    /// <summary>Authentication password</summary>
    [JsonPropertyName("password")]
    [Required]
    public string Password { get; set; }

    /// <summary>Virtual host</summary>
    [JsonPropertyName("virtual_host")]
    public string VirtualHost { get; set; } = "/";

    /// <summary>Queue name configuration</summary>
    [JsonPropertyName("queues")]
    public QueueSettings Queues { get; set; } = new();
}

/// <summary>
/// RabbitMQ queue names.
/// </summary>
public class QueueSettings
{
    /// <summary>Queue for webhook events</summary>
    [JsonPropertyName("webhook_events")]
    public string WebhookEvents { get; set; } = "devpulse.webhook.events";

    /// <summary>Queue for metrics calculation jobs</summary>
    [JsonPropertyName("metrics_calculation")]
    public string MetricsCalculation { get; set; } = "devpulse.metrics.calculate";

    /// <summary>Queue for OpenSearch indexing jobs</summary>
    [JsonPropertyName("search_indexing")]
    public string SearchIndexing { get; set; } = "devpulse.search.index";

    /// <summary>Queue for image optimization jobs</summary>
    [JsonPropertyName("image_optimization")]
    public string ImageOptimization { get; set; } = "devpulse.media.optimize";
}

/// <summary>
/// OpenSearch cluster configuration.
/// </summary>
public class OpenSearchSettings
{
    /// <summary>OpenSearch cluster endpoint URL</summary>
    [JsonPropertyName("endpoint")]
    [Required]
    [Url]
    public string Endpoint { get; set; }

    /// <summary>Authentication username</summary>
    [JsonPropertyName("username")]
    [Required]
    public string Username { get; set; }

    /// <summary>Authentication password</summary>
    [JsonPropertyName("password")]
    [Required]
    public string Password { get; set; }

    /// <summary>Commits index name</summary>
    [JsonPropertyName("commits_index")]
    public string CommitsIndex { get; set; } = "devpulse-commits";

    /// <summary>Pull requests index name</summary>
    [JsonPropertyName("pull_requests_index")]
    public string PullRequestsIndex { get; set; } = "devpulse-pull-requests";

    /// <summary>Reviews index name</summary>
    [JsonPropertyName("reviews_index")]
    public string ReviewsIndex { get; set; } = "devpulse-reviews";
}

/// <summary>
/// AWS S3 storage configuration.
/// </summary>
public class S3Settings
{
    /// <summary>S3 bucket name</summary>
    [JsonPropertyName("bucket_name")]
    [Required]
    public string BucketName { get; set; }

    /// <summary>AWS region</summary>
    [JsonPropertyName("region")]
    [Required]
    public string Region { get; set; } = "us-east-1";

    /// <summary>AWS access key ID</summary>
    [JsonPropertyName("access_key_id")]
    [Required]
    public string AccessKeyId { get; set; }

    /// <summary>AWS secret access key</summary>
    [JsonPropertyName("secret_access_key")]
    [Required]
    public string SecretAccessKey { get; set; }

    /// <summary>CloudFront distribution URL (nullable)</summary>
    [JsonPropertyName("cloudfront_distribution")]
    [Url]
    public string? CloudFrontDistribution { get; set; }
}
```

---

## Type Summary Table

| Category | Entity Count | Primary Storage |
|----------|--------------|-----------------|
| **Core Domain** | 4 entities | PostgreSQL (RDS) |
| **Git Activity** | 5 entities | PostgreSQL (RDS) |
| **Analytics** | 3 entities | PostgreSQL (RDS) |
| **Media** | 1 entity | PostgreSQL (metadata), S3 (files) |
| **Search Documents** | 3 documents | OpenSearch |
| **Event Messages** | 6 message types | RabbitMQ (transient) |
| **API DTOs** | 10+ DTOs | In-memory |
| **Configuration** | 5 settings classes | appsettings.json |

---

**Total Entities:** 33 distinct types
**Database Tables (RDS):** 13 primary tables
**OpenSearch Indices:** 3 indices
**RabbitMQ Queues:** 4 queues

---

## Notes

1. All `DateTime` fields use **UTC timezone** for consistency.
2. All primary keys use **UUID v4** (Guid in C#) for distributed system compatibility.
3. JSON property names use **snake_case** for REST API consistency.
4. All entities include proper **data annotations** for validation.
5. Enums use **JsonStringEnumConverter** for human-readable JSON serialization.
6. Foreign keys follow naming convention: `{EntityName}Id`.
7. Nullable fields use **C# 11 nullable reference types** (`?` suffix).
8. All search documents include indexing optimization annotations.

---

**Generated by:** DevPulse Architecture Team
**Version:** 1.0.0
**Last Updated:** 2026-07-25
