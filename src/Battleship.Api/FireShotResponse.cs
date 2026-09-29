namespace Battleship.Api;

public sealed record FireShotResponse(
    ShotResponse PlayerShot,
    ShotResponse? OpponentShot,
    GameStateResponse Game);
