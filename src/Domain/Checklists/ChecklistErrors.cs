using SharedKernel;

namespace Domain.Checklists;

/// <summary>
/// Centralized error definitions for Checklist domain operations.
/// Follows domain-driven design patterns for error handling.
/// </summary>
public static class ChecklistErrors
{
    public static Error NotFound(Guid customerId) =>
        Error.NotFound(
            "Checklists.NotFound",
            $"Checklist with ID '{customerId}' was not found.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict(
            "Checklists.EmailExists",
            $"Checklist with email '{email}' already exists.");

    public static Error InvalidEmail() =>
        Error.Problem(
            "Checklists.InvalidEmail",
            "The provided email address is invalid.");

    public static Error InvalidName() =>
        Error.Problem(
            "Checklists.InvalidName",
            "Checklist name is required and must be between 1 and 200 characters.");

    public static Error CannotDeleteChecklist() =>
        Error.Failure(
            "Checklists.CannotDelete",
            "This customer cannot be deleted due to existing checklists.");
}
