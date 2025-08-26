using DiscordAIBot.Application.Commands.SetApiKey;
using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.ValueObjects;
using DiscordAIBot.Infrastructure.Data;
using DiscordAIBot.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DiscordAIBot.UnitTests.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for SetApiKey command with real database
/// </summary>
public class SetApiKeyIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SetApiKeyCommandHandler _handler;
    private readonly IOpenAIService _openAIService;

    public SetApiKeyIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        var repository = new GuildRepository(_context);
        _openAIService = Substitute.For<IOpenAIService>();
        var logger = Substitute.For<ILogger<SetApiKeyCommandHandler>>();
        
        _handler = new SetApiKeyCommandHandler(repository, _openAIService, logger);
    }

    [Fact]
    public async Task Handle_EndToEndFlow_ShouldCreateGuildAndSetApiKey()
    {
        // Arrange
        var guildId = new GuildId(12345);
        var apiKeyValue = "sk-test123456789012345678901234567890";
        var command = new SetApiKeyCommand(guildId, apiKeyValue);

        _openAIService.ValidateApiKeyAsync(Arg.Any<OpenAIApiKey>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully");

        // Verify database state
        var savedGuild = await _context.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
        savedGuild.Should().NotBeNull();
        savedGuild!.HasValidApiKey().Should().BeTrue();
        savedGuild.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenApiKeyValidationFails_ShouldNotPersistToDatabase()
    {
        // Arrange
        var guildId = new GuildId(54321);
        var apiKeyValue = "sk-invalid123456789012345678901234567890";
        var command = new SetApiKeyCommand(guildId, apiKeyValue);

        _openAIService.ValidateApiKeyAsync(Arg.Any<OpenAIApiKey>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command,  CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();

        // Verify no guild was created
        var savedGuild = await _context.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
        savedGuild.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}