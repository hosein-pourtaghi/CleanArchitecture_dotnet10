using global::SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SharedApi.Extensions;

/// <summary>
/// Extension methods for converting Result to IActionResult
/// </summary>
public static class ResultToActionResultExtensions
{
    // ==================== Non-Generic Result ====================

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return result.Error.ToProblemDetails();
    }

    public static IActionResult ToActionResult(
        this Result result,
        Func<IActionResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess() : result.Error.ToProblemDetails();
    }

    // ==================== Generic Result<T> ====================

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.Error.ToProblemDetails();
    }

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : result.Error.ToProblemDetails();
    }

    // ==================== Created Results ====================

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string routeName,
        object routeValues)
    {
        if (result.IsFailure)
            return result.Error.ToProblemDetails();

        return new CreatedAtRouteResult(routeName, routeValues, result.Value);
    }

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        string controllerName,
        object? routeValues = null)
    {
        if (result.IsFailure)
            return result.Error.ToProblemDetails();

        return new CreatedAtActionResult(actionName, controllerName, routeValues, result.Value);
    }

    // ==================== Error to ProblemDetails ====================

    public static ObjectResult ToProblemDetails(this Error error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = GetTitle(error.Type),
            Detail = error.Description,
            Status = GetStatusCode(error.Type)
        };

        if (!string.IsNullOrEmpty(error.Code))
        {
            problemDetails.Extensions["code"] = error.Code;
            problemDetails.Extensions["errorCode"] = error.Code;
        }

        problemDetails.Extensions["type"] = error.Type.ToString();

        return new ObjectResult(problemDetails)
        {
            StatusCode = GetStatusCode(error.Type)
        };
    }

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.None => "Success",
        ErrorType.Failure => "An error occurred",
        ErrorType.NotFound => "Resource not found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Validation => "Validation error",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Problem => "Problem",
        _ => "Error"
    };

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.None => StatusCodes.Status200OK,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Problem => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}
