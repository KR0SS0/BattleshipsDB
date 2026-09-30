namespace Battleship.Domain;

public sealed class Board
{
    private readonly List<Ship> _ships = [];
    public const int Size = 10;
    public static bool IsInBounds(Coordinate coordinate)
    {
        var columnInBounds = coordinate.Column is >= 0 and < Size;
        var rowInBounds = coordinate.Row is >= 0 and < Size;

        return columnInBounds && rowInBounds;
    }

    public IReadOnlyList<Ship> Ships => _ships.AsReadOnly();
    private readonly HashSet<Coordinate> _shotCells = [];
    public IReadOnlySet<Coordinate> ShotCells => _shotCells.AsReadOnly();
    private readonly List<Shot> _shotHistory = [];
    public IReadOnlyList<Shot> ShotHistory => _shotHistory.AsReadOnly();
    public static IReadOnlyList<Coordinate> AllCells { get; } = CreateAllCells();

    private static IReadOnlyList<Coordinate> CreateAllCells()
    {
        List<Coordinate> cells = [];
        foreach (var column in Enumerable.Range(0, Size))
            foreach (var row in Enumerable.Range(0, Size))
                cells.Add(new Coordinate(column, row));

        return cells.AsReadOnly();
    }

    public IReadOnlyList<Coordinate> UnshotCells() => AllCells.Where(cell => !_shotCells.Contains(cell)).ToList();

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

    public bool TryPlaceShipsRandomly(Random random)
    {
        foreach (var type in Enum.GetValues<ShipType>())
        {
            // Every possible placement for this ship type
            var candidates = (
                from cell in AllCells
                from orientation in new[] { Orientation.Horizontal, Orientation.Vertical }
                select new Ship(type, cell, orientation)
            ).ToList();

            Shuffle(candidates, random);

            // Try all candidates
            var placed = false;
            foreach (var ship in candidates)
            {
                if (!PlaceShip(ship).IsSuccess)
                    continue;

                placed = true;
                break;
            }

            if (!placed)
                return false;
        }

        return true;
    }

    private static void Shuffle<T>(List<T> list, Random random)
    {
        // Fisher-Yates
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public Result<Shot> ReceiveShot(Coordinate shotCoordinate)
    {
        // In Bounds?
        if (!IsInBounds(shotCoordinate))
            return Result<Shot>.Failure(ShotErrors.OutOfBounds);

        // Already shot here?
        if (_shotCells.Contains(shotCoordinate))
            return Result<Shot>.Failure(ShotErrors.AlreadyShotSpace);

        _shotCells.Add(shotCoordinate);

        // Hit ship?
        var hitShip = ShipAtCoordinate(shotCoordinate);

        Shot shot;
        if (hitShip is null)
            shot = Shot.Miss(shotCoordinate);
        else      
            if (IsShipSunk(hitShip))
                shot = Shot.Sunk(shotCoordinate, hitShip.Type);
            else
                shot = Shot.Hit(shotCoordinate);      

        _shotHistory.Add(shot);
        return Result<Shot>.Success(shot);
    }

    public Ship? ShipAtCoordinate(Coordinate coordinate)
    {
        var foundShip = _ships.FirstOrDefault(ship => ship.GetCells().Contains(coordinate));
        return foundShip;
    }

    public bool IsShipSunk(Ship ship)
    {
        return ship.GetCells().All(_shotCells.Contains);
    }

    public bool AreAllShipsPlaced => Enum.GetValues<ShipType>().All(type => _ships.Any(s => s.Type == type));

    public bool AreAllShipsSunk => _ships.Count > 0 && _ships.All(IsShipSunk);
}