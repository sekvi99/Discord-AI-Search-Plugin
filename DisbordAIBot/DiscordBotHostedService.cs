using Discord;
using Discord.WebSocket;
using DiscordAIBot.Presentation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisbordAIBot;

public class DiscordBotHostedService : BackgroundService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly CommandHandlingService _commandHandler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordBotHostedService> _logger;

    public DiscordBotHostedService(
        DiscordSocketClient discordClient,
        CommandHandlingService commandHandler,
        IConfiguration configuration,
        ILogger<DiscordBotHostedService> logger)
    {
        _discordClient = discordClient;
        _commandHandler = commandHandler;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.Log += LogAsync;
        _discordClient.Ready += ReadyAsync;

        var token = _configuration["Discord:BotToken"] ?? 
                   Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogCritical("Discord bot token not found. Set DISCORD_BOT_TOKEN environment variable or configure Discord:BotToken in appsettings.json");
            return;
        }

        await _commandHandler.InitializeAsync();
        await _discordClient.LoginAsync(TokenType.Bot, token);
        await _discordClient.StartAsync();

        // Keep the bot running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        await _discordClient.LogoutAsync();
        await _discordClient.StopAsync();
    }

    private Task LogAsync(LogMessage log)
    {
        var logLevel = log.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, log.Exception, "[{Source}] {Message}", log.Source, log.Message);
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        _logger.LogInformation("Bot {BotName} is connected and ready!", _discordClient.CurrentUser.Username);
        return Task.CompletedTask;
    }
}
