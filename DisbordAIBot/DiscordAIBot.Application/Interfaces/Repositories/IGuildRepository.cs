using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;

namespace DiscordAIBot.Application.Interfaces.Repositories;

public interface IGuildRepository
{
    Task<Guild?> GetByIdAsync(GuildId guildId, CancellationToken cancellationToken = default);
    Task<Guild> AddAsync(Guild guild, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guild guild, CancellationToken cancellationToken = default);
    Task DeleteAsync(GuildId guildId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guild>> GetActiveGuildsAsync(CancellationToken cancellationToken = default);
}