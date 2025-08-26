using DiscordAIBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DiscordAIBot.UnitTests.Tests.Common;

/// <summary>
/// Base class for integration tests providing common setup
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        var services = new ServiceCollection();
        
        // Add in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        
        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
        Context = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created
        Context.Database.EnsureCreated();
    }

    protected T GetService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();
    
    protected T GetSubstitute<T>() where T : class => Substitute.For<T>();

    public virtual void Dispose()
    {
        Context.Dispose();
        ServiceProvider.GetService<IServiceScope>()?.Dispose();
    }
}