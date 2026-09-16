using System.Text.Json.Serialization;
using PAS.Domain;
using PAS.MarketData.Domain.CurrencyAggregate;

namespace PAS.MarketData.Domain.FundAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundType { Collective, Dedicated }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundStatus { Active, Suspended, Closed }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundValuationPeriodicity { Daily, Weekly, Monthly }

public class Fund : Entity<FundId>, IAggregateRoot {
    public FundType Type { get; private set; }
    public FundStatus Status { get; private set; }
    public string Name { get; private set; } = null!;
    public Isin Isin { get; private set; } = null!;
    public CurrencyId CurrencyId { get; private set; }
    public FundValuationPeriodicity ValuationPeriodicity { get; private set; }

    public int UnitDecimals { get; private set; }
    public int NavDecimals { get; private set; }
    public int NavPricingLag { get; private set; }
    public int NavStalenessTolerance { get; private set; }

    private readonly List<FundNav> navs = [];
    public IReadOnlyCollection<FundNav> Navs => navs.AsReadOnly();

    private Fund() {
        // For EF hydration
    }

    private Fund(FundId id, FundType type, FundStatus status, string name, Isin isin, CurrencyId currencyId,
        FundValuationPeriodicity valuationPeriodicity, int unitDecimals, int navDecimals, int navPricingLag, int navStalenessTolerance
    ) {
        Id = id;
        Type = type;
        Status = status;
        Name = name;
        Isin = isin;
        CurrencyId = currencyId;
        ValuationPeriodicity = valuationPeriodicity;
        UnitDecimals = unitDecimals;
        NavDecimals = navDecimals;
        NavPricingLag = navPricingLag;
        NavStalenessTolerance = navStalenessTolerance;
    }

    public static ErrorOr<Fund> CreateCollectiveFund(FundId? id, FundStatus status, string name, string isin, CurrencyId currencyId,
        FundValuationPeriodicity valuationPeriodicity, int unitDecimals = 0, int navDecimals = 0, int navPricingLag = 0, int navStalenessTolerance = 0
    ) {
        if (id.HasValue && id.Value.Value == Guid.Empty)
            return ErrorInfo.Unprocessable("Invalid fund ID.");

        var eoIsin = Isin.Create(isin);
        if (eoIsin.IsFailure)
            return eoIsin.Errors;

        if (string.IsNullOrWhiteSpace(name))
            return ErrorInfo.Unprocessable("Invalid fund name.");

        if (unitDecimals < 0 || unitDecimals > 10)
            return ErrorInfo.Unprocessable("Unit decimal places is out of acceptable range.");

        if (navDecimals < 0 || navDecimals > 10)
            return ErrorInfo.Unprocessable("NAV decimal places is out of acceptable range.");

        if (navStalenessTolerance < 0)
            return ErrorInfo.Unprocessable("Invalid NAV staleness tolerance.");

        return new Fund(id ?? FundId.New(), FundType.Collective, status, name, eoIsin.Value, currencyId, valuationPeriodicity, navDecimals, navDecimals, navPricingLag, navStalenessTolerance);
    }

    public static ErrorOr<Fund> CreateDedicatedFund(Guid? id, FundStatus status, string name, string isin, string currencyId,
        FundValuationPeriodicity valuationPeriodicity
    ) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add or update a fund NAV at the given date.
    /// </summary>
    /// <remarks>
    /// Precondition: The Fund aggregate should be loaded with navs filtered 
    /// to include at least the nav at the given date (if it exists).
    /// </remarks>
    public ErrorOr<UpsertResult> UpsertNav(DateOnly navDate, decimal navValue) {
        var eoNav = FundNav.Create(navDate, navValue, NavDecimals);
        if (eoNav.IsFailure) return eoNav.Errors;
        var nav = eoNav.Value;

        var existingNav = navs.FirstOrDefault(v => v.Date == nav.Date);
        if (existingNav != null) {
            if (existingNav.Value == nav.Value) return UpsertResult.Unchanged;
            navs.Remove(existingNav);
        }

        navs.Add(nav);
        AddDomainEvent(new FundNavChangedDomainEvent(Id.Value, nav.Date, existingNav?.Value, nav.Value));
        return existingNav == null ? UpsertResult.Created : UpsertResult.Updated;
    }

    public ErrorOr<Success> Close() {
        if (Status == FundStatus.Closed)
            return ErrorInfo.Unprocessable($"Cannot change fund status from '{Status}' to '{FundStatus.Closed}'.");

        AddDomainEvent(new FundClosedDomainEvent(Id.Value));
        return Success.Value;
    }
}
