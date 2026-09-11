using System.Text;
using RabbitMQ.Client;

namespace PAS.Rebus.Dlq;

internal static class RabbitMqHeaderExtensions {

    /// <summary>
    /// Safely retrieves an integer value from RabbitMQ headers, 
    /// handling both native integers and AMQP longstr byte arrays.
    /// </summary>
    public static int GetIntOrDefault(this IDictionary<string, object?> headers, string key, int defaultValue = 0) {
        if (headers == null || !headers.TryGetValue(key, out var value) || value == null) return defaultValue;
        if (value is int intValue) return intValue;

        if (value is byte[] bytes) {
            var rawString = Encoding.UTF8.GetString(bytes);
            return int.TryParse(rawString, out var parsedInt) ? parsedInt : defaultValue;
        }

        if (value is string strValue) {
            return int.TryParse(strValue, out var parsedInt) ? parsedInt : defaultValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely retrieves a date value from RabbitMQ headers, 
    /// handling AmqpTimestamp structures, strings, and AMQP longstr byte arrays.
    /// </summary>
    public static DateTime? GetDateTimeOrNull(this IDictionary<string, object?> headers, string key) {
        if (headers == null || !headers.TryGetValue(key, out var value) || value == null) return null;

        if (value is AmqpTimestamp amqpTimestamp) {
            // AmqpTimestamp.UnixTime represents the number of seconds since 1970-01-01T00:00:00Z
            return DateTimeOffset.FromUnixTimeSeconds(amqpTimestamp.UnixTime).UtcDateTime;
        }

        if (value is DateTime dateValue) return dateValue;

        if (value is byte[] bytes) {
            var rawString = Encoding.UTF8.GetString(bytes);
            return DateTime.TryParse(rawString, out var parsedDate) ? parsedDate : null;
        }

        if (value is string strValue) {
            return DateTime.TryParse(strValue, out var parsedDate) ? parsedDate : null;
        }

        return null;
    }
}
