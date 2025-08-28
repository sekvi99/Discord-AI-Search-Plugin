using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DiscordAIBot.Application.Queries.SearchMessages;

public class SearchMessagesQueryHandler : IRequestHandler<SearchMessagesQuery, SearchMessagesResult>
{
    private readonly IGuildRepository _guildRepository;
    private readonly IDiscordSearchService _searchService;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<SearchMessagesQueryHandler> _logger;

    public SearchMessagesQueryHandler(
        IGuildRepository guildRepository,
        IDiscordSearchService searchService,
        IOpenAIService openAIService,
        ILogger<SearchMessagesQueryHandler> logger)
    {
        _guildRepository = guildRepository;
        _searchService = searchService;
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<SearchMessagesResult> Handle(SearchMessagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
                // Try to get the guild from the repository
                var guild = await _guildRepository.GetByIdAsync(request.GuildId, cancellationToken);
                if (guild == null)
                {
                    // If not found, create and add it using the Discord API
                    // You may need to pass the guild name in the request, or fetch it from Discord
                    var discordGuild = _searchService.GetDiscordGuild(request.GuildId);
                    var guildName = discordGuild?.Name ?? "Unknown";
                    guild = new Guild(request.GuildId, guildName);
                    await _guildRepository.AddAsync(guild, cancellationToken);
                }

            IEnumerable<SearchResult> results;

            if (request.ChannelId is not null)
            {
                results = await _searchService.SearchInChannelAsync(request.ChannelId.Value, request.Query, cancellationToken);
            }
            else if (request.UserId is not null)
            {
                results = await _searchService.SearchByUserAsync(request.GuildId, request.UserId.Value, request.Query, cancellationToken);
            }
            else
            {
                results = await _searchService.SearchAcrossGuildAsync(request.GuildId, request.Query, cancellationToken);
            }

            string? aiSummary = null;
            if (request.UseAIEnhancement && guild.HasValidApiKey())
            {
                var searchContents = results.Take(10).Select(r => r.Content).ToList();
                if (searchContents.Any())
                {
                    aiSummary = await _openAIService.EnhanceSearchResultsAsync(guild.ApiKey!, searchContents, request.Query, cancellationToken);
                }
            }
            else if (request.UseAIEnhancement && !guild.HasValidApiKey())
            {
                return new SearchMessagesResult(false, results,
                    ErrorMessage: "AI enhancement requires an OpenAI API key. Use the `!setapikey` command to configure it.");
            }

            return new SearchMessagesResult(true, results, aiSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages for guild {GuildId}", request.GuildId);
            return new SearchMessagesResult(false, Enumerable.Empty<SearchResult>(),
                ErrorMessage: "An error occurred while searching. Please try again later.");
        }
    }
}
