using Battleship.Domain;
using Battleship.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Battleship.Api;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var games = app.MapGroup("/games").WithTags("Games");

        games.MapPost("/", CreateGame);
        games.MapGet("/{id:guid}", GetGame);
        games.MapPost("/{id:guid}/shots", FireShot);
    }

    private static async Task<Created<GameResponse>> CreateGame(
        GameRepository repository, CancellationToken cancellationToken)
    {
        Game game = new();
        game.PlayerBoard.TryPlaceShipsRandomly(Random.Shared);
        game.OpponentBoard.TryPlaceShipsRandomly(Random.Shared);

        await repository.SaveGameAsync(game, cancellationToken);

        GameResponse body = new(game.Id);
        var address = $"/games/{game.Id}";

        return TypedResults.Created(address, body);
    }

    private static async Task<Results<Ok<GameStateResponse>, ProblemHttpResult>> GetGame(
        Guid id, GameRepository repository, CancellationToken cancellationToken)
    {
        var game = await repository.LoadGameAsync(id, cancellationToken);
        if (game is null)
            return TypedResults.Problem(title: "Game not found", statusCode: StatusCodes.Status404NotFound);

        return TypedResults.Ok(GameStateResponse.From(game));
    }

    // Player requests a cell to shoot, the opponent fires back.
    private static async Task<Results<Ok<FireShotResponse>, ProblemHttpResult>> FireShot(
        Guid id, FireShotRequest request, GameRepository repository, Shooter shooter, CancellationToken cancellationToken)
    {
        if (!Coordinate.TryParse(request.Cell, out var playerCell))
            return TypedResults.Problem(
                title: "Invalid cell",
                detail: $"'{request.Cell}' is not a cell.",
                statusCode: StatusCodes.Status400BadRequest);

        var game = await repository.LoadGameAsync(id, cancellationToken);
        if (game is null)
            return TypedResults.Problem(title: "Game not found", statusCode: StatusCodes.Status404NotFound);

        if (game.CurrentTurn != Side.Player)
            return TypedResults.Problem(title: "It is not the Player's turn", statusCode: StatusCodes.Status409Conflict);

        var playerResult = game.Shoot(playerCell);
        if (!playerResult.IsSuccess)
            return playerResult.Error.ToProblem();

        repository.AddShot(game, playerResult.Value, Side.Opponent);

        // Opponent shooting player
        ShotResponse? firedByOpponent = null;
        if (!game.IsOver)
        {
            var shotAtCell = shooter.RandomShotAtTargetSide(game.PlayerBoard, Random.Shared);
            if (shotAtCell is not null)
            {
                var opponentResult = game.Shoot(shotAtCell.Value);
                repository.AddShot(game, opponentResult.Value, Side.Player);
                firedByOpponent = ShotResponse.From(opponentResult.Value);
            }
        }

        // Save shots
        var saveResult = await repository.SaveChangesAsync(cancellationToken);
        if (!saveResult.IsSuccess)
            return saveResult.Error.ToProblem();

        return TypedResults.Ok(new FireShotResponse(
            ShotResponse.From(playerResult.Value),
            firedByOpponent,
            GameStateResponse.From(game)));
    }
}
