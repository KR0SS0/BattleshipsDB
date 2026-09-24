using Battleship.Domain;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Infrastructure;

public sealed class BattleshipDbContext(DbContextOptions<BattleshipDbContext> options) : DbContext(options)
{
    public DbSet<ShipEntity> Ships { get; set; }
    public DbSet<GameEntity> Games { get; set; }
    public DbSet<ShotEntity> Shots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameEntity>()
            .Property(e => e.Version)
            .IsRowVersion();

        modelBuilder.Entity<GameEntity>()
            .Property(e => e.CurrentTurn)
            .HasConversion<string>();

        modelBuilder.Entity<GameEntity>()
            .HasMany(e => e.Ships)
            .WithOne(e => e.Game)
            .HasForeignKey(e => e.GameId)
            .IsRequired();


        modelBuilder.Entity<GameEntity>()
            .HasMany(e => e.Shots)
            .WithOne(e => e.Game)
            .HasForeignKey(e => e.GameId)
            .IsRequired();

        modelBuilder.Entity<ShipEntity>()
            .Property(e => e.Orientation)
            .HasConversion<string>();

        modelBuilder.Entity<ShipEntity>()
            .Property(e => e.OnSide)
            .HasConversion<string>();

        modelBuilder.Entity<ShipEntity>()
            .Property(e => e.Type)
            .HasConversion<string>();

        modelBuilder.Entity<ShotEntity>()
            .Property(e => e.TargetedSide)
            .HasConversion<string>();

        modelBuilder.Entity<ShotEntity>()
            .Property(e => e.Outcome)
            .HasConversion<string>();

        modelBuilder.Entity<ShotEntity>()
            .Property(e => e.SunkShipType)
            .HasConversion<string>();

        modelBuilder.Entity<ShotEntity>()
            .HasIndex(e => new { e.GameId, e.Sequence })
            .IsUnique();
    }
}