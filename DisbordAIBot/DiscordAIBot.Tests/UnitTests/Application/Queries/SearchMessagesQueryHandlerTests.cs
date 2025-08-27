using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Application.Queries.SearchMessages;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DiscordAIBot.UnitTests.UnitTests.Application.Queries;

/// <summary>
/// Unit tests for SearchMessagesQueryHandler
/// </summary>
public class SearchMessagesQueryHandlerTests
{
    private readonly IGuildRepository _guildRepository;
    private readonly IDiscordSearchService _searchService;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<SearchMessagesQueryHandler> _logger;
    private readonly SearchMessagesQueryHandler _handler;

    public SearchMessagesQueryHandlerTests()
    {
        _guildRepository = Substitute.For<IGuildRepository>();
        _searchService = Substitute.For<IDiscordSearchService>();
        _openAIService = Substitute.For<IOpenAIService>();
        _logger = Substitute.For<ILogger<SearchMessagesQueryHandler>>();
        _handler = new SearchMessagesQueryHandler(_guildRepository, _searchService, _openAIService, _logger);
    }

    [Fact]
    public async Task Handle_WhenSearchingAllChannels_ShouldCallCorrectSearchMethod()
    {
        // Arrange
        var guildId = new GuildId(123);
        var searchQuery = "test query";
        var query = new SearchMessagesQuery(guildId, searchQuery);
        var guild = new Guild(guildId, "Test Guild");
        var searchResults = new List<SearchResult>
        {
            CreateTestSearchResult()
        };

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);
        _searchService.SearchAcrossGuildAsync(guildId, searchQuery, Arg.Any<CancellationToken>())
            .Returns(searchResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(1);
        await _searchService.Received(1).SearchAcrossGuildAsync(guildId, searchQuery, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSearchingSpecificChannel_ShouldCallCorrectSearchMethod()
    {
        // Arrange
        var guildId = new GuildId(123);
        var channelId = new ChannelId(456);
        var searchQuery = "test query";
        var query = new SearchMessagesQuery(guildId, searchQuery, channelId);
        var guild = new Guild(guildId, "Test Guild");
        var searchResults = new List<SearchResult> { CreateTestSearchResult() };

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);
        _searchService.SearchInChannelAsync(channelId, searchQuery, Arg.Any<CancellationToken>())
            .Returns(searchResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _searchService.Received(1).SearchInChannelAsync(channelId, searchQuery, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAIEnhancementRequested_ShouldCallOpenAIService()
    {
        // Arrange
        var guildId = new GuildId(123);
        var searchQuery = "test query";
        var query = new SearchMessagesQuery(guildId, searchQuery, UseAIEnhancement: true);
        var guild = new Guild(guildId, "Test Guild");
        var apiKey = new OpenAIApiKey("sk-test123456789012345678901234567890");
        guild.SetApiKey(apiKey);

        var searchResults = new List<SearchResult> { CreateTestSearchResult() };
        var aiSummary = "AI generated summary";

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);
        _searchService.SearchAcrossGuildAsync(guildId, searchQuery, Arg.Any<CancellationToken>())
            .Returns(searchResults);
        _openAIService.EnhanceSearchResultsAsync(apiKey, Arg.Any<IEnumerable<string>>(), searchQuery, Arg.Any<CancellationToken>())
            .Returns(aiSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AIEnhancedSummary.Should().Be(aiSummary);
        await _openAIService.Received(1).EnhanceSearchResultsAsync(apiKey, Arg.Any<IEnumerable<string>>(), searchQuery, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAIEnhancementRequestedButNoApiKey_ShouldReturnError()
    {
        // Arrange
        var guildId = new GuildId(123);
        var searchQuery = "test query";
        var query = new SearchMessagesQuery(guildId, searchQuery, UseAIEnhancement: true);
        var guild = new Guild(guildId, "Test Guild"); // No API key set

        var searchResults = new List<SearchResult> { CreateTestSearchResult() };

        _guildRepository.GetByIdAsync(guildId, Arg.Any<CancellationToken>())
            .Returns(guild);
        _searchService.SearchAcrossGuildAsync(guildId, searchQuery, Arg.Any<CancellationToken>())
            .Returns(searchResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("AI enhancement requires an OpenAI API key");
    }

    private static SearchResult CreateTestSearchResult()
    {
        return new SearchResult(
            new MessageId(789),
            new ChannelId(456),
            new UserId(321),
            "TestUser",
            "Test message content",
            "general",
            "https://discord.com/channels/123/456/789",
            DateTime.UtcNow,
            1.0);
    }
}