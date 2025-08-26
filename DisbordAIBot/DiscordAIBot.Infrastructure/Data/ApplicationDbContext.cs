using DiscordAIBot.Domain.Entities;
using DiscordAIBot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DiscordAIBot.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<Channel> Channels { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Guild Configuration
        modelBuilder.Entity<Guild>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id)
                .HasConversion(id => id.Value, value => new GuildId(value));
            
            entity.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(g => g.ApiKey)
                .HasConversion(
                    apiKey => apiKey != null ? apiKey.Value : null,
                    value => value != null ? new OpenAIApiKey(value) : null)
                .HasMaxLength(200);

            entity.HasMany(g => g.Channels)
                .WithOne()
                .HasForeignKey(c => c.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Channel Configuration
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id)
                .HasConversion(id => id.Value, value => new ChannelId(value));
            
            entity.Property(c => c.GuildId)
                .HasConversion(id => id.Value, value => new GuildId(value));

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Type)
                .HasConversion<string>();
        });

        base.OnModelCreating(modelBuilder);
    }
}
