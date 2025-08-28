using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Application.Interfaces.Services;

public interface IOpenAIService
{
    Task<string> EnhanceSearchResultsAsync(OpenAIApiKey apiKey, IEnumerable<string> searchResults, string query, CancellationToken cancellationToken = default);
    Task<string> SummarizeContentAsync(OpenAIApiKey apiKey, string content, CancellationToken cancellationToken = default);
    Task<bool> ValidateApiKeyAsync(OpenAIApiKey apiKey, CancellationToken cancellationToken = default);
    Task<string> ExplainQueryAsync(OpenAIApiKey apiKey, string query, CancellationToken cancellationToken = default);
}