using Battleship.Domain;

namespace Battleship.Infrastructure;

public static class GameMapper
{
    public static GameEntity ToNewEntity(Game game)
    {
        var now = DateTimeOffset.UtcNow;

        var entity = new GameEntity
        {
            Id = game.Id,
            CreatedAt = now,
            CurrentTurn = game.CurrentTurn,
        };

        entity.Ships.AddRange(ToShipEntities(game.PlayerBoard, Side.Player, now));
        entity.Ships.AddRange(ToShipEntities(game.OpponentBoard, Side.Opponent, now));

        return entity;
    }

    private static IEnumerable<ShipEntity> ToShipEntities(Board board, Side side, DateTimeOffset createdAt)
    {
        return board.Ships.Select(ship => new ShipEntity
        {
            Type = ship.Type,
            Orientation = ship.Orientation,
            StartColumn = ship.Start.Column,
            StartRow = ship.Start.Row,
            OnSide = side,
            CreatedAt = createdAt,
        });
    }

    public static ShotEntity ToShotEntity(Shot shot, Side targetSide, Coordinate coordinate, int sequence)
    {
        return new ShotEntity
        {
            Sequence = sequence,
            TargetedSide = targetSide,
            Column = coordinate.Column,
            Row = coordinate.Row,
            Outcome = shot.Outcome,
            SunkShipType = shot.SunkShipType,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public static Game ToDomain(GameEntity entity)
    {
        var game = new Game(entity.Id);

        foreach (var shipEntity in entity.Ships)
        {
            var board = shipEntity.OnSide == Side.Player ? game.PlayerBoard : game.OpponentBoard;
            var ship = new Ship(
                shipEntity.Type,
                new Coordinate(shipEntity.StartColumn, shipEntity.StartRow),
                shipEntity.Orientation);

            var placeResult = board.PlaceShip(ship);
            if (!placeResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Failed to rehydrate game {entity.Id}: could not place ship {shipEntity.Type} ({placeResult.Error.Code}).");
        }

        foreach (var shotEntity in entity.Shots.OrderBy(s => s.Sequence))
        {
            var coordinate = new Coordinate(shotEntity.Column, shotEntity.Row);
            var shootResult = game.Shoot(coordinate);
            if (!shootResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Failed to rehydrate game {entity.Id}: could not replay shot at {coordinate} ({shootResult.Error.Code}).");
        }

        return game;
    }
}
