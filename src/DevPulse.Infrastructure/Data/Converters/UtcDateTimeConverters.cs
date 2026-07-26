using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DevPulse.Infrastructure.Data.Converters;

/// <summary>
/// Normalises <see cref="DateTime"/> values to UTC on the way to PostgreSQL and
/// tags them as UTC on the way back.
/// </summary>
/// <remarks>
/// Every timestamp column in this schema is <c>timestamp with time zone</c>, and
/// Npgsql refuses to write a <see cref="DateTime"/> whose
/// <see cref="DateTime.Kind"/> is not <see cref="DateTimeKind.Utc"/> to such a
/// column. Values deserialised from webhook payloads frequently arrive as
/// <see cref="DateTimeKind.Unspecified"/>, so without this converter the first
/// real webhook would throw.
///
/// <see cref="DateTimeKind.Unspecified"/> is treated as already-UTC rather than
/// as local time. Git timestamps are absolute instants, and
/// <c>ToUniversalTime()</c> on an Unspecified value would silently shift it by
/// the host's offset — harmless in a UTC container, wrong on a developer machine.
/// </remarks>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    /// <summary>Initialises the converter.</summary>
    public UtcDateTimeConverter()
        : base(
            v => ToUtc(v),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }

    internal static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}

/// <summary>
/// Nullable counterpart of <see cref="UtcDateTimeConverter"/>.
/// </summary>
public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    /// <summary>Initialises the converter.</summary>
    public UtcNullableDateTimeConverter()
        : base(
            v => v.HasValue ? UtcDateTimeConverter.ToUtc(v.Value) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
