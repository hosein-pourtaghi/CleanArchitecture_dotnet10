using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Products;
 

///// <summary>
///// Represents a customer in the e-commerce system
///// Can be individual or business customer
///// </summary>
//public class Customer : Entity
//{
//    public Guid Id { get; set; }
//    public string Email { get; set; } = string.Empty;
//    public string PhoneNumber { get; set; } = string.Empty;

//    public string FirstName { get; set; } = string.Empty;
//    public string LastName { get; set; } = string.Empty;
//    public string FullName => $"{FirstName} {LastName}";

//    public DateTime? DateOfBirth { get; set; }
//    public Gender? Gender { get; set; }

//    public CustomerType Type { get; set; } = CustomerType.Individual;
//    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

//    // Authentication
//    public string PasswordHash { get; set; } = string.Empty;
//    public bool EmailVerified { get; set; }
//    public bool PhoneVerified { get; set; }
//    public DateTime? LastLoginAt { get; set; }
//    public string? LastLoginIp { get; set; }

//    // Preferences
//    public string PreferredLanguage { get; set; } = "en";
//    public string PreferredCurrency { get; set; } = "USD";
//    public string? TimeZone { get; set; }

//    // Marketing
//    public bool SubscribeToNewsletter { get; set; }
//    public bool SubscribeToSms { get; set; }
//    public bool SubscribeToPush { get; set; }

//    // Stats
//    public int TotalOrders { get; set; }
//    public decimal TotalSpent { get; set; }
//    public int TotalProductsReviewed { get; set; }
//    public int RewardPoints { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? UpdatedAt { get; set; }
//    public DateTime? LastPurchaseAt { get; set; }

//    // Navigation
//    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
//    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
//    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
//    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
//    public virtual ICollection<ShoppingCart> Carts { get; set; } = new List<ShoppingCart>();
//}

//// Domain/Entities/Customers/Address.cs
//namespace YourProject.Domain.Aggregates;

///// <summary>
///// Represents a customer address (shipping or billing)
///// </summary>
//public class Address : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    public AddressType Type { get; set; } = AddressType.Shipping;

//    public string FirstName { get; set; } = string.Empty;
//    public string LastName { get; set; } = string.Empty;
//    public string Company { get; set; } = string.Empty;
//    public string PhoneNumber { get; set; } = string.Empty;

//    public string AddressLine1 { get; set; } = string.Empty;
//    public string AddressLine2 { get; set; } = string.Empty;
//    public string AddressLine3 { get; set; } = string.Empty;

//    public string City { get; set; } = string.Empty;
//    public string State { get; set; } = string.Empty;
//    public string PostalCode { get; set; } = string.Empty;
//    public string Country { get; set; } = string.Empty;
//    public string CountryCode { get; set; } = string.Empty; // ISO 2-letter code

//    public double? Latitude { get; set; }
//    public double? Longitude { get; set; }

//    public string? DeliveryInstructions { get; set; }
//    public string? Landmark { get; set; }

//    public bool IsDefault { get; set; }
//    public bool IsVerified { get; set; }

//    public string FullAddress => $"{AddressLine1}, {AddressLine2}, {City}, {State} {PostalCode}, {Country}";
//}

//// Domain/Entities/Customers/Wishlist.cs
//namespace YourProject.Domain.Aggregates;

///// <summary>
///// Customer wishlist for saving products for later
///// </summary>
//public class Wishlist : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    public string Name { get; set; } = "My Wishlist"; // Can have multiple wishlists
//    public bool IsPublic { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//    public virtual ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
//}

//// Domain/Entities/Customers/WishlistItem.cs
//namespace YourProject.Domain.Aggregates;

///// <summary>
///// Individual item in a wishlist
///// </summary>
//public class WishlistItem : Entity
//{
//    public Guid Id { get; set; }
//    public Guid WishlistId { get; set; }
//    public virtual Wishlist Wishlist { get; set; } = null!;

//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public int Quantity { get; set; } = 1;
//    public string? Note { get; set; } // e.g., "For birthday"

//    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? PurchasedAt { get; set; }
//}

//// Domain/Entities/Customers/ShoppingCart.cs
//namespace YourProject.Domain.Aggregates;

///// <summary>
///// Customer shopping cart
///// </summary>
//public class ShoppingCart : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    public string SessionId { get; set; } = string.Empty; // For guest carts
//    public Guid? GuestId { get; set; }

//    public decimal Subtotal { get; set; }
//    public decimal Discount { get; set; }
//    public decimal Total { get; set; }
//    public string Currency { get; set; } = "USD";

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? ExpiresAt { get; set; }

//    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

//    public void RecalculateTotals()
//    {
//        Subtotal = Items.Sum(i => i.TotalPrice);
//        Discount = Items.Sum(i => i.Discount);
//        Total = Subtotal - Discount;
//    }
//}

//// Domain/Entities/Customers/CartItem.cs
//namespace YourProject.Domain.Aggregates;

///// <summary>
///// Individual item in shopping cart
///// </summary>
//public class CartItem : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CartId { get; set; }
//    public virtual ShoppingCart Cart { get; set; } = null!;

//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public Guid? VariantId { get; set; }
//    public virtual ProductVariant? Variant { get; set; }

//    public int Quantity { get; set; }
//    public decimal UnitPrice { get; set; }
//    public decimal TotalPrice => Quantity * UnitPrice;
//    public decimal Discount { get; set; }

//    public string? CustomAttributes { get; set; } // JSON for custom options

//    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
//}
