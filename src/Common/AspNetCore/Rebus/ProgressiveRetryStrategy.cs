using Rebus.Bus;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Transport;

namespace PAS.AspNetCore.Rebus;

internal class ProgressiveRetryStrategy(IErrorHandler innerErrorHandler, IBus bus) : IErrorHandler {
    private static readonly TimeSpan[] RetryDelays = [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(23)
    ];

    public async Task HandlePoisonMessage(TransportMessage transportMessage, ITransactionContext transactionContext, ExceptionInfo exception) {
        if (ShouldRetry(exception)) {
            var headers = transportMessage.Headers;
            int deferCount = headers.TryGetValue(Headers.DeferCount, out var result) ? int.Parse(result) : 0;

            if (deferCount < RetryDelays.Length) {
                var delayForNextAttempt = RetryDelays[deferCount];

                // Asking Rebus to resend the message in the queue after the delayForNextAttempt
                await bus.Advanced.TransportMessage.Defer(delayForNextAttempt, headers);
                return;
            }
        }

        // Asking Rebus to send the message to the Dead Letter Queue
        await innerErrorHandler.HandlePoisonMessage(transportMessage, transactionContext, exception);
    }

    private static bool ShouldRetry(ExceptionInfo exception) {
        return exception.Type.EndsWith("TimeoutException") ||
            exception.Type.EndsWith("TaskCanceledException") ||
            exception.Type.EndsWith("HttpRequestException") ||
            exception.Type.EndsWith("DbUpdateConcurrencyException");
    }
}
