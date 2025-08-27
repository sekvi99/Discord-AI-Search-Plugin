using Discord.WebSocket;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Application.Interfaces.Services;

public interface IDiscordSearchService
{
    SocketGuild? GetDiscordGuild(GuildId guildId);
    Task<IEnumerable<SearchResult>> SearchAcrossGuildAsync(GuildId guildId, string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResult>> SearchInChannelAsync(ChannelId channelId, string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResult>> SearchByUserAsync(GuildId guildId, UserId userId, string? query = null, CancellationToken cancellationToken = default);
}