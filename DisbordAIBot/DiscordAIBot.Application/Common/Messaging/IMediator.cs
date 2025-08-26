namespace DiscordAIBot.Application.Common.Messaging;

/// <summary>
/// Defines a mediator to encapsulate request/response and publishing interaction patterns
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send a request and get a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response object</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a request without expecting a response
    /// </summary>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}