namespace Battleship.Domain;

// Questions every shot strategy asks about the shots fired so far
internal static class ShotHistoryExtensions
{
    public static HashSet<Coordinate> ShotCells(this IReadOnlyList<Shot> shotHistory) =>
        shotHistory.Select(shot => shot.Coordinate).ToHashSet();

    // Hits on ships that are still afloat, in the order they were fired
    public static List<Coordinate> OpenHits(this IReadOnlyList<Shot> shotHistory)
    {
        var sunkCells = shotHistory
            .Where(shot => shot.SunkShip is not null)
            .SelectMany(shot => shot.SunkShip!.GetCells())
            .ToHashSet();

        return shotHistory
            .Where(shot => shot.Outcome != ShotOutcome.Miss && !sunkCells.Contains(shot.Coordinate))
            .Select(shot => shot.Coordinate)
            .ToList();
    }
}
