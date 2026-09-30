namespace Battleship.Domain;

public sealed class HuntTargetShotStrategy : IShotStrategy
{
    private static readonly IReadOnlyList<(int Column, int Row)> Offsets = [(-1, 0), (0, -1), (1, 0), (0, 1)];

    public Coordinate? ChooseTarget(IReadOnlyList<Shot> shotHistory, Random random)
    {
        var candidates = CellCandidates(shotHistory);

        if (candidates.Count == 0)
        {
            var randomShooter = new RandomShotStrategy();
            return randomShooter.ChooseTarget(shotHistory, random);
        }

        return candidates[random.Next(candidates.Count)];
    }

    internal static List<Coordinate> CellCandidates(IReadOnlyList<Shot> shotHistory)
    {
        var shotCells = shotHistory.Select(shot => shot.Coordinate).ToHashSet();

        // Cells that are sunken ships
        var sunkCells = new HashSet<Coordinate>();
        foreach (var shot in shotHistory)
        {
            if (shot.SunkShip is not null)
                sunkCells.UnionWith(shot.SunkShip.GetCells());
        }

        // Hits on ships that are still afloat
        var openHits = shotHistory
            .Where(shot => shot.Outcome != ShotOutcome.Miss && !sunkCells.Contains(shot.Coordinate))
            .Select(shot => shot.Coordinate);

        var candidates = new List<Coordinate>();
        foreach (var hit in openHits)
        {
            foreach (var offset in Offsets)
            {
                var neighbour = hit + offset;
                if (Board.IsInBounds(neighbour) && !shotCells.Contains(neighbour))
                    candidates.Add(neighbour);
            }
        }

        return candidates.Distinct().ToList();
    }
}
