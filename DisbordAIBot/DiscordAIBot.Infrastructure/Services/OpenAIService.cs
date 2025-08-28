using DiscordAIBot.Application.Interfaces.Services;
using DiscordAIBot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace DiscordAIBot.Infrastructure.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(ILogger<OpenAIService> logger)
    {
        _logger = logger;
    }

    public async Task<string> EnhanceSearchResultsAsync(
        OpenAIApiKey apiKey, 
        IEnumerable<string> searchResults, 
        string query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new OpenAIClient(apiKey.Value);
            
            var prompt = $"""
                Based on the following Discord search results for the query "{query}", provide a concise summary of the key information found:

                Search Results:
                {string.Join("\n\n", searchResults.Select((r, i) => $"{i + 1}. {r}"))}

                Please provide:
                1. A brief summary of the main topics discussed
                2. Key points or answers related to the query
                3. Any notable patterns or themes

                Keep the response under 500 words and focus on the most relevant information.
                """;

            var response = await client.GetChatClient("gpt-3.5-turbo").CompleteChatAsync(
                [new UserChatMessage(prompt)],
                cancellationToken: cancellationToken);

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing search results with OpenAI");
            return "Unable to enhance search results at this time.";
        }
    }

    public async Task<string> ExplainQueryAsync(OpenAIApiKey apiKey,
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new OpenAIClient(apiKey.Value);
            
            var prompt = $"""
                          You are a helpful assistant that explains search queries in a clear and concise manner.

                          Please analyze and explain the following search query:
                          "{query}"

                          Provide an explanation that includes:
                          1. Answer to the provided users query
                          2. Key terms and their significance
                          3. Potential search intent or purpose
                          4. Any suggestions for improving the query if applicable

                          Keep your response informative but concise.
                          """;

            var response = await client.GetChatClient("gpt-3.5-turbo").CompleteChatAsync(
                [new UserChatMessage(prompt)],
                cancellationToken: cancellationToken);

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing search results with OpenAI");
            return "Unable to enhance search results at this time.";
        }
    }

    public async Task<string> SummarizeContentAsync(
        OpenAIApiKey apiKey, 
        string content, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new OpenAIClient(apiKey.Value);
            
            var prompt = $"Please provide a concise summary of the following content:\n\n{content}";

            var response = await client.GetChatClient("gpt-3.5-turbo").CompleteChatAsync(
                [new UserChatMessage(prompt)],
                cancellationToken: cancellationToken);

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing content with OpenAI");
            return "Unable to summarize content at this time.";
        }
    }

    public async Task<bool> ValidateApiKeyAsync(
        OpenAIApiKey apiKey, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new OpenAIClient(apiKey.Value);
            
            // Simple validation request
            var response = await client.GetChatClient("gpt-3.5-turbo").CompleteChatAsync(
                [new UserChatMessage("Hello")],
                cancellationToken: cancellationToken);

            return response.Value != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API key validation failed");
            return false;
        }
    }
}
