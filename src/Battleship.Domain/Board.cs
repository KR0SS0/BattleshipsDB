namespace Battleship.Domain;

public sealed class Board
{
    private readonly List<Ship> _ships = [];
    public const int Size = 10;
    public bool IsInBounds(Coordinate coordinate)
    {
        var columnInBounds = coordinate.Column is >= 0 and < Size;
        var rowInBounds = coordinate.Row is >= 0 and < Size;

        return columnInBounds && rowInBounds;
    }

    public IReadOnlyList<Ship> Ships => _ships.AsReadOnly();

    public Result<Ship> PlaceShip(Ship ship)
    {
        var cells = ship.GetCells();
        
        if (!cells.All(IsInBounds))
            return Result<Ship>.Failure(PlacementErrors.OutOfBounds);

        if (_ships.Any(placed => placed.Type == ship.Type))
            return Result<Ship>.Failure(PlacementErrors.DuplicateShipType);

        var occupied = _ships.SelectMany(placed => placed.GetCells()).ToHashSet();
        if (cells.Any(occupied.Contains))
            return Result<Ship>.Failure(PlacementErrors.Overlap);

        _ships.Add(ship);
        return Result<Ship>.Success(ship);
    }
}