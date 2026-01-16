using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Carts.Create;

/// <summary>
/// Command to create a new cart.
/// Returns the newly created cart's ID.
/// </summary>
public sealed record CreateCartCommand( 
    Guid CustomerId,
    string Currency,
    string? TransactionId,
    string? PaymentType,
    string? Code,
    DateTime PurchaseDate,
    List<CartItemDto> CartItems
    ) : ICommand<Guid>;




