using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NavValuationMode { Backward, Forward }
