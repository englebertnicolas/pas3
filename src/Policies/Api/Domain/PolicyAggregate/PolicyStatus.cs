using System.Text.Json.Serialization;

namespace PAS.Policies.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PolicyStatus { Pending, Issued }
