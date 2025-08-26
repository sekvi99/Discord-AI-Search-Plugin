using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using DiscordAIBot.Infrastructure.Data;
using DiscordAIBot.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DiscordAIBot.UnitTests.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for GuildRepository
/// </summary>
public class GuildRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GuildRepository _repository;

    public GuildRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new GuildRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WhenValidGuild_ShouldPersistToDatabase()
    {
        // Arrange
        var guild = new Guild(new GuildId(123), "Test Guild");
        var apiKey = new OpenAIApiKey("sk-test123456789012345678901234567890");
        guild.SetApiKey(apiKey);

        // Act
        var result = await _repository.AddAsync(guild);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(guild.Id);
        
        var savedGuild = await _context.Guilds.FirstOrDefaultAsync(g => g.Id == guild.Id);
        savedGuild.Should().NotBeNull();
        savedGuild!.Name.Should().Be("Test Guild");
        savedGuild.HasValidApiKey().Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenGuildExists_ShouldReturnGuild()
    {
        // Arrange
        var guildId = new GuildId(456);
        var guild = new Guild(guildId, "Another Test Guild");
        await _repository.AddAsync(guild);

        // Act
        var result = await _repository.GetByIdAsync(guildId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(guildId);
        result.Name.Should().Be("Another Test Guild");
    }

    [Fact]
    public async Task GetByIdAsync_WhenGuildDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentGuildId = new GuildId(999);

        // Act
        var result = await _repository.GetByIdAsync(nonExistentGuildId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenGuildExists_ShouldUpdateGuild()
    {
        // Arrange
        var guild = new Guild(new GuildId(789), "Original Name");
        await _repository.AddAsync(guild);
        
        var apiKey = new OpenAIApiKey("sk-test123456789012345678901234567890");
        guild.SetApiKey(apiKey);

        // Act
        await _repository.UpdateAsync(guild);

        // Assert
        var updatedGuild = await _repository.GetByIdAsync(guild.Id);
        updatedGuild.Should().NotBeNull();
        updatedGuild!.HasValidApiKey().Should().BeTrue();
        updatedGuild.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenGuildExists_ShouldRemoveGuild()
    {
        // Arrange
        var guildId = new GuildId(321);
        var guild = new Guild(guildId, "To Be Deleted");
        await _repository.AddAsync(guild);

        // Act
        await _repository.DeleteAsync(guildId);

        // Assert
        var deletedGuild = await _repository.GetByIdAsync(guildId);
        deletedGuild.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveGuildsAsync_ShouldReturnOnlyActiveGuilds()
    {
        // Arrange
        var activeGuild1 = new Guild(new GuildId(111), "Active 1");
        var activeGuild2 = new Guild(new GuildId(222), "Active 2");
        var inactiveGuild = new Guild(new GuildId(333), "Inactive");
        inactiveGuild.Deactivate();

        await _repository.AddAsync(activeGuild1);
        await _repository.AddAsync(activeGuild2);
        await _repository.AddAsync(inactiveGuild);

        // Act
        var activeGuilds = await _repository.GetActiveGuildsAsync();

        // Assert
        activeGuilds.Should().HaveCount(2);
        activeGuilds.Should().Contain(g => g.Id == activeGuild1.Id);
        activeGuilds.Should().Contain(g => g.Id == activeGuild2.Id);
        activeGuilds.Should().NotContain(g => g.Id == inactiveGuild.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}