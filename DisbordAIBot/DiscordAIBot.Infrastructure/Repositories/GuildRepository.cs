using DiscordAIBot.Application.Interfaces.Repositories;
using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using DiscordAIBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscordAIBot.Infrastructure.Repositories;

public class GuildRepository : IGuildRepository
{
    private readonly ApplicationDbContext _context;

    public GuildRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guild?> GetByIdAsync(GuildId guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Guilds
            .Include(g => g.Channels)
            .FirstOrDefaultAsync(g => g.Id == guildId, cancellationToken);
    }

    public async Task<Guild> AddAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        _context.Guilds.Add(guild);
        await _context.SaveChangesAsync(cancellationToken);
        return guild;
    }

    public async Task UpdateAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        _context.Guilds.Update(guild);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(GuildId guildId, CancellationToken cancellationToken = default)
    {
        var guild = await GetByIdAsync(guildId, cancellationToken);
        if (guild != null)
        {
            _context.Guilds.Remove(guild);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Guild>> GetActiveGuildsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Guilds
            .Where(g => g.IsActive)
            .Include(g => g.Channels)
            .ToListAsync(cancellationToken);
    }
}
