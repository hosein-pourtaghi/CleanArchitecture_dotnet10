using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Carts.Update;

/// <summary>
/// Command to update an existing cart.
/// No return value - operation either succeeds or fails.
/// </summary>
public sealed record UpdateCartCommand(
    Guid Id,
    Guid CustomerId,
    string Currency,
    string? TransactionId,
    string? PaymentType,
    string? Code,
    DateTime PurchaseDate,
    List<CartItemDto> CartItems) : ICommand;
