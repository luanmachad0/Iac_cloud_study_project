using Microsoft.EntityFrameworkCore;
using SportsBetting.Domain.Bets;

namespace SportsBetting.Infrastructure.Persistence;

public sealed class SportsBettingDbContext : DbContext
{
    public SportsBettingDbContext(DbContextOptions<SportsBettingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Bet> Bets => Set<Bet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bet>(builder =>
        {
            builder.ToTable("bets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.MatchId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Prediction).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.SettledAtUtc).IsRequired(false);
        });
    }
}
