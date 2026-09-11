using System.Text.Json.Serialization;

namespace PAS.Policies.Domain.PolicyAggregate;

//public record FundAllocation {
//    [JsonInclude] public IReadOnlyCollection<FundAllocationItem> Items { get; private set; } = null!;

//    [JsonConstructor] private FundAllocation() { }

//    private FundAllocation(IEnumerable<FundAllocationItem> items) {
//        Items = [.. items];
//    }

//    public static ErrorOr<FundAllocation> Create(IEnumerable<FundAllocationItem> items) {
//        var totalRatio = items.Sum(x => x.Ratio);
//        if (Math.Abs(totalRatio - 1.0M) > 0.0001M)
//            return ErrorInfo.Unprocessable("Invalid fund allocation.");

//        return new FundAllocation(items);
//    }
//}

//public record FundAllocation {
//    [JsonInclude] public IReadOnlyCollection<FundAllocationItem> Items { get; private set; } = null!;

//    [JsonConstructor] private FundAllocation() { }

//    private FundAllocation(IEnumerable<FundAllocationItem> items) {
//        Items = [.. items];
//    }

//    public static ErrorOr<FundAllocation> Create(IEnumerable<FundAllocationItem> items) {
//        var totalRatio = items.Sum(x => x.Ratio);
//        if (Math.Abs(totalRatio - 1.0M) > 0.0001M)
//            return ErrorInfo.Unprocessable("Invalid fund allocation.");

//        return new FundAllocation(items);
//    }
//}

//public record FundAllocationItem(FundId FundId, decimal Ratio);

public record FundAllocation(FundId FundId, decimal Ratio);
