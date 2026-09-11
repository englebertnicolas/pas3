namespace PAS.Mediator;

public interface IRequestHandler;

public interface IRequestHandler<TRequest> : IRequestHandler where TRequest : IRequest {
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<TRequest, TResponse> : IRequestHandler where TRequest : IRequest<TResponse> {
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
