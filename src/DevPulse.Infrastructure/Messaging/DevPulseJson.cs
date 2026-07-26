using System.Text.Json;

namespace DevPulse.Infrastructure.Messaging;

/// <summary>
/// Serialiser settings shared by the publisher and every consumer.
/// </summary>
/// <remarks>
/// Both sides must agree, or the consumer silently deserialises a message into an
/// object with default values. The message contracts carry
/// <c>[JsonPropertyName]</c> attributes, so no naming policy is configured here —
/// setting one would override them.
/// </remarks>
internal static class DevPulseJson
{
    /// <summary>Shared options instance.</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };
}
