using Microsoft.Extensions.Logging;

namespace DiscordAIBot.Application.Common.Messaging;

/// <summary>
/// Default implementation of the mediator pattern
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    /// <summary>
    /// Initializes a new instance of the Mediator class
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency resolution</param>
    /// <param name="logger">Logger instance</param>
    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            _logger.LogError("No handler found for request type {RequestType}", requestType.Name);
            throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
        }

        _logger.LogDebug("Handling request {RequestType}", requestType.Name);

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));
        var result = await (Task<TResponse>)method!.Invoke(handler, [request, cancellationToken])!;

        _logger.LogDebug("Request {RequestType} handled successfully", requestType.Name);
        return result;
    }

    /// <inheritdoc />
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            _logger.LogError("No handler found for request type {RequestType}", requestType.Name);
            throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
        }

        _logger.LogDebug("Handling request {RequestType}", requestType.Name);

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest>.Handle));
        await (Task)method!.Invoke(handler, [request, cancellationToken])!;

        _logger.LogDebug("Request {RequestType} handled successfully", requestType.Name);
    }
}
