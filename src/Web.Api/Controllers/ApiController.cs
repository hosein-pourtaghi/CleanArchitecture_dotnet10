using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;

namespace Web.Api.Controllers;

/// <summary>
/// Base controller class providing convenient methods for converting Result objects to IActionResult.
/// This ensures consistent error handling and response formatting across all controllers.
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Converts a successful Result into a NoContent (204) response.
    /// If the result is a failure, returns a Problem response with error details.
    /// </summary>
    protected IActionResult HandleResult(Result result) =>
        result.ToActionResult();

    /// <summary>
    /// Converts a successful Result<TValue> into an Ok (200) response with the value.
    /// If the result is a failure, returns a Problem response with error details.
    /// </summary>
    protected IActionResult HandleResult<TValue>(Result<TValue> result) =>
        result.ToActionResult();

    /// <summary>
    /// Converts a successful Result<TValue> into a custom response.
    /// If the result is a failure, returns a Problem response with error details.
    /// </summary>
    /// <param name="result">The result to handle</param>
    /// <param name="onSuccess">Custom handler for success cases</param>
    protected IActionResult HandleResult<TValue>(
        Result<TValue> result,
        Func<TValue, IActionResult> onSuccess) =>
        result.ToActionResult(onSuccess);

    /// <summary>
    /// Converts a Result to a custom response with success and failure handlers.
    /// Provides maximum flexibility for non-standard response patterns.
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
        string? routeName = null,
        object? routeValues = null) =>
        result.ToActionResult(value =>
            routeName is not null
                ? CreatedAtRoute(routeName, routeValues, value)
                : Created(string.Empty, value));

    /// <summary>
    /// Converts a successful Result<Guid> to Created (201) response with standard route.
    /// </summary>
    protected IActionResult HandleCreatedResult<TValue>(
        Result<TValue> result,
        string controllerName,
        Guid id) =>
        result.ToActionResult(value =>
            CreatedAtAction(nameof(GetById), new { id }, value));

    /// <summary>
    /// Dummy method to support nameof() in HandleCreatedResult.
    /// Override this method signature in derived controllers as needed.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public virtual IActionResult GetById(Guid id) =>
        NotFound();
}
