using Discord;
using Discord.Commands;
using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Application.Queries.ExplainMessages;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Presentation.Commands;

public class ExplainCommands : ModuleBase<SocketCommandContext>
{
    private readonly IMediator _mediator;

    public ExplainCommands(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Command("explain")]
    [Summary("Explain a sentence, concept, or answer any provided query using AI.")]
    public async Task ExplainQueryAsync([Remainder] string query)
    {
        await ExecuteExplanationAsync(query);
    }

    private async Task ExecuteExplanationAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await ReplyAsync("❌ Please provide a query to explain.");
            return;
        }

        await Context.Message.AddReactionAsync(new Emoji("🧠"));

        try
        {
            var explainQuery = new ExplainMessagesQuery(new GuildId(Context.Guild.Id), query);

            var result = await _mediator.Send(explainQuery);
            
            if (!result.Success)
            {
                await ReplyAsync($"❌ {result.ErrorMessage}");
                return;
            }

            if (string.IsNullOrWhiteSpace(result.AIExplanation))
            {
                await ReplyAsync("❌ AI could not generate an explanation. Try rephrasing your query.");
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithTitle($"🧠 Explanation for: {query}")
                    .WithDescription(result.AIExplanation.Length > 1024 ? result.AIExplanation.Substring(0, 1021) + "..." : result.AIExplanation)
                    .WithColor(Color.Green)
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            await Context.Message.RemoveReactionAsync(new Emoji("🔍"), Context.Client.CurrentUser);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }
        catch (Exception ex)
        {
            await ReplyAsync($"❌ An error occurred while explaining: {ex.Message}");
            await Context.Message.RemoveReactionAsync(new Emoji("🔍"), Context.Client.CurrentUser);
            await Context.Message.AddReactionAsync(new Emoji("❌"));
        }
    }
}