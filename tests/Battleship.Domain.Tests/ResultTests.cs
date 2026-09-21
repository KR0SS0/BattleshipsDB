namespace Battleship.Domain.Tests;

public class ResultTests
{
    private static readonly Error TestError = new(ErrorType.Invalid, "test.code", "Test message");

    [Fact]
    public void Success_HasValueAndNoError()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_HasErrorAndIsNotSuccess()
    {
        var result = Result<int>.Failure(TestError);

        Assert.False(result.IsSuccess);
        Assert.Equal(TestError, result.Error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<int>.Failure(TestError);

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() => result.Error);
    }
}
