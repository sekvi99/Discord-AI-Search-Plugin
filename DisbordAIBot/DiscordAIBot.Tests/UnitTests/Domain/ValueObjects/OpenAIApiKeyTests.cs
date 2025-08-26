using DiscordAIBot.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DiscordAIBot.UnitTests.UnitTests.Domain.ValueObjects;

/// <summary>
/// Unit tests for OpenAIApiKey value object
/// </summary>
public class OpenAIApiKeyTests
{
    [Fact]
    public void OpenAIApiKey_WhenValidKey_ShouldCreateSuccessfully()
    {
        // Arrange
        var validKey = "sk-test123456789012345678901234567890";

        // Act
        var apiKey = new OpenAIApiKey(validKey);

        // Assert
        apiKey.Value.Should().Be(validKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void OpenAIApiKey_WhenNullOrEmpty_ShouldThrowArgumentException(string? invalidKey)
    {
        // Act & Assert
        var act = () => new OpenAIApiKey(invalidKey!);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value")
            .WithMessage("API key cannot be null or empty*");
    }

    [Fact]
    public void OpenAIApiKey_WhenInvalidFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidKey = "invalid-key-format";

        // Act & Assert
        var act = () => new OpenAIApiKey(invalidKey);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value")
            .WithMessage("Invalid OpenAI API key format*");
    }

    [Fact]
    public void ToString_ShouldReturnMaskedKey()
    {
        // Arrange
        var validKey = "sk-test123456789012345678901234567890";
        var apiKey = new OpenAIApiKey(validKey);

        // Act
        var maskedKey = apiKey.ToString();

        // Assert
        maskedKey.Should().Be("sk-***7890");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var validKey = "sk-test123456789012345678901234567890";
        var apiKey = new OpenAIApiKey(validKey);

        // Act
        string convertedKey = apiKey;

        // Assert
        convertedKey.Should().Be(validKey);
    }
}
