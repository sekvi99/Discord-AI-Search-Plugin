using Discord;
using Discord.Commands;

namespace DiscordAIBot.Presentation.Commands;

public class HelpCommands : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("Show available commands")]
    public async Task ShowHelpAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("🤖 Discord AI Assistant Bot")
            .WithDescription("Search and analyze your Discord server messages with optional AI enhancement.")
            .WithColor(Color.Gold)
            .WithTimestamp(DateTimeOffset.Now);

        embed.AddField("📊 **Search Commands**",
            "`!search <query>` - Search across all channels\n" +
            "`!search ai <query>` - Search with AI summary (requires API key)\n" +
            "`!search user @user [query]` - Search messages from specific user\n" +
            "`!search channel #channel <query>` - Search within specific channel",
            false);

        embed.AddField("⚙️ **Configuration Commands** (Admin Only)",
            "`!setapikey <api-key>` - Set OpenAI API key for AI features\n" +
            "`!removeapikey` - Remove stored API key",
            false);

        embed.AddField("ℹ️ **Other Commands**",
            "`!help` - Show this help message\n" +
            "`!about` - Bot information",
            false);

        embed.AddField("🔑 **Getting an OpenAI API Key**",
            "1. Visit [OpenAI Platform](https://platform.openai.com/api-keys)\n" +
            "2. Create an account and generate an API key\n" +
            "3. Use `!setapikey sk-your-key-here` to enable AI features",
            false);

        embed.WithFooter("💡 Tip: AI features provide enhanced summaries and insights from your search results!");

        await ReplyAsync(embed: embed.Build());
    }

    [Command("about")]
    [Summary("Show bot information")]
    public async Task ShowAboutAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("🤖 Discord AI Assistant Bot")
            .WithDescription("An intelligent Discord bot that helps you search and analyze server messages.")
            .WithColor(Color.Purple)
            .WithTimestamp(DateTimeOffset.Now);

        embed.AddField("✨ **Features**",
            "• Smart message search across all channels\n" +
            "• User and channel-specific searches\n" +
            "• AI-powered result summaries\n" +
            "• Relevance scoring and ranking\n" +
            "• Privacy-focused (your API keys stay on your server)",
            false);

        embed.AddField("🛠️ **Built With**",
            "• C# .NET 8.0\n" +
            "• Discord.NET\n" +
            "• OpenAI API\n" +
            "• Entity Framework Core\n" +
            "• Clean Architecture",
            false);

        embed.AddField("🔒 **Privacy & Security**",
            "• API keys are stored securely in your server's database\n" +
            "• Bot only accesses channels it has permission to read\n" +
            "• No message content is stored permanently\n" +
            "• API key commands are automatically deleted",
            false);

        await ReplyAsync(embed: embed.Build());
    }
}
