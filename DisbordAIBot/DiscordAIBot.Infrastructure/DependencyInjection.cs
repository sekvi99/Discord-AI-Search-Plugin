using Discord;
using Discord.WebSocket;
using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Infrastructure.Data;
using DiscordAIBot.Infrastructure.Repositories;
using DiscordAIBot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordAIBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=discordbot.db"));

        // Discord
        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All
        }));

        // Repositories
        services.AddScoped<IGuildRepository, GuildRepository>();

        // Services
        services.AddScoped<IDiscordSearchService, DiscordSearchService>();
        services.AddScoped<IOpenAIService, OpenAIService>();

        return services;
    }
}
