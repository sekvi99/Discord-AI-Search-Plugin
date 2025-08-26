namespace DiscordAIBot.Application.Common.Messaging;

/// <summary>
/// Represents a request that returns a response
/// </summary>
/// <typeparam name="TResponse">The type of response returned</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Represents a request without a response
/// </summary>
public interface IRequest
{
}