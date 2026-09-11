using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Retry.Simple;

namespace PAS.Rebus.Internal;

internal class DeferredErrorHandler(IBus bus) : IHandleMessages<IFailed<object>> {
    private readonly int[] DelaysInSeconds = [10, 30, 60, 180];

    public async Task Handle(IFailed<object> failedMessage) {
        var exceptionInfo = failedMessage.Exceptions.FirstOrDefault();
        var exceptionType = exceptionInfo?.Type ?? "Unknown type";
        var exceptionMessage = exceptionInfo?.Message ?? "Unknown error";
        var failFast = !(exceptionInfo is ExceptionInfoExtended ei && ei.IsTransient);

        if (failFast) {
            await bus.Advanced.TransportMessage.Deadletter($"Message permanently failed because FailFast policy triggered on exception '{exceptionType}'. Original error: {exceptionMessage}");
            return;
        }

        // Get the number of deferred retries already performed from the native Rebus header
        int currentAttempt = 0;
        if (failedMessage.Headers.TryGetValue(Headers.DeferCount, out var deferCountStr)) {
            _ = int.TryParse(deferCountStr, out currentAttempt);
        }

        if (currentAttempt < DelaysInSeconds.Length) {
            // If second-level retries are not exhausted yet,
            // defer the original message into the future
            int delay = DelaysInSeconds[currentAttempt];
            failedMessage.Headers["x-delay"] = (delay * 1000).ToString(); // Necessary for the RabbitMQ's Delayed Message Exchange Plugin
            await bus.DeferLocal(TimeSpan.FromSeconds(delay), failedMessage.Message, failedMessage.Headers);

        } else {
            await bus.Advanced.TransportMessage.Deadletter($"Message permanently failed after {DelaysInSeconds.Length} second-level retry attempts. Original error: {exceptionMessage}");
        }
    }
}
