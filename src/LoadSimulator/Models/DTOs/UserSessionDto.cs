using System.ComponentModel.DataAnnotations.Schema;

namespace LoadSimulator.Models.DTOs;

/// <summary>
/// Represents a simulated user session
/// </summary>
public class UserSessionDto
{
    public string UserId { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string JwtToken { get; set; } = string.Empty;
    
    public DateTime LoginTime { get; set; }
    
    public DateTime TokenExpiryTime { get; set; }
    
    public bool IsTokenValid => DateTime.UtcNow < TokenExpiryTime;
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Number { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }


    public string ManufacturePart { get; set; }
    public string ShortDesc { get; set; }
    public string Barcode { get; set; }
    public bool DLC { get; set; }
    public bool UL { get; set; }
    public bool ETL { get; set; }
    public bool ES { get; set; }
    public bool CUL { get; set; }
    public bool CETL { get; set; }
    public string IP { get; set; }
    public string SKU { get; set; }
    public string SPC { get; set; }
    public string ManufactorerPartNumber { get; set; }
    public int? IncomeAccountID { get; set; }
    public string Image { get; set; }
    public int? ReorderPoint { get; set; }

    public decimal? Price { get; set; }
    public bool Taxable { get; set; }
    public bool Salable { get; set; }
    public bool Enable { get; set; }
    public bool BatchTraceable { get; set; }
    public bool? Deleted { get; set; }
    public bool EccomerceViewable { get; set; }
    public bool EccomerceSalable { get; set; }
    public string Notes { get; set; }
    public float? WeightKg { get; set; }
    public float? LengthCm { get; set; }
    public float? WidthCm { get; set; }
    public float? HeightCm { get; set; }
    public string AlternateName { get; set; }


    public int? PreferredSupplierId { get; set; }
    public float MaximumStockLevel { get; set; }
    public float MinimumStockLevel { get; set; }
    public float PurchasePrice { get; set; }
}

public class CartDto
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

    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;



    public List<CartItemDto> CartItems { get; set; } = new();
    
}

public class CartItemDto
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
    public Guid ProductId { get; set; }


}
