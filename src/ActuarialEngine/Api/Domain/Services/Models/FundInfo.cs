using PAS.ActuarialEngine.Domain.AssetViews;

namespace PAS.ActuarialEngine.Domain.Services.Models;

public record FundInfo(
    FundId Id,
    CurrencyId CurrencyId,
    FundNavInfo[] Navs,
    int UnitDecimals,
    int NavPricingLag,
    int NavStalenessTolerance
);

public record FundNavInfo(DateOnly Date, decimal Value);