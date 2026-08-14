using PAS.Assets.Contracts;
using Rebus.Handlers;

namespace PAS.ActuarialEngine.Features.RetroactiveChanges;

public partial class EvaluateFundNavChange(
    ILogger<EvaluateFundNavChange> logger
) : IHandleMessages<FundNavChangedIntegrationEvent> {

    public Task Handle(FundNavChangedIntegrationEvent message) {
        LogEventReceived(logger, message.FundId, message.Date, message.OldValue, message.NewValue);

        // TODO: rechercher les polices concernées par le changement de NAV
        // et planifier un éventuel recalcul de la valorisation de ces polices.

        // Remarque: le cancellationToken peut être obtenu en faisant :
        // -> MessageContext.Current?.GetCancellationToken() ?? CancellationToken.None;

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Received FundNavChangedIntegrationEvent for FundId: {FundId}, Date: {Date}, OldValue: {OldValue}, NewValue: {NewValue}")]
    public static partial void LogEventReceived(ILogger logger, Guid fundId, DateTime date, double? oldValue, double newValue);
}
