using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Infrastructure;

namespace Web.Api.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Transforms a Result into a value using provided success and failure handlers.
    /// </summary>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    /// <summary>
    /// Transforms a Result<TIn> into a value using provided success and failure handlers.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }

    /// <summary>
    /// Converts a Result to IActionResult with default behavior.
    /// Success returns NoContent (204), Failure returns Problem.
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        var problemResult = CustomResults.Problem(result);
        return new ObjectResult(problemResult) { StatusCode = GetStatusCode(result.Error.Type) };
    }

    /// <summary>
    /// Converts a Result<TValue> to IActionResult with default behavior.
    /// Success returns Ok(value) (200), Failure returns Problem.
    /// </summary>
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        var problemResult = CustomResults.Problem(result);
        return new ObjectResult(problemResult) { StatusCode = GetStatusCode(result.Error.Type) };
    }

    /// <summary>
    /// Converts a Result<TValue> to IActionResult with custom success handler.
    /// </summary>
    public static IActionResult ToActionResult<TValue>(
        this Result<TValue> result,
        Func<TValue, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        var problemResult = CustomResults.Problem(result);
        return new ObjectResult(problemResult) { StatusCode = GetStatusCode(result.Error.Type) };
    }

    /// <summary>
    /// Converts a Result to IActionResult with custom success handler.
    /// </summary>
    public static IActionResult ToActionResult(
        this Result result,
        Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess();
        }

        var problemResult = CustomResults.Problem(result);
        return new ObjectResult(problemResult) { StatusCode = GetStatusCode(result.Error.Type) };
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
}
