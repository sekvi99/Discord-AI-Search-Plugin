namespace DiscordAIBot.Domain.ValueObjects;

public record ChannelId(ulong Value)
{
    public static implicit operator ulong(ChannelId channelId) => channelId.Value;
    public static implicit operator ChannelId(ulong value) => new(value);
    public override string ToString() => Value.ToString();
}