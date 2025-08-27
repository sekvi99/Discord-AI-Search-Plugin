using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordAIBot.Presentation.Services;

public class CommandHandlingService
{
    private readonly DiscordSocketClient _discord;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandHandlingService> _logger;

    public CommandHandlingService(
        DiscordSocketClient discord,
        CommandService commands,
        IServiceProvider services,
        ILogger<CommandHandlingService> logger)
    {
        _discord = discord;
        _commands = commands;
        _services = services;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        _discord.MessageReceived += HandleCommandAsync;
        _commands.CommandExecuted += OnCommandExecutedAsync;
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;

        int argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || 
              message.HasMentionPrefix(_discord.CurrentUser, ref argPos))) return;

        var context = new SocketCommandContext(_discord, message);
        await _commands.ExecuteAsync(context, argPos, _services);
    }

    private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
            _logger.LogError("Command {CommandName} failed: {Error}", command.Value?.Name ?? "Unknown", result.ErrorReason);
            
            var embed = new EmbedBuilder()
                .WithTitle("❌ Command Error")
                .WithDescription(result.ErrorReason)
                .WithColor(Color.Red)
                .WithTimestamp(DateTimeOffset.Now);

            await context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
