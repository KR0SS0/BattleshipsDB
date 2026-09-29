using Battleship.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Battleship.Api;

public static class ErrorExtensions
{
    public static ProblemHttpResult ToProblem(this Error error) =>
        TypedResults.Problem(
            title: error.Message,
            statusCode: error.Type switch
            {
                ErrorType.Invalid => StatusCodes.Status422UnprocessableEntity,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error.Type, null)
            },
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
