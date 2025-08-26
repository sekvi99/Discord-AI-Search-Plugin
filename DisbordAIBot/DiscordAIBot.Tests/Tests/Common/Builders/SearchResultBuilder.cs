using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.UnitTests.Tests.Common.Builders;

/// <summary>
/// Test builder for SearchResult entities
/// </summary>
public class SearchResultBuilder
{
    private MessageId _messageId = new(789);
    private ChannelId _channelId = new(456);
    private UserId _authorId = new(321);
    private string _authorName = "TestUser";
    private string _content = "Test message content";
    private string _channelName = "general";
    private string _messageUrl = "https://discord.com/channels/123/456/789";
    private DateTime _timestamp = DateTime.UtcNow;
    private double _relevanceScore = 1.0;

    public SearchResultBuilder WithMessageId(MessageId messageId)
    {
        _messageId = messageId;
        return this;
    }

    public SearchResultBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public SearchResultBuilder WithAuthor(UserId authorId, string authorName)
    {
        _authorId = authorId;
        _authorName = authorName;
        return this;
    }

    public SearchResultBuilder WithChannel(ChannelId channelId, string channelName)
    {
        _channelId = channelId;
        _channelName = channelName;
        return this;
    }

    public SearchResultBuilder WithRelevanceScore(double score)
    {
        _relevanceScore = score;
        return this;
    }

    public SearchResultBuilder WithTimestamp(DateTime timestamp)
    {
        _timestamp = timestamp;
        return this;
    }

    public SearchResult Build()
    {
        return new SearchResult(
            _messageId,
            _channelId,
            _authorId,
            _authorName,
            _content,
            _channelName,
            _messageUrl,
            _timestamp,
            _relevanceScore);
    }

    public static implicit operator SearchResult(SearchResultBuilder builder) => builder.Build();
}