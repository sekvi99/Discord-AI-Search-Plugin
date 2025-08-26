using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Application.Queries.SearchMessages;

public record SearchMessagesQuery(
    GuildId GuildId,
    string Query,
    ChannelId? ChannelId = null,
    UserId? UserId = null,
    bool UseAIEnhancement = false) : IRequest<SearchMessagesResult>;

public record SearchMessagesResult(
    bool Success,
    IEnumerable<SearchResult> Results,
    string? AIEnhancedSummary = null,
    string? ErrorMessage = null);