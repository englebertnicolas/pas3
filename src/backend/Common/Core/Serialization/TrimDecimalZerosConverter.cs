using System.Text.Json;
using System.Text.Json.Serialization;

namespace PAS.Core.Serialization;

public class TrimDecimalZerosConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Normalize());
    }
}
