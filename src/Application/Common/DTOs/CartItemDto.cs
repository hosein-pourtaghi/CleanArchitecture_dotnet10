using System.ComponentModel.DataAnnotations.Schema;
using Domain.Carts;

namespace Application.Common.DTOs;

/// <summary>
/// Data Transfer Object for Customer.
/// Used for API responses and query results.
/// </summary>
public sealed class CartItemDto
{
    public Guid CartId { get; set; }

    public int Quantity { get; set; } = 1;
    /// <summary>
    /// price of plan on purchase 
    /// </summary>
    public decimal OriginalPrice { get; set; }
    /// <summary>
    /// price after applying coupon if exists
    /// if not its OriginalPrice
    /// </summary>
    public decimal DiscountedPrice { get; set; }
    /// <summary>
    /// Discount amount that is applied to the original price in Currency(Dollar)
    /// </summary>
    /// <value></value>
    public decimal DiscountAmount { get; set; }
    /// <summary>
    /// applied tax for cartItem in dollar
    /// </summary>
    public decimal TaxPrice { get; set; } = 0;
    public string? TransactionId { get; set; }
    public string CouponCode { get; set; }
     

}
