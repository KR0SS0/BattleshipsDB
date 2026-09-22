using System.ComponentModel.DataAnnotations;

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
    private readonly HashSet<Coordinate> _shots = [];

    public Result<Ship> PlaceShip(Ship ship)
    {
        var cells = ship.GetCells();
        
        // In Bounds?
        if (!cells.All(IsInBounds))
            return Result<Ship>.Failure(PlacementErrors.OutOfBounds);

        // Duplicate shiptype?
        if (_ships.Any(placed => placed.Type == ship.Type))
            return Result<Ship>.Failure(PlacementErrors.DuplicateShipType);

        // Already occupied?
        var occupied = _ships.SelectMany(placed => placed.GetCells()).ToHashSet();
        if (cells.Any(occupied.Contains))
            return Result<Ship>.Failure(PlacementErrors.Overlap);

        _ships.Add(ship);
        return Result<Ship>.Success(ship);
    }

    public Result<Shot> ReceiveShot(Coordinate shotCoordinate)
    {
        // In Bounds?
        if (!IsInBounds(shotCoordinate))
            return Result<Shot>.Failure(ShotErrors.OutOfBounds);

        // Already shot here?
        if (_shots.Contains(shotCoordinate))
            return Result<Shot>.Failure(ShotErrors.AlreadyShotSpace);

        _shots.Add(shotCoordinate);

        // Hit ship?
        var hitShip = _ships.FirstOrDefault(ship => ship.GetCells().Contains(shotCoordinate));
        if (hitShip is null)
            return Result<Shot>.Success(Shot.Miss());

        var isSunk = hitShip.GetCells().All(_shots.Contains);

        return Result<Shot>.Success(isSunk
            ? Shot.Sunk(hitShip.Type)
            : Shot.Hit());
    }

}