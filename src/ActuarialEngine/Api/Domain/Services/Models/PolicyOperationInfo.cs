using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Domain.Services.Models;

public interface IPolicySingleOperationInfo {
    PolicyOperationId Id { get; }
    DateOnly Date { get; }
    int DailySeq { get; }
}

public interface IPolicyRecurringOperationInfo {
    PolicyOperationId Id { get; }
    DateOnly StartDate { get; }
    DateOnly? EndDate { get; }
    //TODO: Périodicité + Anticipatif/terme échu
}

public abstract record PolicyOperationInfo(PolicyOperationId Id) {
    public abstract IEnumerable<FundId> GetDistinctFundIds();
}

public record PremiumOperationInfo(
    PolicyOperationId Id,
    DateOnly Date,
    int DailySeq,
    decimal Amount,
    CurrencyId CurrencyId,
    FundAllocationItemInfo[] Allocations
) : PolicyOperationInfo(Id), IPolicySingleOperationInfo {
    public override IEnumerable<FundId> GetDistinctFundIds() => Allocations.Select(x => x.FundId);
}

public record FullSurrenderOperationInfo(
    PolicyOperationId Id,
    DateOnly Date,
    int DailySeq
) : PolicyOperationInfo(Id), IPolicySingleOperationInfo {
    public override IEnumerable<FundId> GetDistinctFundIds() => [];
}

public record FundAllocationItemInfo(FundId FundId, decimal Ratio);
