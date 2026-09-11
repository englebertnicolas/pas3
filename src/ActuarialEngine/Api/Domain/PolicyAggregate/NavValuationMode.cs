using System.Text.Json.Serialization;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NavValuationMode { Backward, Forward }
