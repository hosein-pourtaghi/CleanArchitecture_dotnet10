using Domain.Aggregates.Carts;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Payments;
using Domain.Aggregates.Products;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Customers;


///// <summary>
///// Customer Aggregate Root - Manages all customer-related entities
///// Includes: Customer, Address, Wishlist, WishlistItem
///// </summary>
//public class Customer : Entity
//{
//    #region Identity
//    public Guid Id { get; set; }
//    #endregion

//    #region Basic Info
//    public string Email { get; set; } = string.Empty;
//    public string PhoneNumber { get; set; } = string.Empty;
//    public string FirstName { get; set; } = string.Empty;
//    public string LastName { get; set; } = string.Empty;
//    public string FullName => $"{FirstName} {LastName}".Trim();
//    public DateTime? DateOfBirth { get; set; }
//    public Gender? Gender { get; set; }
//    public CustomerType Type { get; set; } = CustomerType.Individual;
//    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
//    #endregion

//    #region Authentication
//    public string PasswordHash { get; set; } = string.Empty;
//    public bool EmailVerified { get; set; }
//    public bool PhoneVerified { get; set; }
//    public DateTime? LastLoginAt { get; set; }
//    public string? LastLoginIp { get; set; }
//    public string? RefreshToken { get; set; }
//    public DateTime? RefreshTokenExpiry { get; set; }
//    #endregion

//    #region Business Preferences
//    public string PreferredLanguage { get; set; } = "en";
//    public string PreferredCurrency { get; set; } = "USD";
//    public string? TimeZone { get; set; }
//    #endregion

//    #region Marketing Preferences
//    public bool SubscribeToNewsletter { get; set; }
//    public bool SubscribeToSms { get; set; }
//    public bool SubscribeToPush { get; set; }
//    public DateTime? LastEmailSentAt { get; set; }
//    #endregion

//    #region Loyalty & Stats
//    public int TotalOrders { get; set; }
//    public decimal TotalSpent { get; set; }
//    public int TotalProductsReviewed { get; set; }
//    public int RewardPoints { get; set; }
//    public decimal LifetimeValue { get; set; }
//    public DateTime? LastPurchaseAt { get; set; }
//    public string CustomerTier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum
//    #endregion

//    #region Timestamps
//    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public new DateTime? UpdatedAt { get; set; }
//    #endregion

//    #region Navigation - Address
//    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

//    // Helper methods for Address
//    public Address AddAddress(Address address)
//    {
//        address.CustomerId = Id;

//        // If this is the first address or marked as default, set as default
//        if (!Addresses.Any() || address.IsDefault)
//        {
//            foreach (var existing in Addresses)
//                existing.IsDefault = false;
//            address.IsDefault = true;
//        }

//        Addresses.Add(address);
//        return address;
//    }

//    public void RemoveAddress(Guid addressId)
//    {
//        var address = Addresses.FirstOrDefault(a => a.Id == addressId);
//        if (address != null)
//        {
//            var wasDefault = address.IsDefault;
//            Addresses.Remove(address);

//            // If removed address was default, make first remaining address default
//            if (wasDefault && Addresses.Any())
//                Addresses.First().IsDefault = true;
//        }
//    }

//    public Address? GetDefaultShippingAddress() =>
//        Addresses.FirstOrDefault(a => a.IsDefault && (a.Type == AddressType.Shipping || a.Type == AddressType.Both));

//    public Address? GetDefaultBillingAddress() =>
//        Addresses.FirstOrDefault(a => a.IsDefault && (a.Type == AddressType.Billing || a.Type == AddressType.Both));
//    #endregion

//    #region Navigation - Wishlist
//    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

//    public Wishlist AddWishlist(string name, bool isPublic = false)
//    {
//        var wishlist = new Wishlist
//        {
//            Id = Guid.NewGuid(),
//            CustomerId = Id,
//            Name = name,
//            IsPublic = isPublic,
//            CreatedAt = DateTime.UtcNow
//        };
//        Wishlists.Add(wishlist);
//        return wishlist;
//    }

//    public Wishlist GetOrCreateDefaultWishlist()
//    {
//        var defaultWishlist = Wishlists.FirstOrDefault(w => w.Name == "My Wishlist");
//        if (defaultWishlist == null)
//            defaultWishlist = AddWishlist("My Wishlist");
//        return defaultWishlist;
//    }
//    #endregion

//    #region Navigation - References
//    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
//    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
//    public virtual ICollection<ShoppingCart> Carts { get; set; } = new List<ShoppingCart>();
//    #endregion

//    #region Navigation - References
//    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
//    #endregion


//    #region Domain Methods
//    public void UpdateProfile(string firstName, string lastName, string phoneNumber)
//    {
//        FirstName = firstName;
//        LastName = lastName;
//        PhoneNumber = phoneNumber;
//        SetUpdatedBy(null);
//    }

//    public void RecordLogin(string ipAddress)
//    {
//        LastLoginAt = DateTime.UtcNow;
//        LastLoginIp = ipAddress;
//    }

//    public void AddRewardPoints(int points, string reason)
//    {
//        RewardPoints += points;
//        AddDomainEvent(new CustomerRewardPointsChangedEvent(Id, points, reason));
//    }

//    public void UpdateStatsAfterOrder(decimal orderTotal)
//    {
//        TotalOrders++;
//        TotalSpent += orderTotal;
//        LastPurchaseAt = DateTime.UtcNow;

//        // Update tier based on lifetime value
//        UpdateCustomerTier();
//    }

//    private void UpdateCustomerTier()
//    {
//        CustomerTier = LifetimeValue switch
//        {
//            >= 10000 => "Platinum",
//            >= 5000 => "Gold",
//            >= 1000 => "Silver",
//            _ => "Bronze"
//        };
//    }

//    public void Suspend(string reason)
//    {
//        Status = CustomerStatus.Suspended;
//        AddDomainEvent(new CustomerSuspendedEvent(Id, reason));
//    }

//    public void Activate()
//    {
//        Status = CustomerStatus.Active;
//    }

//    public void VerifyEmail()
//    {
//        EmailVerified = true;
//        AddDomainEvent(new CustomerEmailVerifiedEvent(Id));
//    }

//    public void VerifyPhone()
//    {
//        PhoneVerified = true;
//    }

//    public void UpdatePreferences(string language, string currency, string? timeZone)
//    {
//        PreferredLanguage = language;
//        PreferredCurrency = currency;
//        TimeZone = timeZone;
//    }
//    #endregion
//}

///// <summary>
///// Represents a customer address (shipping or billing)
///// Part of Customer aggregate - Value Object with identity for address management
///// </summary>
//public class Address : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    #region Address Details
//    public AddressType Type { get; set; } = AddressType.Shipping;
//    public string FirstName { get; set; } = string.Empty;
//    public string LastName { get; set; } = string.Empty;
//    public string FullName => $"{FirstName} {LastName}".Trim();
//    public string Company { get; set; } = string.Empty;
//    public string PhoneNumber { get; set; } = string.Empty;

//    public string AddressLine1 { get; set; } = string.Empty;
//    public string AddressLine2 { get; set; } = string.Empty;
//    public string AddressLine3 { get; set; } = string.Empty;

//    public string City { get; set; } = string.Empty;
//    public string State { get; set; } = string.Empty;
//    public string PostalCode { get; set; } = string.Empty;
//    public string Country { get; set; } = string.Empty;
//    public string CountryCode { get; set; } = string.Empty; // ISO 2-letter code (e.g., "US", "UK")
//    #endregion

//    #region Geolocation
//    public double? Latitude { get; set; }
//    public double? Longitude { get; set; }
//    public string? TimeZoneId { get; set; }
//    #endregion

//    #region Delivery
//    public string? DeliveryInstructions { get; set; }
//    public string? Landmark { get; set; }
//    public string? AccessCode { get; set; }
//    #endregion

//    #region Metadata
//    public bool IsDefault { get; set; }
//    public bool IsVerified { get; set; }
//    public DateTime? VerifiedAt { get; set; }
//    #endregion

//    #region Computed
//    public string FullAddress => string.Join(", ",
//        new[] { AddressLine1, AddressLine2, AddressLine3, City, State, PostalCode, Country }
//            .Where(s => !string.IsNullOrWhiteSpace(s)));

//    public string SingleLineAddress => $"{AddressLine1}, {City}, {State} {PostalCode}, {Country}";
//    #endregion

//    #region Domain Methods
//    public void SetAsDefault()
//    {
//        if (Customer != null)
//        {
//            foreach (var address in Customer.Addresses)
//                address.IsDefault = false;
//        }
//        IsDefault = true;
//    }

//    public void UpdateAddress(string line1, string line2, string city, string state,
//        string postalCode, string country, string countryCode)
//    {
//        AddressLine1 = line1;
//        AddressLine2 = line2;
//        City = city;
//        State = state;
//        PostalCode = postalCode;
//        Country = country;
//        CountryCode = countryCode;
//    }

//    public bool IsValidForShipping() =>
//        !string.IsNullOrWhiteSpace(FullName) &&
//        !string.IsNullOrWhiteSpace(PhoneNumber) &&
//        !string.IsNullOrWhiteSpace(AddressLine1) &&
//        !string.IsNullOrWhiteSpace(City) &&
//        !string.IsNullOrWhiteSpace(State) &&
//        !string.IsNullOrWhiteSpace(PostalCode) &&
//        !string.IsNullOrWhiteSpace(Country);
//    #endregion
//}

///// <summary>
///// Customer wishlist - allows customers to save products for later
///// Part of Customer aggregate
///// </summary>
//public class Wishlist : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    public string Name { get; set; } = "My Wishlist";
//    public string? Description { get; set; }
//    public bool IsPublic { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? SharedAt { get; set; }
//    public string? ShareLink { get; set; }

//    #region Navigation
//    public virtual ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
//    #endregion

//    #region Computed
//    public int ItemCount => Items.Count;
//    public int ActiveItemCount => Items.Count(i => i.PurchasedAt == null);
//    #endregion

//    #region Domain Methods
//    public WishlistItem AddItem(Guid productId, Guid? variantId = null, int quantity = 1, string? note = null)
//    {
//        // Check if item already exists
//        var existingItem = Items.FirstOrDefault(i =>
//            i.ProductId == productId &&
//            i.VariantId == variantId &&
//            i.PurchasedAt == null);

//        if (existingItem != null)
//        {
//            existingItem.Quantity += quantity;
//            return existingItem;
//        }

//        var item = new WishlistItem
//        {
//            Id = Guid.NewGuid(),
//            WishlistId = Id,
//            ProductId = productId,
//            VariantId = variantId,
//            Quantity = quantity,
//            Note = note,
//            AddedAt = DateTime.UtcNow
//        };

//        Items.Add(item);
//        return item;
//    }

//    public void RemoveItem(Guid itemId)
//    {
//        var item = Items.FirstOrDefault(i => i.Id == itemId);
//        if (item != null)
//            Items.Remove(item);
//    }

//    public void UpdateQuantity(Guid itemId, int quantity)
//    {
//        var item = Items.FirstOrDefault(i => i.Id == itemId);
//        if (item != null)
//        {
//            if (quantity <= 0)
//                Items.Remove(item);
//            else
//                item.Quantity = quantity;
//        }
//    }

//    public void MarkAsPurchased(Guid itemId)
//    {
//        var item = Items.FirstOrDefault(i => i.Id == itemId);
//        if (item != null)
//            item.PurchasedAt = DateTime.UtcNow;
//    }

//    public void Share()
//    {
//        if (string.IsNullOrEmpty(ShareLink))
//        {
//            ShareLink = Guid.NewGuid().ToString("N");
//            SharedAt = DateTime.UtcNow;
//        }
//        IsPublic = true;
//    }

//    public void Unshare()
//    {
//        IsPublic = false;
//        ShareLink = null;
//        SharedAt = null;
//    }
//}

///// <summary>
///// Individual item in a wishlist
///// Part of Customer aggregate (belongs to Wishlist)
///// </summary>
//public class WishlistItem : Entity
//{
//    public Guid Id { get; set; }
//    public Guid WishlistId { get; set; }
//    public virtual Wishlist Wishlist { get; set; } = null!;

//    public Guid ProductId { get; set; }
//    public virtual Product Product { get; set; } = null!;

//    public Guid? VariantId { get; set; }
//    public virtual ProductVariant? Variant { get; set; }

//    public int Quantity { get; set; } = 1;
//    public string? Note { get; set; } // e.g., "For birthday"
//    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

//    public DateTime? PurchasedAt { get; set; }
//    public DateTime? NotifiedAt { get; set; } // Last notification sent
//    public decimal? PriceWhenAdded { get; set; } // Track price at time of adding

//    #region Computed
//    public bool IsPurchased => PurchasedAt.HasValue;
//    public bool IsPriceChanged => PriceWhenAdded.HasValue && Product != null && Product.CurrentPrice != PriceWhenAdded;
//    public decimal PriceDifference => PriceWhenAdded.HasValue && Product != null
//        ? Product.CurrentPrice - PriceWhenAdded.Value
//        : 0;
//    #endregion
//}

//// ============================================================================
//// ENUMS - Customer Aggregate
//// ============================================================================

//public enum CustomerType
//{
//    Individual = 0,
//    Business = 1
//}

//public enum CustomerStatus
//{
//    Active = 0,
//    Inactive = 1,
//    Suspended = 2,
//    Banned = 3,
//    PendingVerification = 4
//}

//public enum Gender
//{
//    Male = 0,
//    Female = 1,
//    Other = 2,
//    PreferNotToSay = 3
//}

//public enum AddressType
//{
//    Shipping = 0,
//    Billing = 1,
//    Both = 2,
//    StorePickup = 3
//}

//// ============================================================================
//// DOMAIN EVENTS - Customer Aggregate
//// ============================================================================

//public record CustomerRewardPointsChangedEvent(Guid CustomerId, int Points, string Reason) : IDomainEvent;
//public record CustomerSuspendedEvent(Guid CustomerId, string Reason) : IDomainEvent;
//public record CustomerEmailVerifiedEvent(Guid CustomerId) : IDomainEvent;
//#endregion
