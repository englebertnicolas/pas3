using PAS.Core.Amqp;

namespace PAS.Assets.Contracts;

[Topic("PAS.Assets.CurrencyExchangeRateChanged")]
public record CurrencyExchangeRateChangedIntegrationEvent(
    string BaseCurrencyId,
    string QuoteCurrencyId,
    DateOnly Date,
    decimal? OldRate,
    decimal NewRate
) : IIntegrationEvent;
