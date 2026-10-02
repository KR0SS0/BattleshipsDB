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
        var shotCells = shotHistory.ShotCells();
        var openHits = shotHistory.OpenHits();

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
