using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.UnitTests.Tests.Common.Builders;

/// <summary>
/// Test builder for Guild entities
/// </summary>
public class GuildBuilder
{
    private GuildId _id = new(123456789);
    private string _name = "Test Guild";
    private OpenAIApiKey? _apiKey;
    private bool _isActive = true;

    public GuildBuilder WithId(GuildId id)
    {
        _id = id;
        return this;
    }

    public GuildBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public GuildBuilder WithApiKey(string apiKey)
    {
        _apiKey = new OpenAIApiKey(apiKey);
        return this;
    }

    public GuildBuilder WithApiKey(OpenAIApiKey apiKey)
    {
        _apiKey = apiKey;
        return this;
    }

    public GuildBuilder Inactive()
    {
        _isActive = false;
        return this;
    }

    public Guild Build()
    {
        var guild = new Guild(_id, _name);
        
        if (_apiKey != null)
        {
            guild.SetApiKey(_apiKey);
        }

        if (!_isActive)
        {
            guild.Deactivate();
        }

        return guild;
    }

    public static implicit operator Guild(GuildBuilder builder) => builder.Build();
}