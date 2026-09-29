namespace PAS.PolicyValuation;

public record PolicyValuationOptions
{
    public const string SectionName = "PolicyValuation";

    public string WorkerCron { get; init; } = "";
}
