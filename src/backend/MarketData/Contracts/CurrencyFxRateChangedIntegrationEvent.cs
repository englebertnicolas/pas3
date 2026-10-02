using PAS.Core.Amqp;

namespace PAS.MarketData.Contracts;

[Topic("PAS.MarketData.CurrencyRateChanged")]
public record CurrencyFxRateChangedIntegrationEvent(
    string CurrencyId,
    DateOnly Date,
    decimal? OldRateToEur,
    decimal NewRateToEur
) : IIntegrationEvent;
