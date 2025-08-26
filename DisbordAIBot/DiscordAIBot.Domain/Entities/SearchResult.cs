using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Domain.Entities;

public class SearchResult
{
    public MessageId MessageId { get; private set; }
    public ChannelId ChannelId { get; private set; }
    public UserId AuthorId { get; private set; }
    public string AuthorName { get; private set; }
    public string Content { get; private set; }
    public string ChannelName { get; private set; }
    public string MessageUrl { get; private set; }
    public DateTime Timestamp { get; private set; }
    public double RelevanceScore { get; private set; }
    public string? AIEnhancedSummary { get; private set; }

    private SearchResult() { } // EF Constructor

    public SearchResult(
        MessageId messageId,
        ChannelId channelId,
        UserId authorId,
        string authorName,
        string content,
        string channelName,
        string messageUrl,
        DateTime timestamp,
        double relevanceScore)
    {
        MessageId = messageId;
        ChannelId = channelId;
        AuthorId = authorId;
        AuthorName = authorName;
        Content = content;
        ChannelName = channelName;
        MessageUrl = messageUrl;
        Timestamp = timestamp;
        RelevanceScore = relevanceScore;
    }

    public void SetAIEnhancedSummary(string summary)
    {
        AIEnhancedSummary = summary;
    }
}
