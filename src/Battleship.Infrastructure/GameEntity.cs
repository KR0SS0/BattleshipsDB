using Battleship.Domain;

namespace Battleship.Infrastructure;

// Principal (Parent)
public sealed class GameEntity
{
    public Guid Id { get; set; }
    public uint Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Side CurrentTurn { get; set; }

    public List<ShipEntity> Ships { get; set; } = []; // One (game) to many (ships)
    public List<ShotEntity> Shots { get; set; } = []; // One (game) to many (shots)
    public Difficulty Difficulty { get; set; }
}