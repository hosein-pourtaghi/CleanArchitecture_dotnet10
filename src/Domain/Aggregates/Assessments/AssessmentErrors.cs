using SharedKernel;

namespace Domain.Aggregates.Assessments;

/// <summary>
/// Centralized error definitions for Assessment domain operations.
/// Follows domain-driven design patterns for error handling.
/// </summary>
public static class AssessmentErrors
{
    public static Error NotFound(Guid customerId) =>
        Error.NotFound(
            "Assessments.NotFound",
            $"Assessment with ID '{customerId}' was not found.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict(
            "Assessments.EmailExists",
            $"Assessment with email '{email}' already exists.");

    public static Error InvalidEmail() =>
        Error.Problem(
            "Assessments.InvalidEmail",
            "The provided email address is invalid.");

    public static Error InvalidName() =>
        Error.Problem(
            "Assessments.InvalidName",
            "Assessment name is required and must be between 1 and 200 characters.");

    public static Error CannotDeleteAssessment() =>
        Error.Failure(
            "Assessments.CannotDelete",
            "This customer cannot be deleted due to existing assessments.");
}
