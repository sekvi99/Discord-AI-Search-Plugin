using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DiscordAIBot.Application.Queries.ExplainMessages;

public class ExplainMessageQueryHandler : IRequestHandler<ExplainMessagesQuery, ExplainMessagesResult>
{
    private readonly IGuildRepository _guildRepository;
    private readonly IDiscordSearchService _searchService;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ExplainMessageQueryHandler> _logger;

    public ExplainMessageQueryHandler(
        IGuildRepository guildRepository,
        IDiscordSearchService searchService,
        IOpenAIService openAIService,
        ILogger<ExplainMessageQueryHandler> logger)
    {
        _guildRepository = guildRepository;
        _openAIService = openAIService;
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<ExplainMessagesResult> Handle(ExplainMessagesQuery request, CancellationToken cancellationToken)
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

            string? aiSummary = null;
            if (guild.HasValidApiKey())
            {
                aiSummary = await _openAIService.ExplainQueryAsync(guild.ApiKey!, request.Query, cancellationToken);
            }
            else
            {
                return new ExplainMessagesResult(false, null,
                    ErrorMessage: "AI enhancement requires an OpenAI API key. Use the `!setapikey` command to configure it.");
            }

            return new ExplainMessagesResult(true, aiSummary, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when trying to explain query: {query}", request.Query);
            return new ExplainMessagesResult(false, null,
                ErrorMessage: "An error occurred while searching. Please try again later.");
        }
    }
}