namespace Battleship.Domain;

public sealed record Error(ErrorType Type, string Code, string Message)
{

}