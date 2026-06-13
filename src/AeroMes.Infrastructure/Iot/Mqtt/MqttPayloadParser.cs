using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Iot.Mqtt;

public static class MqttPayloadParser
{
    /// <summary>
    /// Parses an MQTT payload string into a (value, timestamp) tuple.
    /// Returns (0, UtcNow) on any parse failure — callers should treat that as a discard.
    /// </summary>
    public static (decimal value, DateTimeOffset timestamp) Parse(
        string payload, MqttAdapterConfig config, ILogger logger)
    {
        try
        {
            return config.PayloadFormat.ToUpperInvariant() switch
            {
                "PLAIN_NUMBER" => ParsePlainNumber(payload),
                "CSV"          => ParseCsv(payload, config, logger),
                _              => ParseJson(payload, config, logger),
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse MQTT payload '{Payload}' with format '{Format}'",
                payload, config.PayloadFormat);
            return (0, DateTimeOffset.UtcNow);
        }
    }

    // ── Plain number ──────────────────────────────────────────────────────

    private static (decimal, DateTimeOffset) ParsePlainNumber(string payload)
    {
        var value = decimal.Parse(payload.Trim(), System.Globalization.CultureInfo.InvariantCulture);
        return (value, DateTimeOffset.UtcNow);
    }

    // ── CSV ───────────────────────────────────────────────────────────────

    private static (decimal, DateTimeOffset) ParseCsv(string payload, MqttAdapterConfig config, ILogger logger)
    {
        var parts = payload.Split(',');
        if (parts.Length < 1)
        {
            logger.LogWarning("CSV payload has no parts: '{Payload}'", payload);
            return (0, DateTimeOffset.UtcNow);
        }

        var value = decimal.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
        var timestamp = parts.Length > 1
            ? ParseTimestamp(parts[1].Trim(), config.TimestampFormat, logger)
            : DateTimeOffset.UtcNow;

        return (value, timestamp);
    }

    // ── JSON ──────────────────────────────────────────────────────────────

    private static (decimal, DateTimeOffset) ParseJson(string payload, MqttAdapterConfig config, ILogger logger)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var value = GetJsonDecimal(root, config.JsonValuePath, logger);

        var timestamp = config.JsonTimestampPath is { Length: > 0 }
            ? GetJsonTimestamp(root, config.JsonTimestampPath, config.TimestampFormat, logger)
            : DateTimeOffset.UtcNow;

        return (value, timestamp);
    }

    private static decimal GetJsonDecimal(JsonElement root, string path, ILogger logger)
    {
        var prop = StripJsonPathPrefix(path);
        if (!root.TryGetProperty(prop, out var el))
        {
            logger.LogWarning("JSON property '{Prop}' not found in payload", prop);
            return 0;
        }

        return el.ValueKind switch
        {
            JsonValueKind.Number => el.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(
                el.GetString(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) => d,
            _ => 0
        };
    }

    private static DateTimeOffset GetJsonTimestamp(
        JsonElement root, string path, string format, ILogger logger)
    {
        var prop = StripJsonPathPrefix(path);
        if (!root.TryGetProperty(prop, out var el))
            return DateTimeOffset.UtcNow;

        var raw = el.ValueKind switch
        {
            JsonValueKind.Number => el.GetRawText(),
            JsonValueKind.String => el.GetString() ?? string.Empty,
            _ => string.Empty
        };

        return ParseTimestamp(raw, format, logger);
    }

    // ── Timestamp helpers ─────────────────────────────────────────────────

    private static DateTimeOffset ParseTimestamp(string raw, string format, ILogger logger)
    {
        try
        {
            return format.ToLowerInvariant() switch
            {
                "unix_ms"  => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(raw)),
                "unix_s"   => DateTimeOffset.FromUnixTimeSeconds(long.Parse(raw)),
                "iso8601"  => DateTimeOffset.Parse(raw),
                _          => DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse timestamp '{Raw}' with format '{Format}'", raw, format);
            return DateTimeOffset.UtcNow;
        }
    }

    private static string StripJsonPathPrefix(string path)
    {
        // "$.value" → "value",  "value" → "value"
        if (path.StartsWith("$."))
            return path[2..];
        if (path.StartsWith("$"))
            return path[1..];
        return path;
    }
}
