namespace Battleship.Domain;

// Hunt/target, but once two hits line up it follows that line instead of trying every neighbour
public sealed class SmartShotStrategy : IShotStrategy
{
    private static readonly IReadOnlyList<(int Column, int Row)> Directions = [(1, 0), (0, 1)];

    // Use the hunt/target strategy, unless there are two shots in a line, in which case shoot past the ends of that line
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

    // The ends of any line of hits, or the hunt/target neighbours when there is no open line end
    internal static List<Coordinate> CellCandidates(IReadOnlyList<Shot> shotHistory)
    {
        var lineEnds = LineEndCandidates(shotHistory);
        return lineEnds.Count > 0 ? lineEnds : HuntTargetShotStrategy.CellCandidates(shotHistory);
    }

    // The first unshot cell past each end of every line of two or more open hits
    private static List<Coordinate> LineEndCandidates(IReadOnlyList<Shot> shotHistory)
    {
        var shotCells = shotHistory.ShotCells();
        var openHits = shotHistory.OpenHits().ToHashSet();

        var candidates = new List<Coordinate>();
        foreach (var hit in openHits)
        {
            foreach (var step in Directions)
            {
                // Only a line if the next cell in this direction is an open hit too
                if (!openHits.Contains(hit + step))
                    continue;

                candidates.Add(PastEndOfLine(hit, step, openHits));
                candidates.Add(PastEndOfLine(hit, (-step.Column, -step.Row), openHits));
            }
        }

        return candidates
            .Where(cell => Board.IsInBounds(cell) && !shotCells.Contains(cell))
            .Distinct()
            .ToList();
    }

    // Walks from a hit while the cells are open hits, and returns the first cell after them
    private static Coordinate PastEndOfLine(Coordinate start, (int Column, int Row) step, HashSet<Coordinate> openHits)
    {
        var cell = start;
        while (openHits.Contains(cell))
            cell += step;

        return cell;
    }
}
