using Microsoft.Data.SqlClient;

namespace SqlService.Infrastructure.Extensions;

/// <summary>
/// SqlDataReader extension methods
/// AOT-Compatible: No reflection
/// </summary>
public static class SqlDataReaderExtensions
{
    /// <summary>
    /// Kolon var mı kontrol eder
    /// </summary>
    public static bool HasColumn(this SqlDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        try
        {
            reader.GetOrdinal(columnName);
            return true;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// Nullable değer okur
    /// </summary>
    public static T? GetNullableValue<T>(this SqlDataReader reader, string columnName) where T : struct
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        return (T)reader.GetValue(ordinal);
    }

    /// <summary>
    /// String değer okur (null safe)
    /// </summary>
    public static string? GetStringOrNull(this SqlDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        return reader.GetString(ordinal);
    }

    /// <summary>
    /// Int değer okur (null safe)
    /// </summary>
    public static int? GetInt32OrNull(this SqlDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        return reader.GetInt32(ordinal);
    }

    /// <summary>
    /// DateTime değer okur (null safe)
    /// </summary>
    public static DateTime? GetDateTimeOrNull(this SqlDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        return reader.GetDateTime(ordinal);
    }

    /// <summary>
    /// Guid değer okur (null safe)
    /// </summary>
    public static Guid? GetGuidOrNull(this SqlDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        return reader.GetGuid(ordinal);
    }

    /// <summary>
    /// Bool değer okur (null safe)
    /// </summary>
    public static bool? GetBooleanOrNull(this SqlDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return null;

        return reader.GetBoolean(ordinal);
    }
}
