using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Application.Commands.SetApiKey;

public record SetApiKeyCommand(GuildId GuildId, string ApiKey) : IRequest<SetApiKeyResult>;

public record SetApiKeyResult(bool Success, string Message);