using Battleship.Domain;

namespace Battleship.Infrastructure;

public sealed class ShotEntity
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Side TargetedSide { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }
    public ShotOutcome Outcome { get; set; }
    public ShipType? SunkShipType { get; set; }

    public GameEntity Game { get; set; } = null!;
    public Guid GameId { get; set; }
}