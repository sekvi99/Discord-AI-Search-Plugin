using DiscordAIBot.Application.Commands.SetApiKey;
using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace DiscordAIBot.UnitTests.UnitTests.Application.Commands;

/// <summary>
/// Unit tests for SetApiKeyCommandHandler
/// </summary>
public class SetApiKeyCommandHandlerTests
{
    private readonly IGuildRepository _guildRepository;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<SetApiKeyCommandHandler> _logger;
    private readonly SetApiKeyCommandHandler _handler;

    public SetApiKeyCommandHandlerTests()
    {
        _guildRepository = Substitute.For<IGuildRepository>();
        _openAIService = Substitute.For<IOpenAIService>();
        _logger = Substitute.For<ILogger<SetApiKeyCommandHandler>>();
        _handler = new SetApiKeyCommandHandler(_guildRepository, _openAIService, _logger);
    }

    [Fact]
    public async Task Handle_WhenValidApiKeyAndExistingGuild_ShouldSetApiKeySuccessfully()
    {
        // Arrange
        var guildId = new GuildId(123);
        var apiKeyValue = "sk-test123456789012345678901234567890";
        var command = new SetApiKeyCommand(guildId, apiKeyValue);
        var guild = new Guild(guildId, "Test Guild");

        _openAIService.ValidateApiKeyAsync(Arg.Any<OpenAIApiKey>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully");
        await _guildRepository.Received(1).UpdateAsync(guild, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidApiKeyAndNewGuild_ShouldCreateGuildAndSetApiKey()
    {
        // Arrange
        var guildId = new GuildId(123);
        var apiKeyValue = "sk-test123456789012345678901234567890";
        var command = new SetApiKeyCommand(guildId, apiKeyValue);
        var newGuild = new Guild(guildId, "Unknown");

        _openAIService.ValidateApiKeyAsync(Arg.Any<OpenAIApiKey>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns((Guild?)null);
        _guildRepository.AddAsync(Arg.Any<Guild>(), Arg.Any<CancellationToken>())
            .Returns(newGuild);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _guildRepository.Received(1).AddAsync(Arg.Any<Guild>(), Arg.Any<CancellationToken>());
        await _guildRepository.Received(1).UpdateAsync(Arg.Any<Guild>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInvalidApiKey_ShouldReturnFailure()
    {
        // Arrange
        var guildId = new GuildId(123);
        var apiKeyValue = "sk-invalid";
        var command = new SetApiKeyCommand(guildId, apiKeyValue);

        _openAIService.ValidateApiKeyAsync(Arg.Any<OpenAIApiKey>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid OpenAI API key");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var guildId = new GuildId(123);
        var apiKeyValue = "sk-test123456789012345678901234567890";
        var command = new SetApiKeyCommand(guildId, apiKeyValue);

        _openAIService.ValidateApiKeyAsync(Arg.Any<OpenAIApiKey>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("An error occurred");
    }
}