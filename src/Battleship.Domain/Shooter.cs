namespace Battleship.Domain;

public sealed class Shooter
{
    public Coordinate? RandomShotAtTargetSide(Board targetBoard, Random random)
    {
        var cellsToTryShooting = targetBoard.UnshotCells();

        if (cellsToTryShooting.Count == 0)
            return null;

        return cellsToTryShooting[random.Next(cellsToTryShooting.Count)];
    }
}