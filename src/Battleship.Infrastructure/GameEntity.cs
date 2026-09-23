using Battleship.Domain;

namespace Battleship.Infrastructure; 

public sealed class GameEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Side CurrentTurn { get; set; }
    
    public List<ShipEntity> Ships { get; set; } = [];
    public List<ShotEntity> Shots { get; set; } = [];
}