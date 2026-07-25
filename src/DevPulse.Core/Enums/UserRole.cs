namespace DevPulse.Core.Enums;

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
