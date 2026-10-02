namespace PAS.Bff;

public record BffOptions
{
    public const string SectionName = "Bff";

    public Dictionary<string, string> Apis { get; init; } = [];
}
