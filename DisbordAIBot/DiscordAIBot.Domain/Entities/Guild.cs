using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Domain.Entities;

public class Guild
{
    public GuildId Id { get; private set; }
    public string Name { get; private set; }
    public OpenAIApiKey? ApiKey { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public List<Channel> Channels { get; private set; } = new();

    private Guild() { } // EF Constructor

    public Guild(GuildId id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetApiKey(OpenAIApiKey apiKey)
    {
        ApiKey = apiKey;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasValidApiKey() => ApiKey != null && !string.IsNullOrEmpty(ApiKey.Value);
}
