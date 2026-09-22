namespace Battleship.Domain.Tests;

public class GameTests
{
    [Fact]
    public void NewGame_CurrentTurnIsPlayer()
    {
        var game = new Game();

        Assert.Equal(Side.Player, game.CurrentTurn);
    }

    [Fact]
    public void NewGame_HasNoWinnerAndIsNotOver()
    {
        var game = new Game();

        Assert.Null(game.Winner);
        Assert.False(game.IsOver);
    }

}

