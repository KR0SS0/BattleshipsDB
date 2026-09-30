namespace Battleship.Domain;

public interface IShotStrategy
{
    Coordinate? ChooseTarget(IReadOnlyList<Shot> shotHistory, Random random);
}