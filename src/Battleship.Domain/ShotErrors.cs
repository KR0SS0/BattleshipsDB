namespace Battleship.Domain;

public static class ShotErrors
{
    public static readonly Error OutOfBounds = new(ErrorType.Invalid, "shot.out_of_bounds", "Shot was not in board.");
    public static readonly Error AlreadyShotSpace = new(ErrorType.Conflict, "shot.already_shot_space", "This space has already been shot.");
}
