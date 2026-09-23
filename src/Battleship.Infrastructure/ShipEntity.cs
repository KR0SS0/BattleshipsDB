using Battleship.Domain;

namespace Battleship.Infrastructure;

public sealed class ShipEntity
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Orientation Orientation { get; set; }
    public int StartColumn { get; set; }
    public int StartRow { get; set; }
    public Side OnSide { get; set; }
    public ShipType Type { get; set; }

    public GameEntity Game { get; set; } = null!;
    public Guid GameId { get; set; }
}