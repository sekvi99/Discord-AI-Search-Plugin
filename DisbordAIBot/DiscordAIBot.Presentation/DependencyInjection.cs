using Discord.Commands;
using DiscordAIBot.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordAIBot.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<CommandService>();
        services.AddSingleton<CommandHandlingService>();
        
        return services;
    }
}
