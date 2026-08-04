using Ordivo.SharedKernel.Results;
namespace Ordivo.Api.Common;
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result) => result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error.Code switch
        {
            "validation" => Results.BadRequest(new { error = result.Error }),
            "not_found" => Results.NotFound(new { error = result.Error }),
            "conflict" => Results.Conflict(new { error = result.Error }),
            "unauthorized" => Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status401Unauthorized),
            _ => Results.Problem(result.Error.Description)
        };
}
