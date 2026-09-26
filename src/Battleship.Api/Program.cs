using Battleship.Api;
using Battleship.Domain;
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddDbContextPool<BattleshipDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BattleShipDbContext")));
builder.Services.AddScoped<GameRepository>();

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

app.Run();

