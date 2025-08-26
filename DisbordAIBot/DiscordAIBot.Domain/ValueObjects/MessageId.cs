namespace DiscordAIBot.Domain.ValueObjects;

public record MessageId(ulong Value)
{
    public static implicit operator ulong(MessageId messageId) => messageId.Value;
    public static implicit operator MessageId(ulong value) => new(value);
    public override string ToString() => Value.ToString();
}