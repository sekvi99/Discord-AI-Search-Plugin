namespace DiscordAIBot.Domain.ValueObjects;

public record GuildId(ulong Value)
{
    public static implicit operator ulong(GuildId guildId) => guildId.Value;
    public static implicit operator GuildId(ulong value) => new(value);
    public override string ToString() => Value.ToString();
}