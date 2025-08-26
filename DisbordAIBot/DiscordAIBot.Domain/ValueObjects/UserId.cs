namespace DiscordAIBot.Domain.ValueObjects;

public record UserId(ulong Value)
{
    public static implicit operator ulong(UserId userId) => userId.Value;
    public static implicit operator UserId(ulong value) => new(value);
    public override string ToString() => Value.ToString();
}