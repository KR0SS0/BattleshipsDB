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

    private static async Task<Results<Created<GameResponse>, ProblemHttpResult>> CreateGame(
        GameRepository repository, CreateGameRequest? request, CancellationToken cancellationToken)
    {
        var difficulty = request?.Difficulty ?? Game.DefaultDifficulty;
        if (!Enum.IsDefined(difficulty))
            return TypedResults.Problem(
                title: "Invalid difficulty",
                detail: $"'{difficulty}' is not a difficulty.",
                statusCode: StatusCodes.Status400BadRequest);

        Game game = new(difficulty);
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
        Guid id, FireShotRequest request, GameRepository repository, CancellationToken cancellationToken)
    {
        if (!Coordinate.TryParse(request.Cell, out var playerCell))
            return TypedResults.Problem(
                title: "Invalid cell",
                detail: $"'{request.Cell}' is not a cell.",
                statusCode: StatusCodes.Status400BadRequest);

        var game = await repository.LoadGameAsync(id, cancellationToken);
        if (game is null)
            return TypedResults.Problem(title: "Game not found", statusCode: StatusCodes.Status404NotFound);

        var roundResult = game.PlayRound(playerCell, Random.Shared);
        if (!roundResult.IsSuccess)
            return roundResult.Error.ToProblem();

        var round = roundResult.Value;
        repository.AddShot(game, round.PlayerShot, Side.Opponent);
        if (round.OpponentShot is not null)
            repository.AddShot(game, round.OpponentShot, Side.Player);

        // Save shots
        var saveResult = await repository.SaveChangesAsync(cancellationToken);
        if (!saveResult.IsSuccess)
            return saveResult.Error.ToProblem();

        return TypedResults.Ok(new FireShotResponse(
            ShotResponse.From(round.PlayerShot),
            round.OpponentShot is null ? null : ShotResponse.From(round.OpponentShot),
            GameStateResponse.From(game)));
    }
}
