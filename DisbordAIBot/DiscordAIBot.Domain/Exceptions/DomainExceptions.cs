namespace DiscordAIBot.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidApiKeyException : DomainException
{
    public InvalidApiKeyException(string message) : base(message) { }
}

public class GuildNotFoundException : DomainException
{
    public GuildNotFoundException(ulong guildId) : base($"Guild with ID {guildId} not found") { }
}

public class ApiKeyNotSetException : DomainException
{
    public ApiKeyNotSetException(ulong guildId) : base($"OpenAI API key not set for guild {guildId}") { }
}