using DiscordAIBot.Application;
using DiscordAIBot.Application.Common.Messaging;
using DiscordAIBot.UnitTests.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DiscordAIBot.UnitTests.Tests;

/// <summary>
/// Tests for application layer dependency injection setup
/// </summary>
public class ApplicationLayerTests : TestBase
{
    [Fact]
    public void AddApplication_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddLogging();
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IMediator>().Should().NotBeNull();
        provider.GetService<IMediator>().Should().BeOfType<Mediator>();
    }

    [Fact]
    public void AddApplication_ShouldRegisterAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Act
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        // Assert - Check that handlers are registered by resolving them
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
        
        // The mediator should be able to handle requests when handlers are available
        var handlerServices = services.Where(s => 
                s.ServiceType.IsGenericType && 
                (s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                 s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
            .ToList();
        
        handlerServices.Should().NotBeEmpty();
    }
}