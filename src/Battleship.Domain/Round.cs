namespace Battleship.Domain;

public sealed record Round(Shot PlayerShot, Shot? OpponentShot);