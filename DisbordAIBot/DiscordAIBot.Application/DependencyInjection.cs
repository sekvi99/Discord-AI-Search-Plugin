
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DiscordAIBot.Application.Common.Messaging;

namespace DiscordAIBot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomMediator(Assembly.GetExecutingAssembly());
        return services;
    }
}