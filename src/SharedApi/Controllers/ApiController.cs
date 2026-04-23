using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedApi.Extensions;
using SharedKernel;

namespace SharedApi.Controllers;

/// <summary>
/// Base controller class providing consistent error handling and response formatting.
/// Follows Clean Architecture principles with proper separation of concerns.
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Converts a successful Result into NoContent (204) or ProblemDetails on failure.
    /// </summary>
    protected IActionResult HandleResult(Result result) => result.ToActionResult();

    /// <summary>
    /// Converts a successful Result<TValue> into Ok (200) or ProblemDetails on failure.
    /// </summary>
    protected IActionResult HandleResult<TValue>(Result<TValue> result) =>
        result.ToActionResult();

    /// <summary>
    /// Converts a successful Result<TValue> into a custom response.
    /// </summary>
    protected IActionResult HandleResult<TValue>(
        Result<TValue> result,
        Func<TValue, IActionResult> onSuccess) =>
        result.ToActionResult(onSuccess);

    /// <summary>
    /// Converts a Result to a custom response with success and failure handlers.
    /// </summary>
    protected IActionResult HandleResult(
        Result result,
        Func<IActionResult> onSuccess) =>
        result.ToActionResult(onSuccess);


    /// <summary>
    /// Converts a successful Result<TValue> to Created (201) response.
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue>(
        Result<TValue> result,
        string routeName,
        object routeValues) =>
        result.ToCreatedResult(routeName, routeValues);

    /// <summary>
    /// Converts a successful Result<TValue> to Created (201) response using action name.
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue>(
        Result<TValue> result,
        string actionName,
        string controllerName,
        object? routeValues = null) =>
        result.ToCreatedResult(actionName, controllerName, routeValues);

    /// <summary>
    /// Converts a successful Result to Created (201) with auto-generated route values.
    /// Usage: HandleCreatedResult(result, "GetById", x => x.Id)
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue, TId>(
        Result<TValue> result,
        string actionName,
        Func<TValue, TId> idSelector)
    {
        if (result.IsFailure)
            return result.Error.ToProblemDetails();

        var id = idSelector(result.Value);
        var routeValues = new { id };

        return CreatedAtAction(actionName, routeValues, result.Value);
    }

    /// <summary>
    /// Converts a successful Result to Created (201) with controller and id selector.
    /// Usage: HandleCreatedResult(result, "GetById", "Users", x => x.Id)
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue, TId>(
        Result<TValue> result,
        string actionName,
        string controllerName,
        Func<TValue, TId> idSelector)
    {
        if (result.IsFailure)
            return result.Error.ToProblemDetails();

        var id = idSelector(result.Value);

        return CreatedAtAction(
            actionName,
            controllerName,
            new { id },
            result.Value);
    }

    /// <summary>
    /// Handles paginated results with X-Pagination header.
    /// </summary>
    protected IActionResult HandlePaginatedResult<TDto>(
        Result<PaginatedResult<TDto>> result)
    {
        if (result.IsFailure)
            return result.Error.ToProblemDetails();

        var paginationInfo = result.Value;

        Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
        {
            paginationInfo.TotalCount,
            paginationInfo.Page,
            paginationInfo.PageSize
        }));

        return Ok(result.Value.Items);
    }

    /// <summary>
    /// Handles paginated results with custom mapping.
    /// </summary>
    protected IActionResult HandlePaginatedResult<TDto, TResponse>(
        Result<PaginatedResult<TDto>> result,
        Func<IReadOnlyList<TDto>, TResponse> mapper)
    {
        if (result.IsFailure)
            return result.Error.ToProblemDetails();

        var paginationInfo = result.Value;

        Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
        {
            paginationInfo.TotalCount,
            paginationInfo.Page,
            paginationInfo.PageSize
        }));

        return Ok(mapper(result.Value.Items));
    }

    /// <summary>
    /// Validates the model state and returns ProblemDetails if invalid.
    /// </summary>
    protected IActionResult? ValidateModelState()
    {
        if (ModelState.IsValid)
            return null;

        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        return BadRequest(problemDetails);
    }
}
