using PAS.Core.Amqp;

namespace PAS.MarketData.Contracts;

[Topic("PAS.MarketData.CurrencyRateChanged")]
public record CurrencyRateChangedIntegrationEvent(
    string BaseCurrencyId,
    string QuoteCurrencyId,
    DateOnly Date,
    decimal? OldRate,
    decimal NewRate
) : IIntegrationEvent;
