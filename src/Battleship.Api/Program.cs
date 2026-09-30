using Battleship.Api;
using Battleship.Domain;
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
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
app.MapGameEndpoints();

app.Run();

