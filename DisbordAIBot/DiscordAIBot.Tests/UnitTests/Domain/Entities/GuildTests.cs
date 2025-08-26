using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DiscordAIBot.UnitTests.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for Guild entity
/// </summary>
public class GuildTests
{
    [Fact]
    public void Guild_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var guildId = new GuildId(123456789);
        var guildName = "Test Guild";

        // Act
        var guild = new Guild(guildId, guildName);

        // Assert
        guild.Id.Should().Be(guildId);
        guild.Name.Should().Be(guildName);
        guild.IsActive.Should().BeTrue();
        guild.ApiKey.Should().BeNull();
        guild.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        guild.UpdatedAt.Should().BeNull();
        guild.Channels.Should().BeEmpty();
    }

    [Fact]
    public void SetApiKey_WhenValidKey_ShouldUpdateApiKey()
    {
        // Arrange
        var guild = new Guild(new GuildId(123), "Test");
        var apiKey = new OpenAIApiKey("sk-test123456789012345678901234567890");

        // Act
        guild.SetApiKey(apiKey);

        // Assert
        guild.ApiKey.Should().Be(apiKey);
        guild.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void HasValidApiKey_WhenApiKeyIsSet_ShouldReturnTrue()
    {
        // Arrange
        var guild = new Guild(new GuildId(123), "Test");
        var apiKey = new OpenAIApiKey("sk-test123456789012345678901234567890");
        guild.SetApiKey(apiKey);

        // Act
        var hasValidKey = guild.HasValidApiKey();

        // Assert
        hasValidKey.Should().BeTrue();
    }

    [Fact]
    public void HasValidApiKey_WhenApiKeyIsNull_ShouldReturnFalse()
    {
        // Arrange
        var guild = new Guild(new GuildId(123), "Test");

        // Act
        var hasValidKey = guild.HasValidApiKey();

        // Assert
        hasValidKey.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var guild = new Guild(new GuildId(123), "Test");
        guild.Deactivate();

        // Act
        guild.Activate();

        // Assert
        guild.IsActive.Should().BeTrue();
        guild.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var guild = new Guild(new GuildId(123), "Test");

        // Act
        guild.Deactivate();

        // Assert
        guild.IsActive.Should().BeFalse();
        guild.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}