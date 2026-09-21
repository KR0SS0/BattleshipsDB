namespace Battleship.Domain;

public readonly record struct Coordinate(int Column, int Row)
{
    public override string ToString()
    {
        return $"{(char) ('A' + Column)}{Row}";
    }
}

