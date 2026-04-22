// FileStorage.Domain/Enums/AccessLevel.cs
namespace FileStorage.Domain.Enums;

/// <summary>
/// Defines the access level for file attachments.
/// </summary>
public enum AccessLevel
{
    /// <summary>
    /// Anyone can access without authentication (e.g., product images)
    /// </summary>
    Public = 0,

    /// <summary>
    /// Any authenticated user can access
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// Only owner or explicitly granted users can access
    /// </summary>
    Private = 2,

    /// <summary>
    /// Role-based access (Admin, Manager, etc.)
    /// </summary>
    Restricted = 3
}
