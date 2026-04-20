using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Customers;

///// <summary>
///// Shopping Cart Aggregate Root
///// Manages cart lifecycle, items, pricing calculations, and cart-to-order conversion
///// </summary>
//public class ShoppingCart : Entity
//{
//    #region Identity
//    public Guid Id { get; set; }
//    #endregion

//    #region Ownership
//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    // For guest carts
//    public string? SessionId { get; set; }
//    public Guid? GuestId { get; set; }
//    #endregion

//    #region Status
//    public CartStatus Status { get; set; } = CartStatus.Active;
//    public bool IsGuestCart => CustomerId == Guid.Empty;
//    #endregion

//    #region Pricing
//    public decimal Subtotal { get; private set; }
//    public decimal Discount { get; private set; }
//    public decimal SubtotalAfterDiscount => Subtotal - Discount;
//    public decimal TaxAmount { get; private set; }
//    public decimal ShippingCost { get; private set; }
//    public decimal Total => SubtotalAfterDiscount + TaxAmount + ShippingCost;
//    public string Currency { get; set; } = "USD";
//    #endregion

//    #region Applied Discounts
//    public Guid? AppliedCouponId { get; set; }
//    public string? AppliedCouponCode { get; set; }
//    public decimal? CouponDiscount { get; set; }
//    public int AppliedRewardPoints { get; set; }
//    public decimal RewardPointsDiscount { get; set; }
//    #endregion

//    #region Timestamps
//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? ExpiresAt { get; set; }
//    public DateTime? LastActivityAt { get; set; } = DateTime.UtcNow;
//    #endregion

//    #region Shipping
//    public Guid? ShippingAddressId { get; set; }
//    public virtual Address? ShippingAddress { get; set; }
//    public ShippingMethod SelectedShippingMethod { get; set; }
//    #endregion

//    #region Navigation
//    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
//    #endregion

//    #region Computed
//    public int ItemCount => Items.Sum(i => i.Quantity);
//    public int UniqueItemCount => Items.Count;
//    public bool IsEmpty => !Items.Any();
//    public decimal TotalWeight => Items.Sum(i => (i.Product?.Weight ?? 0) * i.Quantity);
//    public bool HasDigitalItems => Items.Any(i => i.Product?.ProductType == ProductType.Digital);
//    public bool HasPhysicalItems => Items.Any(i => i.Product?.ProductType == ProductType.Physical);
//    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
//    #endregion

//    #region Domain Methods - Item Management

//    /// <summary>
//    /// Adds a product to the cart
//    /// </summary>
//    public CartItem AddItem(Guid productId, Guid? variantId, int quantity, decimal? priceOverride = null)
//    {
//        // Validate expiration
//        if (IsExpired)
//            throw new InvalidOperationException("Cart has expired");

//        // Check if item already exists
//        var existingItem = Items.FirstOrDefault(i =>
//            i.ProductId == productId &&
//            i.VariantId == variantId &&
//            i.CustomAttributes == null);

//        if (existingItem != null)
//        {
//            existingItem.Quantity += quantity;
//            existingItem.UnitPrice = priceOverride ?? existingItem.UnitPrice;
//            ValidateQuantity(existingItem);
//            RecalculateTotals();
//            return existingItem;
//        }

//        var item = new CartItem
//        {
//            Id = Guid.NewGuid(),
//            CartId = Id,
//            ProductId = productId,
//            VariantId = variantId,
//            Quantity = quantity,
//            UnitPrice = priceOverride ?? 0, // Should be set by domain service
//            AddedAt = DateTime.UtcNow
//        };

//        Items.Add(item);
//        RecalculateTotals();
//        return item;
//    }

//    /// <summary>
//    /// Adds item with custom attributes (e.g., personalized engravings)
//    /// </summary>
//    public CartItem AddItemWithAttributes(Guid productId, Guid? variantId, int quantity,
//        string customAttributes, decimal? priceOverride = null)
//    {
//        var existingItem = Items.FirstOrDefault(i =>
//            i.ProductId == productId &&
//            i.VariantId == variantId &&
//            i.CustomAttributes == customAttributes);

//        if (existingItem != null)
//        {
//            existingItem.Quantity += quantity;
//            RecalculateTotals();
//            return existingItem;
//        }

//        var item = new CartItem
//        {
//            Id = Guid.NewGuid(),
//            CartId = Id,
//            ProductId = productId,
//            VariantId = variantId,
//            Quantity = quantity,
//            CustomAttributes = customAttributes,
//            UnitPrice = priceOverride ?? 0,
//            AddedAt = DateTime.UtcNow
//        };

//        Items.Add(item);
//        RecalculateTotals();
//        return item;
//    }

//    public void RemoveItem(Guid itemId)
//    {
//        var item = Items.FirstOrDefault(i => i.Id == itemId);
//        if (item != null)
//        {
//            Items.Remove(item);
//            RecalculateTotals();
//        }
//    }

//    public void UpdateQuantity(Guid itemId, int quantity)
//    {
//        var item = Items.FirstOrDefault(i => i.Id == itemId);
//        if (item == null)
//            throw new InvalidOperationException("Item not found in cart");

//        if (quantity <= 0)
//        {
//            Items.Remove(item);
//        }
//        else
//        {
//            item.Quantity = quantity;
//            ValidateQuantity(item);
//        }

//        RecalculateTotals();
//    }

//    public void ApplyBundle(Guid bundleId, List<(Guid ProductId, Guid? VariantId, int Quantity)> items)
//    {
//        foreach (var (productId, variantId, quantity) in items)
//        {
//            AddItem(productId, variantId, quantity);
//        }

//        // Mark items as part of bundle
//        foreach (var item in Items.Where(i => items.Any(b => b.ProductId == i.ProductId)))
//        {
//            item.BundleId = bundleId;
//        }

//        RecalculateTotals();
//    }

//    private void ValidateQuantity(CartItem item)
//    {
//        // Check against product limits
//        if (item.Product?.MaximumOrderQuantity.HasValue == true &&
//            item.Quantity > item.Product.MaximumOrderQuantity.Value)
//        {
//            item.Quantity = (int)item.Product.MaximumOrderQuantity.Value;
//        }
//    }

//    #endregion

//    #region Domain Methods - Pricing & Discounts

//    public void RecalculateTotals()
//    {
//        // Calculate subtotal from items
//        Subtotal = Items.Sum(i => i.UnitPrice * i.Quantity);

//        // Apply item-level discounts
//        Discount = Items.Sum(i => i.Discount);

//        // Recalculate each item's totals
//        foreach (var item in Items)
//        {
//            item.TotalPrice = item.UnitPrice * item.Quantity;
//            item.Discount = item.Discount; // Keep as-is for now
//        }

//        UpdatedAt = DateTime.UtcNow;
//        LastActivityAt = DateTime.UtcNow;

//        AddDomainEvent(new CartUpdatedEvent(Id, Items.Count, Total));
//    }

//    public void ApplyCoupon(Coupon coupon)
//    {
//        if (!coupon.IsValid())
//            throw new InvalidOperationException("Coupon is not valid");

//        // Check minimum order amount
//        if (coupon.MinimumOrderAmount.HasValue && Subtotal < coupon.MinimumOrderAmount.Value)
//            throw new InvalidOperationException($"Minimum order amount of {coupon.MinimumOrderAmount} required");

//        // Check product restrictions
//        if (coupon.ProductIds.Any())
//        {
//            var cartProductIds = Items.Select(i => i.ProductId).ToList();
//            if (!coupon.ProductIds.Intersect(cartProductIds).Any())
//                throw new InvalidOperationException("No eligible products in cart for this coupon");
//        }

//        AppliedCouponId = coupon.Id;
//        AppliedCouponCode = coupon.Code;

//        // Calculate coupon discount
//        CouponDiscount = coupon.CalculateDiscount(Subtotal);
//        Discount += CouponDiscount ?? 0;

//        RecalculateTotals();
//    }

//    public void RemoveCoupon()
//    {
//        if (CouponDiscount.HasValue)
//            Discount -= CouponDiscount.Value;

//        AppliedCouponId = null;
//        AppliedCouponCode = null;
//        CouponDiscount = null;

//        RecalculateTotals();
//    }

//    public void ApplyRewardPoints(int points, decimal pointsValue)
//    {
//        // Validate customer has enough points
//        AppliedRewardPoints = Math.Min(points, Customer?.RewardPoints ?? 0);
//        RewardPointsDiscount = pointsValue;
//        Discount += RewardPointsDiscount;
//        RecalculateTotals();
//    }

//    public void RemoveRewardPoints()
//    {
//        Discount -= RewardPointsDiscount;
//        AppliedRewardPoints = 0;
//        RewardPointsDiscount = 0;
//        RecalculateTotals();
//    }

//    public void SetShippingAddress(Address address)
//    {
//        ShippingAddressId = address.Id;
//        ShippingAddress = address;
//        AddDomainEvent(new CartShippingAddressChangedEvent(Id, address.Id));
//    }

//    public void SetShippingMethod(ShippingMethod method, decimal cost)
//    {
//        SelectedShippingMethod = method;
//        ShippingCost = cost;
//        RecalculateTotals();
//    }

//    public void CalculateTax(decimal taxRate)
//    {
//        TaxAmount = SubtotalAfterDiscount * taxRate;
//        RecalculateTotals();
//    }

//    #endregion

//    #region Domain Methods - Lifecycle

//    public void Abandon()
//    {
//        Status = CartStatus.Abandoned;
//        AddDomainEvent(new CartAbandonedEvent(Id, Items.Count, Total));
//    }

//    public void ConvertToOrder(Guid orderId)
//    {
//        Status = CartStatus.Converted;
//        AddDomainEvent(new CartConvertedToOrderEvent(Id, orderId));
//    }

//    public void Expire()
//    {
//        if (Status == CartStatus.Active)
//        {
//            Status = CartStatus.Expired;
//            AddDomainEvent(new CartExpiredEvent(Id));
//        }
//    }

//    public void Merge(ShoppingCart otherCart)
//    {
//        // Merge items from other cart into this cart
//        foreach (var otherItem in otherCart.Items)
//        {
//            var existingItem = Items.FirstOrDefault(i =>
//                i.ProductId == otherItem.ProductId &&
//                i.VariantId == otherItem.VariantId);

//            if (existingItem != null)
//            {
//                existingItem.Quantity += otherItem.Quantity;
//            }
//            else
//            {
//                Items.Add(new CartItem
//                {
//                    Id = Guid.NewGuid(),
//                    CartId = Id,
//                    ProductId = otherItem.ProductId,
//                    VariantId = otherItem.VariantId,
//                    Quantity = otherItem.Quantity,
//                    UnitPrice = otherItem.UnitPrice,
//                    AddedAt = DateTime.UtcNow
//                });
//            }
//        }

//        RecalculateTotals();
//    }

//    public void Clear()
//    {
//        Items.Clear();
//        Discount = 0;
//        TaxAmount = 0;
//        ShippingCost = 0;
//        AppliedCouponId = null;
//        AppliedCouponCode = null;
//        CouponDiscount = null;
//        AppliedRewardPoints = 0;
//        RewardPointsDiscount = 0;
//        RecalculateTotals();
//    }

//    #endregion

//    #region Validation

//    public List<string> ValidateForCheckout()
//    {
//        var errors = new List<string>();

//        if (IsEmpty)
//            errors.Add("Cart is empty");

//        if (!Items.All(i => i.IsValid()))
//            errors.Add("Some items in cart are invalid");

//        if (Items.Any(i => !i.IsAvailable))
//            errors.Add("Some items are no longer available");

//        if (Items.Any(i => i.Quantity > i.AvailableStock))
//            errors.Add("Some items have exceeded available stock");

//        if (ShippingAddress == null && HasPhysicalItems)
//            errors.Add("Shipping address is required");

//        if (IsExpired)
//            errors.Add("Cart has expired");

//        return errors;
//    }

//    public bool CanCheckout() => !ValidateForCheckout().Any();

//    #endregion
//}

///// <summary>
///// Individual item in shopping cart
///// Part of Cart aggregate
///// </summary>
//public class CartItem : Entity
//{
//    public Guid Id { get; set; }
//    public Guid CartId { get; set; }
//    public virtual ShoppingCart Cart { get; set; } = null!;

//    #region Product Reference
//    public Guid ProductId { get; set; }
//    public virtual Product Product { get; set; } = null!;

//    public Guid? VariantId { get; set; }
//    public virtual ProductVariant? Variant { get; set; }
//    #endregion

//    #region Quantity
//    public int Quantity { get; set; }
//    #endregion

//    #region Pricing (at time of adding to cart)
//    public decimal UnitPrice { get; set; }
//    public decimal TotalPrice => Quantity * UnitPrice;
//    public decimal Discount { get; set; }
//    public decimal FinalPrice => TotalPrice - Discount;
//    #endregion

//    #region Customization
//    public string? CustomAttributes { get; set; } // JSON for custom options (e.g., engraving text)
//    #endregion

//    #region Bundle
//    public Guid? BundleId { get; set; }
//    #endregion

//    #region Timestamps
//    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? PriceChangedAt { get; set; }
//    #endregion

//    #region Stock Validation
//    public bool IsAvailable => Product?.IsInStock == true;
//    public int AvailableStock => Product?.AvailableQuantity ?? 0;
//    public bool IsPriceChanged => Product != null && Product.CurrentPrice != UnitPrice;
//    public decimal PriceDifference => Product != null ? Product.CurrentPrice - UnitPrice : 0;
//    #endregion

//    #region Domain Methods

//    public bool IsValid()
//    {
//        return Product != null &&
//               Quantity > 0 &&
//               UnitPrice >= 0 &&
//               (Product.MaximumOrderQuantity == null || Quantity <= Product.MaximumOrderQuantity);
//    }

//    public void UpdateQuantity(int newQuantity)
//    {
//        if (newQuantity <= 0)
//        {
//            Cart.RemoveItem(Id);
//        }
//        else
//        {
//            Quantity = newQuantity;

//            // Check for price changes
//            if (IsPriceChanged)
//            {
//                PriceChangedAt = DateTime.UtcNow;
//            }
//        }
//    }

//    public void ApplyDiscount(decimal discountAmount)
//    {
//        if (discountAmount > TotalPrice)
//            discountAmount = TotalPrice;
//        Discount = discountAmount;
//    }

//    #endregion
//}

//// ============================================================================
//// ENUMS - Cart Aggregate
//// ============================================================================

//public enum CartStatus
//{
//    Active = 0,
//    Converted = 1,
//    Abandoned = 2,
//    Expired = 3,
//    Merged = 4
//}

//public enum ShippingMethod
//{
//    Standard = 0,
//    Express = 1,
//    NextDay = 2,
//    International = 3,
//    StorePickup = 4,
//    DigitalDelivery = 5,
//    Freight = 6
//}

//// ============================================================================
//// DOMAIN EVENTS - Cart Aggregate
//// ============================================================================


//public record CartUpdatedEvent(Guid CartId, int ItemCount, decimal Total) : IDomainEvent;
//public record CartItemAddedEvent(Guid CartId, Guid ProductId, int Quantity) : IDomainEvent;
//public record CartItemRemovedEvent(Guid CartId, Guid ProductId) : IDomainEvent;
//public record CartShippingAddressChangedEvent(Guid CartId, Guid AddressId) : IDomainEvent;
//public record CartCouponAppliedEvent(Guid CartId, string CouponCode, decimal Discount) : IDomainEvent;
//public record CartAbandonedEvent(Guid CartId, int ItemCount, decimal Total) : IDomainEvent;
//public record CartConvertedToOrderEvent(Guid CartId, Guid OrderId) : IDomainEvent;
//public record CartExpiredEvent(Guid CartId) : IDomainEvent;
