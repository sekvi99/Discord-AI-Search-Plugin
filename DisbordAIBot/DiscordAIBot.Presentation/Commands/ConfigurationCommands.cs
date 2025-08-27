using Discord;
using Discord.Commands;
using DiscordAIBot.Application.Commands.SetApiKey;
using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Presentation.Commands;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class ConfigurationCommands : ModuleBase<SocketCommandContext>
{
    private readonly IMediator _mediator;

    public ConfigurationCommands(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Command("setapikey")]
    [Summary("Set OpenAI API key for AI-enhanced features (Admin only)")]
    public async Task SetApiKeyAsync([Remainder] string apiKey)
    {
        // Delete the command message for security
        try
        {
            await Context.Message.DeleteAsync();
        }
        catch
        {
            // Ignore if we can't delete (permissions)
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await ReplyAsync("❌ Please provide an OpenAI API key. Usage: `!setapikey sk-your-api-key-here`");
            return;
        }

        var command = new SetApiKeyCommand(new GuildId(Context.Guild.Id), apiKey.Trim());
        var result = await _mediator.Send(command);

        var embed = new EmbedBuilder()
            .WithTimestamp(DateTimeOffset.Now);

        if (result.Success)
        {
            embed.WithTitle("✅ API Key Set Successfully")
                .WithDescription(result.Message)
                .WithColor(Color.Green);
        }
        else
        {
            embed.WithTitle("❌ Failed to Set API Key")
                .WithDescription(result.Message)
                .WithColor(Color.Red);
        }

        var responseMessage = await ReplyAsync(embed: embed.Build());

        // Delete the response after 30 seconds for security
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            try
            {
                await responseMessage.DeleteAsync();
            }
            catch
            {
                // Ignore if we can't delete
            }
        });
    }

    [Command("removeapikey")]
    [Summary("Remove the stored OpenAI API key (Admin only)")]
    public async Task RemoveApiKeyAsync()
    {
        var command = new SetApiKeyCommand(new GuildId(Context.Guild.Id), "");
        await _mediator.Send(command);

        await ReplyAsync("✅ OpenAI API key has been removed. AI-enhanced features are now disabled.");
    }
}