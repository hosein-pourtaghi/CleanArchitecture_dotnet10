using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;
 

public interface ICurrentUserService
{
    /// <summary>
    /// Current user ID (null if not authenticated)
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Current user email
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Current user roles
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Check if current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Check if current user has specific role
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Check if current user has any of the specified roles
    /// </summary>
    bool HasAnyRole(params string[] roles);
}
