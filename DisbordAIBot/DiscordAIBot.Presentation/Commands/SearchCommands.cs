using Discord;
using Discord.Commands;
using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.Application.Queries.SearchMessages;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Presentation.Commands;

[Group("search")]
public class SearchCommands : ModuleBase<SocketCommandContext>
{
    private readonly IMediator _mediator;

    public SearchCommands(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Command("")]
    [Summary("Search across all channels for specific information")]
    public async Task SearchAllChannelsAsync([Remainder] string query)
    {
        await ExecuteSearchAsync(query, useAI: false);
    }

    [Command("ai")]
    [Summary("Search across all channels with AI enhancement")]
    public async Task SearchAllChannelsWithAIAsync([Remainder] string query)
    {
        await ExecuteSearchAsync(query, useAI: true);
    }

    [Command("user")]
    [Summary("Search for messages from a specific user")]
    public async Task SearchUserMessagesAsync(IUser user, [Remainder] string? query = null)
    {
        await ExecuteSearchAsync(query ?? "", useAI: false, userId: new UserId(user.Id));
    }

    [Command("channel")]
    [Summary("Search within a specific channel")]
    public async Task SearchChannelAsync(ITextChannel channel, [Remainder] string query)
    {
        await ExecuteSearchAsync(query, useAI: false, channelId: new ChannelId(channel.Id));
    }

    private async Task ExecuteSearchAsync(string query, bool useAI, ChannelId? channelId = null, UserId? userId = null)
    {
        if (string.IsNullOrWhiteSpace(query) && userId == null)
        {
            await ReplyAsync("❌ Please provide a search query.");
            return;
        }

        await Context.Message.AddReactionAsync(new Emoji("🔍"));

        try
        {
            var searchQuery = new SearchMessagesQuery(
                new GuildId(Context.Guild.Id),
                query,
                channelId,
                userId,
                useAI);

            var result = await _mediator.Send(searchQuery);

            if (!result.Success)
            {
                await ReplyAsync($"❌ {result.ErrorMessage}");
                return;
            }

            if (!result.Results.Any())
            {
                var searchType = channelId is not null ? $"in {Context.Guild.GetTextChannel(channelId.Value).Mention}" :
                               userId is not null ? $"from {Context.Guild.GetUser(userId.Value)?.Mention ?? "user"}" :
                               "across all channels";
                
                await ReplyAsync($"❌ No results found {searchType} for: `{query}`");
                return;
            }

            var embed = await CreateSearchResultsEmbedAsync(result, query, channelId, userId);
            await ReplyAsync(embed: embed);

            await Context.Message.RemoveReactionAsync(new Emoji("🔍"), Context.Client.CurrentUser);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }
        catch (Exception ex)
        {
            await ReplyAsync($"❌ An error occurred while searching: {ex.Message}");
            await Context.Message.RemoveReactionAsync(new Emoji("🔍"), Context.Client.CurrentUser);
            await Context.Message.AddReactionAsync(new Emoji("❌"));
        }
    }

    private async Task<Embed> CreateSearchResultsEmbedAsync(
        SearchMessagesResult result, 
        string query, 
        ChannelId? channelId = null, 
        UserId? userId = null)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now);

        // Set title based on search type
        if (channelId is not null)
        {
            var channel = Context.Guild.GetTextChannel(channelId.Value);
            embed.WithTitle($"🔍 Search Results in #{channel?.Name}");
        }
        else if (userId is not null)
        {
            var user = Context.Guild.GetUser(userId.Value);
            embed.WithTitle($"🔍 Messages from {user?.Username ?? "User"}");
        }
        else
        {
            embed.WithTitle($"🔍 Search Results for: {query}");
        }

        // Add AI summary if available
        if (!string.IsNullOrEmpty(result.AIEnhancedSummary))
        {
            embed.AddField("🤖 AI Summary", result.AIEnhancedSummary, false);
        }

        // Add search results
        int resultCount = 0;
        foreach (var searchResult in result.Results.Take(5))
        {
            resultCount++;
            string preview = searchResult.Content.Length > 200
                ? searchResult.Content.Substring(0, 200) + "..."
                : searchResult.Content;

            var fieldName = channelId is not null
                ? $"{searchResult.AuthorName} - {searchResult.Timestamp:MMM dd, yyyy}"
                : $"#{searchResult.ChannelName} - {searchResult.AuthorName}";

            embed.AddField(
                fieldName,
                $"{preview}\n[Jump to message]({searchResult.MessageUrl})",
                false);
        }

        if (result.Results.Count() > 5)
        {
            embed.WithFooter($"Showing 5 of {result.Results.Count()} results");
        }

        return embed.Build();
    }
}