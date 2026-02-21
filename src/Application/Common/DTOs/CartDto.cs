using System.ComponentModel.DataAnnotations.Schema;
using Domain.Carts;

namespace Application.Common.DTOs;

/// <summary>
/// Data Transfer Object for Customer.
/// Used for API responses and query results.
/// </summary>
public sealed class CartDto
{
    public Guid CustomerId { get; set; }

    public decimal TotalOriginalPrice { get => CartItems.Sum(x => x.OriginalPrice); }
    public decimal TotalDiscountedPrice { get => CartItems.Sum(x => x.DiscountedPrice); }
    public decimal TotalDiscountAmount { get => CartItems.Sum(x => x.DiscountAmount); }
    public string Currency { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentType { get; set; }
    /// <summary>
    /// invoice code
    /// </summary>
    public string? Code { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int CartItemCount { get =>  CartItems.Count; }  



    #region Navigations 
    public List<CartItemDto> CartItems { get; set; } = new();
    #endregion


}
