using System.Globalization;

namespace Battleship.Domain;

public readonly record struct Coordinate(int Column, int Row)
{
    public static bool TryParse(string? text, out Coordinate coordinate)
    {
        coordinate = default;

        if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
            return false;

        var column = char.ToUpperInvariant(text[0]) - 'A';
        if (column < 0 || column > 25)
            return false;

        if (!int.TryParse(text.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var row) || row < 1)
            return false;

        coordinate = new Coordinate(column, row - 1);
        return true;
    }

    public override string ToString()
    {
        return $"{(char)('A' + Column)}{Row + 1}";
    }

    public static Coordinate operator +(Coordinate coordinate, (int Column, int Row) offset) =>
        new(coordinate.Column + offset.Column, coordinate.Row + offset.Row);

    public static Coordinate operator -(Coordinate coordinate, (int Column, int Row) offset) =>
        new(coordinate.Column - offset.Column, coordinate.Row - offset.Row);
}

