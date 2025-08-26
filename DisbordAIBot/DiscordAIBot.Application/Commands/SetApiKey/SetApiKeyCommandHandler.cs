using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.Exceptions;
using DiscordAIBot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DiscordAIBot.Application.Commands.SetApiKey;

public class SetApiKeyCommandHandler : IRequestHandler<SetApiKeyCommand, SetApiKeyResult>
{
    private readonly IGuildRepository _guildRepository;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<SetApiKeyCommandHandler> _logger;

    public SetApiKeyCommandHandler(
        IGuildRepository guildRepository,
        IOpenAIService openAIService,
        ILogger<SetApiKeyCommandHandler> logger)
    {
        _guildRepository = guildRepository;
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<SetApiKeyResult> Handle(SetApiKeyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = new OpenAIApiKey(request.ApiKey);
            
            // Validate API key with OpenAI
            var isValid = await _openAIService.ValidateApiKeyAsync(apiKey, cancellationToken);
            if (!isValid)
            {
                return new SetApiKeyResult(false, "Invalid OpenAI API key. Please check your key and try again.");
            }

            var guild = await _guildRepository.GetByIdAsync(request.GuildId, cancellationToken);
            if (guild == null)
            {
                guild = new Guild(request.GuildId, "Unknown"); // Will be updated later
                guild = await _guildRepository.AddAsync(guild, cancellationToken);
            }

            guild.SetApiKey(apiKey);
            await _guildRepository.UpdateAsync(guild, cancellationToken);

            _logger.LogInformation("API key set successfully for guild {GuildId}", request.GuildId);
            return new SetApiKeyResult(true, "OpenAI API key has been set successfully! You can now use AI-enhanced search features.");
        }
        catch (InvalidApiKeyException ex)
        {
            _logger.LogWarning("Invalid API key format for guild {GuildId}: {Message}", request.GuildId, ex.Message);
            return new SetApiKeyResult(false, "Invalid API key format. OpenAI API keys should start with 'sk-'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting API key for guild {GuildId}", request.GuildId);
            return new SetApiKeyResult(false, "An error occurred while setting the API key. Please try again later.");
        }
    }
}
