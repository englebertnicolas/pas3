using Microsoft.Extensions.DependencyInjection;

namespace PAS.Mediator;

public sealed class Mediator(IServiceProvider serviceProvider) : IMediator {

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest {
        Guard.ThrowIfNull(request);

        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        return handler.HandleAsync(request, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) {
        Guard.ThrowIfNull(request);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod("HandleAsync");
        return (Task<TResponse>)method!.Invoke(handler, [request, cancellationToken])!;
    }
}
