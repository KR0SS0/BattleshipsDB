namespace Battleship.Domain;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    public bool IsSuccess => _error is null;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read Value of a failed result.");

    public Error Error => _error
        ?? throw new InvalidOperationException("Cannot read Error of a successful result.");

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, error);
    }
}
