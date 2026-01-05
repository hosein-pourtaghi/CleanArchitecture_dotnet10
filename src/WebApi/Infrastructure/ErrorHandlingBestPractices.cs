using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace WebApi.Infrastructure;

/// <summary>
/// Best practices for error handling in controllers.
/// This file documents the recommended patterns for handling Result types.
/// </summary>
public static class ErrorHandlingBestPractices
{
    /// <summary>
    /// BEST PRACTICE PATTERN 1: Use the ApiController base class
    /// 
    /// The ApiController provides helper methods that automatically handle Result-to-IActionResult conversion.
    /// This is the recommended approach for all API controllers.
    /// 
    /// Example:
    /// <code>
    /// [Route("customers")]
    /// [Authorize]
    /// public class CustomersController : ApiController
    /// {
    ///     [HttpGet]
    ///     public async Task<IActionResult> GetCustomers(
    ///         IQueryHandler<GetCustomersQuery, List<CustomerDto>> handler,
    ///         CancellationToken cancellationToken)
    ///     {
    ///         var result = await handler.Handle(new GetCustomersQuery(), cancellationToken);
    ///         return HandleResult(result);  // Automatically returns Ok(data) on success or Problem on failure
    ///     }
    /// }
    /// </code>
    /// </summary>
    public static class Pattern1_UseApiControllerBaseClass { }

    /// <summary>
    /// BEST PRACTICE PATTERN 2: Explicit result type conversion with ToActionResult()
    /// 
    /// Use the ToActionResult extension methods for explicit, self-documenting result handling.
    /// 
    /// Example:
    /// <code>
    /// var result = await handler.Handle(command, cancellationToken);
    /// return result.ToActionResult();  // Success -> NoContent, Failure -> Problem
    /// 
    /// var queryResult = await handler.Handle(query, cancellationToken);
    /// return queryResult.ToActionResult();  // Success -> Ok(value), Failure -> Problem
    /// </code>
    /// </summary>
    public static class Pattern2_UseToActionResultExtensions { }

    /// <summary>
    /// BEST PRACTICE PATTERN 3: Custom success responses
    /// 
    /// When standard Ok/NoContent responses don't match your use case, provide custom handlers.
    /// 
    /// Example:
    /// <code>
    /// var result = await handler.Handle(command, cancellationToken);
    /// return result.ToActionResult(
    ///     onSuccess: id => CreatedAtAction(nameof(GetById), new { id }, null),
    ///     onFailure: e => CustomResults.Problem(e)
    /// );
    /// </code>
    /// </summary>
    public static class Pattern3_CustomSuccessResponses { }

    /// <summary>
    /// ERROR HANDLING CHARACTERISTICS:
    /// 
    /// 1. VALIDATION ERRORS (ErrorType.Validation)
    ///    - HTTP Status: 400 Bad Request
    ///    - Response: Includes detailed validation error information
    ///    - Use Case: Invalid request parameters, failed data validation
    /// 
    /// 2. NOT FOUND ERRORS (ErrorType.NotFound)
    ///    - HTTP Status: 404 Not Found
    ///    - Response: Standard problem response with error details
    ///    - Use Case: Resource doesn't exist
    /// 
    /// 3. CONFLICT ERRORS (ErrorType.Conflict)
    ///    - HTTP Status: 409 Conflict
    ///    - Response: Standard problem response with error details
    ///    - Use Case: Resource already exists, business rule violation
    /// 
    /// 4. PROBLEM ERRORS (ErrorType.Problem)
    ///    - HTTP Status: 400 Bad Request
    ///    - Response: Standard problem response with error details
    ///    - Use Case: Business logic failures, invalid operations
    /// 
    /// 5. FAILURE ERRORS (ErrorType.Failure)
    ///    - HTTP Status: 500 Internal Server Error
    ///    - Response: Generic error message
    ///    - Use Case: Unexpected application errors
    /// </summary>
    public static class ErrorTypeMapping { }

    /// <summary>
    /// RECOMMENDED RESPONSE PATTERNS:
    /// 
    /// 1. GET SINGLE RESOURCE:
    ///    Success -> 200 Ok(data)
    ///    Not Found -> 404 Not Found
    ///    Validation Error -> 400 Bad Request
    /// 
    /// 2. GET COLLECTION:
    ///    Success -> 200 Ok(data[])
    ///    Validation Error -> 400 Bad Request
    /// 
    /// 3. POST (Create):
    ///    Success -> 201 Created(data) with Location header
    ///    Conflict -> 409 Conflict
    ///    Validation Error -> 400 Bad Request
    /// 
    /// 4. PUT (Update):
    ///    Success -> 204 No Content
    ///    Not Found -> 404 Not Found
    ///    Conflict -> 409 Conflict
    ///    Validation Error -> 400 Bad Request
    /// 
    /// 5. DELETE:
    ///    Success -> 204 No Content
    ///    Not Found -> 404 Not Found
    /// </summary>
    public static class RecommendedStatusCodes { }

    /// <summary>
    /// ANTI-PATTERNS TO AVOID:
    /// 
    /// 1. ANTI-PATTERN: Checking IsSuccess/IsFailure manually
    ///    ❌ Bad:
    ///    if (result.IsSuccess) return Ok(result.Value);
    ///    else return CustomResults.Problem(result);
    ///    
    ///    ✅ Good:
    ///    return result.ToActionResult();
    /// 
    /// 2. ANTI-PATTERN: Unchecked result.Value access
    ///    ❌ Bad:
    ///    var value = result.Value;  // Throws if IsFailure
    ///    
    ///    ✅ Good:
    ///    return result.ToActionResult(v => Ok(v));
    /// 
    /// 3. ANTI-PATTERN: Mixed error handling approaches
    ///    ❌ Bad:
    ///    Some methods use Match, some use ToActionResult, some use Result.Success
    ///    
    ///    ✅ Good:
    ///    Consistently use ApiController.HandleResult() throughout
    /// 
    /// 4. ANTI-PATTERN: Swallowing errors
    ///    ❌ Bad:
    ///    catch (Exception) { return Ok(); }
    ///    
    ///    ✅ Good:
    ///    Let unhandled exceptions propagate to GlobalExceptionHandler
    /// </summary>
    public static class AntiPatterns { }
}
