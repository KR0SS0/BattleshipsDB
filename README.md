# Battleship

[![CI/CD](https://github.com/KR0SS0/BattleshipsDB/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/KR0SS0/BattleshipsDB/actions/workflows/ci-cd.yml)

Battleship in the browser, built with C# and .NET 10.

**[Play it here](https://kr0ss0.github.io/BattleshipsDB/)**

I made this project more as an excersice and for fun. I wanted to write ASP.NET Core and with a database, PostgreSQL. I chose Battleship because it's a game everyone knows and there is enough to make it interesting.

I might still be updating the game, unless I've found a new project I'm interested in :>

## How to play

1. Pick a difficulty and press **New game**.
2. Place your five ships on the **Your fleet**-board. Left click places the next ship and right click rotates it.
3. Once all five are placed, the battle starts. Click a cell in the **Enemy waters**-board to fire, and the enemy fires back after every shot.
4. Sink all five enemy ships before they sink yours.

| Difficulty | How the enemy shoots |
|---|---|
| Easy | Random cells |
| Normal | Random until it hits something, then tries the cells around the hit |
| Hard | Like Normal, but once two hits line up it keeps going in that direction |

I measured the difference by simulating 250 games per opponent. To sink a whole fleet, Easy needs 95.2 shots on average, Normal 62.8 and Hard 59.3.

## How it's built

- `Battleship.Domain` has the game rules: board, ships, shots, turns and the opponents. It has no dependencies, so the same code runs on the server and in the browser.
- `Battleship.Web` is the game you play, a Blazor WebAssembly app hosted on GitHub Pages.
- `Battleship.Api` is an ASP.NET Core minimal API to create games, load them and fire shots. Errors come back as ProblemDetails (400, 404, 409, 422).
- `Battleship.Infrastructure` stores games in PostgreSQL with EF Core. Every shot is its own row, and a game is loaded by replaying its shots in order. If two requests fire at the same game at once, the second one gets a 409 instead of corrupting the game.
- `tests` has xUnit tests for the rules, and integration tests that run the API against PostgreSQL in Docker using Testcontainers.

GitHub Actions builds and tests everything on every push, and only deploys the game if the tests pass.

## Use of AI

I used Claude Code.

- `Battleship.Domain`, `Battleship.Infrastructure` and `Battleship.Api` are my own code. I let AI review my work to find issues or gaps, and to optimize it by shortening methods while keeping my logic.
- `Battleship.Web` was mostly written by AI: the layout, styling, animations and sound.
- For tests, I wrote the tests for a method myself and then let AI write the missing ones.

## Credits

- Font: Black Ops One, SIL Open Font License
- Icon: "frigate" from [IconPark](https://github.com/bytedance/IconPark) by ByteDance, Apache License 2.0
