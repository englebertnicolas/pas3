using PAS.Domain.Ids;

namespace PAS.PolicyAdmin.Domain.PolicyAggregate;

public record FundAllocation(FundId FundId, decimal Ratio);
