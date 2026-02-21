using System.ComponentModel.DataAnnotations.Schema;
using Domain.Customers;
using SharedKernel;

namespace Domain.Carts;

public class Cart : Entity
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    [NotMapped]
    public decimal TotalOriginalPrice { get => CartItems.Sum(x => x.OriginalPrice); }
    [NotMapped]
    public decimal TotalDiscountedPrice { get => CartItems.Sum(x => x.DiscountedPrice); }
    [NotMapped]
    public decimal TotalDiscountAmount { get => CartItems.Sum(x => x.DiscountAmount); }
    public string Currency { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentType { get; set; }
    /// <summary>
    /// invoice code
    /// </summary>
    public string? Code { get; set; }
    public DateTime PurchaseDate { get; set; } 



    #region Navigations 
    public virtual List<CartItem> CartItems { get; set; } = new();
    #endregion


}
