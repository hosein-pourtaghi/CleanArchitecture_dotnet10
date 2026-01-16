using Application.Abstractions.Messaging;

namespace Application.Carts.Copy;

/// <summary>
/// Command to Copy a new cart.
/// Returns the newly Copied cart's ID.
/// </summary>
public sealed record CopyCartCommand( ) : ICommand<bool>;
