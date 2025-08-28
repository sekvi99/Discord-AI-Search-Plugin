using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Application.Queries.ExplainMessages;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace DiscordAIBot.UnitTests.UnitTests.Application.Queries;

/// <summary>
/// Unit tests for ExplainMessageQueryHandler
/// </summary>
public class ExplainMessageQueryHandlerTests
{
    private readonly IGuildRepository _guildRepository;
    private readonly IDiscordSearchService _searchService;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ExplainMessageQueryHandler> _logger;
    private readonly ExplainMessageQueryHandler _handler;

    public ExplainMessageQueryHandlerTests()
    {
        _guildRepository = Substitute.For<IGuildRepository>();
        _searchService = Substitute.For<IDiscordSearchService>();
        _openAIService = Substitute.For<IOpenAIService>();
        _logger = Substitute.For<ILogger<ExplainMessageQueryHandler>>();
        _handler = new ExplainMessageQueryHandler(_guildRepository, _searchService, _openAIService, _logger);
    }

    [Fact]
    public async Task Handle_WhenGuildExistsAndApiKeyValid_ShouldReturnExplanation()
    {
        // Arrange
        var guildId = new GuildId(123);
        var query = "What is AI?";
        var explainQuery = new ExplainMessagesQuery(guildId, query);
        var guild = new Guild(guildId, "Test Guild");
        var apiKey = new OpenAIApiKey("sk-test123456789012345678901234567890");
        guild.SetApiKey(apiKey);
        var explanation = "AI stands for Artificial Intelligence...";

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);
        _openAIService.ExplainQueryAsync(apiKey, query, Arg.Any<CancellationToken>())
            .Returns(explanation);

        // Act
        var result = await _handler.Handle(explainQuery, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AIExplanation.Should().Be(explanation);
        result.ErrorMessage.Should().BeNull();
        await _openAIService.Received(1).ExplainQueryAsync(apiKey, query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoApiKey_ShouldReturnError()
    {
        // Arrange
        var guildId = new GuildId(789);
        var query = "Explain blockchain.";
        var explainQuery = new ExplainMessagesQuery(guildId, query);
        var guild = new Guild(guildId, "Test Guild"); // No API key set

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);

        // Act
        var result = await _handler.Handle(explainQuery, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.AIExplanation.Should().BeNull();
        result.ErrorMessage.Should().Contain("AI enhancement requires an OpenAI API key");
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldReturnError()
    {
        // Arrange
        var guildId = new GuildId(999);
        var query = "Explain error handling.";
        var explainQuery = new ExplainMessagesQuery(guildId, query);

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(explainQuery, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.AIExplanation.Should().BeNull();
        result.ErrorMessage.Should().Contain("An error occurred");
    }
}
