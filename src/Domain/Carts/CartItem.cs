using SharedKernel;

namespace Domain.Carts;

public class CartItem : Entity
{
    public Guid Id { get; set; }
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


    #region Navigations
    public Cart Cart { get; set; }
    #endregion


}
