using DiscordAIBot.Application.Common.Messaging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DiscordAIBot.UnitTests.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for custom Mediator implementation
/// </summary>
public class MediatorTests
{
    public record TestRequest(string Value) : IRequest<TestResponse>;
    public record TestResponse(string Result);
    public record TestRequestNoResponse(string Value) : IRequest;

    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public Task<TestResponse> Handle(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TestResponse($"Handled: {request.Value}"));
        }
    }

    public class TestRequestNoResponseHandler : IRequestHandler<TestRequestNoResponse>
    {
        public static bool WasCalled { get; set; }

        public Task Handle(TestRequestNoResponse request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Send_WithValidHandler_ShouldReturnCorrectResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<TestRequest, TestResponse>, TestRequestHandler>();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<Mediator>>()!;
        var mediator = new Mediator(serviceProvider, logger);

        var request = new TestRequest("test");

        // Act
        var response = await mediator.Send(request);

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().Be("Handled: test");
    }

    [Fact]
    public async Task Send_WithNoResponseHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        TestRequestNoResponseHandler.WasCalled = false;
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<TestRequestNoResponse>, TestRequestNoResponseHandler>();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<Mediator>>()!;
        var mediator = new Mediator(serviceProvider, logger);

        var request = new TestRequestNoResponse("test");

        // Act
        await mediator.Send(request);

        // Assert
        TestRequestNoResponseHandler.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Send_WithNoHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<Mediator>>()!;
        var mediator = new Mediator(serviceProvider, logger);

        var request = new TestRequest("test");

        // Act & Assert
        var act = () => mediator.Send(request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No handler found for request type TestRequest");
    }
}