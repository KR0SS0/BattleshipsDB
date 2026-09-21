namespace Battleship.Domain;

public sealed record Ship(ShipType Type, Coordinate Start, Orientation Orientation)
{
    public IReadOnlyList<Coordinate> GetCells()
    {
        var length = Type.Length();

        return Orientation switch 
        {
            Orientation.Horizontal => Enumerable.Range(0, length)
                .Select(i => new Coordinate(Start.Column + i, Start.Row))
                .ToList(),
            Orientation.Vertical => Enumerable.Range(0, length)
                .Select(i => new Coordinate(Start.Column, Start.Row + i))
                .ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(Orientation), Orientation, null)
        };
    }
}