# Discord-AI-Search-Plugin

An intelligent Discord bot for searching and analyzing server messages, with optional AI-powered summaries using OpenAI.

---

## 📁 Project Structure

```
DisbordAIBot/
	DisbordAIBot.csproj           # Main bot project
	Program.cs                    # Entry point
	...
	DiscordAIBot.Application/     # Application layer (CQRS, Mediator, Handlers)
		Commands/                   # Command handlers
		Common/Messaging/           # Custom Mediator implementation
		Interfaces/                 # Service and repository interfaces
		Queries/                    # Query handlers
	DiscordAIBot.Domain/          # Domain entities, value objects, enums
	DiscordAIBot.Infrastructure/  # EF Core, repositories, Discord services
		Data/                       # ApplicationDbContext
		Repositories/               # EF Core repositories
		Services/                   # Discord and OpenAI services
	DiscordAIBot.Presentation/    # Discord.NET command modules
	DiscordAIBot.Tests/           # Unit and integration tests
	appsettings.json              # Configuration file
	...
```

---

## 🚀 Getting Started

### 1. Clone the Repository

```
git clone https://github.com/sekvi99/Discord-AI-Search-Plugin.git
cd Discord-AI-Search-Plugin
```

### 2. Configure the Bot

1. **Create a Discord Application & Bot:**
	 - Go to the [Discord Developer Portal](https://discord.com/developers/applications)
	 - Create a new application, add a bot, and copy the **Bot Token**
2. **Set the Bot Token:**
	 - Add your token to `appsettings.json`:
		 ```json
		 {
			 "Discord": {
				 "BotToken": "YOUR_DISCORD_BOT_TOKEN"
			 }
		 }
		 ```
	 - Or set the `DISCORD_BOT_TOKEN` environment variable
3. **Invite the Bot to Your Server:**
	 - In the Developer Portal, go to **OAuth2 > URL Generator**
	 - Select `bot` scope and required permissions (Read, Send, Manage Messages, etc.)
	 - Copy and open the generated URL, select your server, and authorize

### 3. Configure OpenAI (Optional, for AI features)

1. Get an API key from [OpenAI Platform](https://platform.openai.com/api-keys)
2. Use the `!setapikey sk-...` command in your Discord server to store the key securely

### 4. Database Setup

The bot uses SQLite by default. On first run, it will create the database and tables automatically using EF Core migrations.

If you need to manually apply migrations:
```
dotnet ef database update --project DisbordAIBot.Infrastructure/DiscordAIBot.Infrastructure.csproj --startup-project DisbordAIBot/DisbordAIBot.csproj
```

---

## 🛠️ Build & Run

```
dotnet build
dotnet run --project DisbordAIBot/DisbordAIBot.csproj
```

---

## 💬 Usage

### Search Commands

- `!search <query>` — Search across all channels
- `!search ai <query>` — Search with AI summary (requires API key)
- `!search user @user [query]` — Search messages from a specific user
- `!search channel #channel <query>` — Search within a specific channel

### Configuration Commands (Admin Only)

- `!setapikey <api-key>` — Set OpenAI API key for AI features
- `!removeapikey` — Remove stored API key


### Explanation Command

- `!explain <query>` — Get an AI-powered explanation or summary for any question, sentence, or concept

### Other Commands

- `!help` — Show help message
- `!about` — Bot information

---

## 🔑 Privileged Gateway Intents

For full functionality, enable the following intents in the Discord Developer Portal (Bot tab):
- **MESSAGE CONTENT INTENT** (required for message search)
- **SERVER MEMBERS INTENT** (if you want to search by user)

---

## 🧪 Testing

Run all tests:
```
dotnet test
```
Test coverage is enforced in CI (see `.github/workflows/dotnet-test-coverage.yml`).

---

## 🤝 Contributing

Pull requests and issues are welcome! Please open an issue to discuss major changes.

---

## 📄 License

MIT