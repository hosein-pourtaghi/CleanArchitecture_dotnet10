using Application.Common.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using WebApi.Extensions;

namespace WebApi.Controllers;

/// <summary>
/// Base controller class providing convenient methods for converting Result objects to IActionResult.
/// This ensures consistent error handling and response formatting across all controllers.
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Converts a successful Result into a NoContent (204) response.
    /// </summary>
    protected IActionResult HandleResult(Result result) => result.ToActionResult();

    /// <summary>
    /// Converts a successful Result<TValue> into an Ok (200) response.
    /// </summary>
    protected IActionResult HandleResult<TValue>(Result<TValue> result) => result.ToActionResult();

    /// <summary>
    /// Converts a successful Result<TValue> into a custom response.
    /// </summary>
    protected IActionResult HandleResult<TValue>(
        Result<TValue> result,
        Func<TValue, IActionResult> onSuccess) => result.ToActionResult(onSuccess);

    /// <summary>
    /// Converts a Result to a custom response with success and failure handlers.
    /// </summary>
    protected IActionResult HandleResult(
        Result result,
        Func<IActionResult> onSuccess) => result.ToActionResult(onSuccess);

    /// <summary>
    /// Converts a successful Result<TValue> to Created (201) response.
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue>(
        Result<TValue> result,
        string routeName,
        object routeValues) =>
        result.ToActionResult(value => CreatedAtRoute(routeName, routeValues, value));

    /// <summary>
    /// Converts a successful Result<Guid> to Created (201) with standard get-by-id route.
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue>(
        Result<TValue> result,
        string controllerName,
        Func<TValue, Guid> idSelector) =>
        result.ToActionResult(value => CreatedAtAction(
            "GetById",
            new { id = idSelector(value) },
            value));

    /// <summary>
    /// Handle paginated results with X-Pagination header
    /// </summary>
    protected IActionResult HandlePaginatedResult<TDto>(
        Result<PaginatedResult<TDto>> result)
    {
        var actionResult = result.ToActionResult();

        if (result.IsSuccess && result.Value != null)
        {
            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
            {
                result.Value.TotalCount,
                result.Value.Page,
                result.Value.PageSize
            }));
        }

        return actionResult;
    }
}

