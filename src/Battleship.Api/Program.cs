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

app.Run();

