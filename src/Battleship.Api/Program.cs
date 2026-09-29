using Battleship.Api;
using Battleship.Domain;
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddDbContextPool<BattleshipDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BattleShipDbContext")));
builder.Services.AddScoped<GameRepository>();
builder.Services.AddSingleton<Shooter>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapPost("/games", async (GameRepository repository, CancellationToken cancellationToken) =>
{
    Game game = new();
    game.PlayerBoard.TryPlaceShipsRandomly(Random.Shared);
    game.OpponentBoard.TryPlaceShipsRandomly(Random.Shared);

    await repository.SaveGameAsync(game, cancellationToken);

    GameResponse body = new GameResponse(game.Id);
    var address = $"/games/{game.Id}";

    return TypedResults.Created(address, body);
});

app.MapGet("/games/{id:guid}", async Task<Results<Ok<GameStateResponse>, ProblemHttpResult>> (
    Guid id, GameRepository repository, CancellationToken cancellationToken) =>
{
    var game = await repository.LoadGameAsync(id, cancellationToken);
    if (game is null)
        return TypedResults.Problem(title: "Game not found", statusCode: StatusCodes.Status404NotFound);

    return TypedResults.Ok(GameStateResponse.From(game));
});

// Player requests a cell to shoot, the opponent fires back.
app.MapPost("/games/{id:guid}/shots", async Task<Results<Ok<FireShotResponse>, ProblemHttpResult>> (
    Guid id, FireShotRequest request, GameRepository repository, Shooter shooter, CancellationToken cancellationToken) =>
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
});

app.Run();

