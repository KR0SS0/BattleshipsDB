namespace Battleship.Domain;

public sealed class RandomShotStrategy : IShotStrategy
{
    public Coordinate? ChooseTarget(IReadOnlyList<Shot> shotHistory, Random random)
    {
        var shotCells = shotHistory.Select(shot => shot.Coordinate).ToHashSet();
        var cellsToTryShooting = Board.AllCells.Where(cell => !shotCells.Contains(cell)).ToList();

        if (cellsToTryShooting.Count == 0)
            return null;

        return cellsToTryShooting[random.Next(cellsToTryShooting.Count)];
    }
}