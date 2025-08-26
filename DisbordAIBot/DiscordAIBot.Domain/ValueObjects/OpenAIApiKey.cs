namespace DiscordAIBot.Domain.ValueObjects;

public record OpenAIApiKey
{
    public string Value { get; }

    public OpenAIApiKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("API key cannot be null or empty", nameof(value));
        
        if (!value.StartsWith("sk-"))
            throw new ArgumentException("Invalid OpenAI API key format", nameof(value));

        Value = value;
    }

    public static implicit operator string(OpenAIApiKey apiKey) => apiKey.Value;
    public override string ToString() => $"sk-***{Value[^4..]}"; // Masked for logging
}
