using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace DiscordAIBot.Infrastructure.Services;

public class DiscordSearchService : IDiscordSearchService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly ILogger<DiscordSearchService> _logger;

    public DiscordSearchService(DiscordSocketClient discordClient, ILogger<DiscordSearchService> logger)
    {
        _discordClient = discordClient;
        _logger = logger;
    }

    public async Task<IEnumerable<SearchResult>> SearchAcrossGuildAsync(GuildId guildId, string query, CancellationToken cancellationToken = default)
    {
        var guild = _discordClient.GetGuild(guildId);
        if (guild == null) return Enumerable.Empty<SearchResult>();

        var results = new List<SearchResult>();
        var searchTerms = ExtractSearchTerms(query);

        foreach (var channel in guild.TextChannels)
        {
            if (!HasReadPermission(channel)) continue;

            try
            {
                var channelResults = await SearchInChannelInternalAsync(channel, searchTerms, cancellationToken);
                results.AddRange(channelResults);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching channel {ChannelId} in guild {GuildId}", channel.Id, guildId);
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).ThenByDescending(r => r.Timestamp);
    }

    public async Task<IEnumerable<SearchResult>> SearchInChannelAsync(ChannelId channelId, string query, CancellationToken cancellationToken = default)
    {
        var channel = _discordClient.GetChannel(channelId) as ITextChannel;
        if (channel == null) return Enumerable.Empty<SearchResult>();

        var searchTerms = ExtractSearchTerms(query);
        return await SearchInChannelInternalAsync(channel, searchTerms, cancellationToken);
    }

    public async Task<IEnumerable<SearchResult>> SearchByUserAsync(GuildId guildId, UserId userId, string? query = null, CancellationToken cancellationToken = default)
    {
        var guild = _discordClient.GetGuild(guildId);
        if (guild == null) return Enumerable.Empty<SearchResult>();

        var results = new List<SearchResult>();
        var searchTerms = query != null ? ExtractSearchTerms(query) : new List<string>();

        foreach (var channel in guild.TextChannels)
        {
            if (!HasReadPermission(channel)) continue;

            try
            {
                var messages = await channel.GetMessagesAsync(1000).FlattenAsync();
                foreach (var message in messages)
                {
                    if (message.Author.Id != userId) continue;
                    if (cancellationToken.IsCancellationRequested) break;

                    double relevanceScore = 1.0;
                    if (query != null)
                    {
                        relevanceScore = CalculateRelevance(message.Content, searchTerms);
                        if (relevanceScore == 0) continue;
                    }

                    results.Add(CreateSearchResult(message, channel, relevanceScore));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching user messages in channel {ChannelId}", channel.Id);
            }
        }

        return results.OrderByDescending(r => r.Timestamp);
    }

    private async Task<IEnumerable<SearchResult>> SearchInChannelInternalAsync(
        ITextChannel channel, 
        List<string> searchTerms, 
        CancellationToken cancellationToken)
    {
        var results = new List<SearchResult>();
        var messages = await channel.GetMessagesAsync(1000).FlattenAsync();

        foreach (var message in messages)
        {
            if (message.Author.IsBot) continue;
            if (cancellationToken.IsCancellationRequested) break;

            var relevanceScore = CalculateRelevance(message.Content, searchTerms);
            if (relevanceScore > 0)
            {
                results.Add(CreateSearchResult(message, channel, relevanceScore));
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore);
    }

    private SearchResult CreateSearchResult(IMessage message, IMessageChannel channel, double relevanceScore)
    {
        return new SearchResult(
            new MessageId(message.Id),
            new ChannelId(channel.Id),
            new UserId(message.Author.Id),
            message.Author.Username,
            message.Content,
            channel.Name,
            GetMessageUrl(message),
            message.Timestamp.DateTime,
            relevanceScore);
    }

    private List<string> ExtractSearchTerms(string query)
    {
        return query.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length > 2)
            .Distinct()
            .ToList();
    }

    private double CalculateRelevance(string content, List<string> searchTerms)
    {
        if (string.IsNullOrWhiteSpace(content) || !searchTerms.Any()) return 0;

        var lowerContent = content.ToLower();
        double score = 0;
        int matchedTerms = 0;

        foreach (var term in searchTerms)
        {
            int occurrences = Regex.Matches(lowerContent, Regex.Escape(term)).Count;
            if (occurrences > 0)
            {
                score += occurrences * (term.Length / 10.0);
                matchedTerms++;
            }
        }

        if (matchedTerms > 1)
        {
            score *= 1.0 + (matchedTerms - 1) * 0.3;
        }

        if (searchTerms.Count > 1)
        {
            var phrase = string.Join(" ", searchTerms);
            if (lowerContent.Contains(phrase))
            {
                score *= 2.0;
            }
        }

        return score;
    }

    private bool HasReadPermission(SocketTextChannel channel)
    {
        var botUser = channel.Guild.GetUser(_discordClient.CurrentUser.Id);
        return botUser.GetPermissions(channel).ReadMessageHistory;
    }

    private string GetMessageUrl(IMessage message)
    {
        return $"https://discord.com/channels/{((IGuildChannel)message.Channel).GuildId}/{message.Channel.Id}/{message.Id}";
    }
}