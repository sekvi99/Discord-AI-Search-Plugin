using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Application.Queries.ExplainMessages;

public record ExplainMessagesQuery(
    GuildId GuildId,
    string Query) : IRequest<ExplainMessagesResult>;

public record ExplainMessagesResult(
    bool Success,
    string? AIExplanation = null,
    string? ErrorMessage = null);