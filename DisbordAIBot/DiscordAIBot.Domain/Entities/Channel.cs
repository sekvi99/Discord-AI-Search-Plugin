using DiscordAIBot.Domain.Enums;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Domain.Entities;

public class Channel
{
    public ChannelId Id { get; private set; }
    public GuildId GuildId { get; private set; }
    public string Name { get; private set; }
    public ChannelType Type { get; private set; }
    public bool IsSearchable { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Channel() { } // EF Constructor

    public Channel(ChannelId id, GuildId guildId, string name, ChannelType type)
    {
        Id = id;
        GuildId = guildId;
        Name = name;
        Type = type;
        IsSearchable = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void ToggleSearchable()
    {
        IsSearchable = !IsSearchable;
    }
}
