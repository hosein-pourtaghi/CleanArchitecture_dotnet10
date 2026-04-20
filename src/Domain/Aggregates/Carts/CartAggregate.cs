using Domain.Aggregates.Customers;
using Domain.Aggregates.Products;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Carts;



/// <summary>
/// ═══════════════════════════════════════════════════════════════════════════
/// SHOPPING CART AGGREGATE
/// ═══════════════════════════════════════════════════════════════════════════
/// 
/// Aggregate Root: ShoppingCart
/// 
/// Purpose:
/// - Manages the complete shopping cart lifecycle
/// - Handles cart items, pricing, discounts, and coupons
/// - Provides cart-to-order conversion
/// - Supports both authenticated and guest users
/// 
/// Boundaries:
/// - Cart is independent of other aggregates
/// - References Product (read-only, no ownership)
/// - References Customer (for authenticated carts)
/// - Converts to Order aggregate upon checkout
/// 
/// ═══════════════════════════════════════════════════════════════════════════
/// </summary>
public class ShoppingCart : Entity
{
    #region ═══════════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════════════════

    public Guid Id { get; set; }

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // OWNERSHIP - Who owns this cart?
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Customer ID (null for guest carts)
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Navigation to customer (optional for guest carts)
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Session ID for guest carts (stored in cookie)
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Guest identifier for anonymous users
    /// </summary>
    public Guid? GuestId { get; set; }

    /// <summary>
    /// Is this a guest cart?
    /// </summary>
    public bool IsGuestCart => CustomerId == Guid.Empty;

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // STATUS
    // ═══════════════════════════════════════════════════════════════════════════

    public CartStatus Status { get; set; } = CartStatus.Active;

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // PRICING - All monetary values
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sum of all item prices before any discounts
    /// </summary>
    public decimal Subtotal { get; private set; }

    /// <summary>
    /// Total discount applied (coupons + rewards + item discounts)
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Subtotal after discount
    /// </summary>
    public decimal SubtotalAfterDiscount => Subtotal - Discount;

    /// <summary>
    /// Tax amount (calculated based on shipping address)
    /// </summary>
    public decimal TaxAmount { get; private set; }

    /// <summary>
    /// Shipping cost
    /// </summary>
    public decimal ShippingCost { get; private set; }

    /// <summary>
    /// Final total (SubtotalAfterDiscount + Tax + Shipping)
    /// </summary>
    public decimal Total => SubtotalAfterDiscount + TaxAmount + ShippingCost;

    /// <summary>
    /// Currency code (ISO 4217)
    /// </summary>
    public string Currency { get; set; } = "USD";

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // APPLIED DISCOUNTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applied coupon ID
    /// </summary>
    public Guid? AppliedCouponId { get; set; }

    /// <summary>
    /// Applied coupon code
    /// </summary>
    public string? AppliedCouponCode { get; set; }

    /// <summary>
    /// Discount amount from coupon
    /// </summary>
    public decimal? CouponDiscount { get; private set; }

    /// <summary>
    /// Reward points applied to this cart
    /// </summary>
    public int AppliedRewardPoints { get; private set; }

    /// <summary>
    /// Monetary value of applied reward points
    /// </summary>
    public decimal RewardPointsDiscount { get; private set; }

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // SHIPPING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Selected shipping address ID
    /// </summary>
    public Guid? ShippingAddressId { get; set; }

    /// <summary>
    /// Shipping address (snapshot at time of selection)
    /// </summary>
    public virtual Address? ShippingAddress { get; set; }

    /// <summary>
    /// Selected shipping method
    /// </summary>
    public ShippingMethod SelectedShippingMethod { get; set; }

    /// <summary>
    /// Estimated delivery date
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // TIMESTAMPS
    // ═══════════════════════════════════════════════════════════════════════════

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; } = DateTime.UtcNow;

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // NAVIGATION - Child Entities
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Items in this cart
    /// </summary>
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Total number of items (sum of quantities)
    /// </summary>
    public int TotalItemCount => Items.Sum(i => i.Quantity);

    /// <summary>
    /// Number of unique products in cart
    /// </summary>
    public int UniqueItemCount => Items.Count;

    /// <summary>
    /// Is the cart empty?
    /// </summary>
    public bool IsEmpty => !Items.Any();

    /// <summary>
    /// Total weight of all items (for shipping calculation)
    /// </summary>
    public decimal TotalWeight => Items.Sum(i => (i.Product?.Weight ?? 0) * i.Quantity);

    /// <summary>
    /// Does cart contain digital products?
    /// </summary>
    public bool HasDigitalItems => Items.Any(i => i.Product?.ProductType == ProductType.Digital);

    /// <summary>
    /// Does cart contain physical products?
    /// </summary>
    public bool HasPhysicalItems => Items.Any(i => i.Product?.ProductType == ProductType.Physical);

    /// <summary>
    /// Has the cart expired?
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Can this cart proceed to checkout?
    /// </summary>
    public bool CanCheckout => !IsEmpty && !IsExpired && Status == CartStatus.Active;

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // DOMAIN METHODS - Item Management
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a product to the cart
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="variantId">Optional variant ID</param>
    /// <param name="quantity">Quantity to add</param>
    /// <param name="priceOverride">Optional price override</param>
    /// <returns>The added or updated cart item</returns>
    public CartItem AddItem(Guid productId, Guid? variantId = null, int quantity = 1, decimal? priceOverride = null)
    {
        // Validate cart is active
        if (Status != CartStatus.Active)
            throw new InvalidOperationException("Cannot add items to an inactive cart");

        // Validate cart hasn't expired
        if (IsExpired)
            throw new InvalidOperationException("Cart has expired");

        // Validate quantity
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        // Check if item already exists in cart
        var existingItem = Items.FirstOrDefault(i =>
            i.ProductId == productId &&
            i.VariantId == variantId &&
            string.IsNullOrEmpty(i.CustomAttributes));

        if (existingItem != null)
        {
            // Update existing item
            existingItem.Quantity += quantity;
            existingItem.UnitPrice = priceOverride ?? existingItem.UnitPrice;

            // Validate against product limits
            ValidateQuantity(existingItem);

            RecalculateTotals();

            AddDomainEvent(new CartItemUpdatedEvent(Id, existingItem.ProductId, existingItem.Quantity));

            return existingItem;
        }

        // Create new cart item
        var newItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = Id,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
            UnitPrice = priceOverride ?? 0, // Should be set by domain service from product
            AddedAt = DateTime.UtcNow
        };

        Items.Add(newItem);
        RecalculateTotals();

        AddDomainEvent(new CartItemAddedEvent(Id, productId, variantId, quantity));

        return newItem;
    }

    /// <summary>
    /// Adds an item with custom attributes (e.g., personalized engravings)
    /// </summary>
    public CartItem AddItemWithAttributes(
        Guid productId,
        Guid? variantId,
        int quantity,
        string customAttributes,
        decimal? priceOverride = null)
    {
        if (string.IsNullOrWhiteSpace(customAttributes))
            return AddItem(productId, variantId, quantity, priceOverride);

        // Check if same item with same attributes exists
        var existingItem = Items.FirstOrDefault(i =>
            i.ProductId == productId &&
            i.VariantId == variantId &&
            i.CustomAttributes == customAttributes);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            RecalculateTotals();
            return existingItem;
        }

        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = Id,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
            CustomAttributes = customAttributes,
            UnitPrice = priceOverride ?? 0,
            AddedAt = DateTime.UtcNow
        };

        Items.Add(item);
        RecalculateTotals();

        AddDomainEvent(new CartItemAddedEvent(Id, productId, variantId, quantity));

        return item;
    }

    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var productId = item.ProductId;
        Items.Remove(item);
        RecalculateTotals();

        AddDomainEvent(new CartItemRemovedEvent(Id, productId, item.VariantId));
    }

    /// <summary>
    /// Updates the quantity of an item
    /// </summary>
    public void UpdateQuantity(Guid itemId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found in cart");

        if (quantity <= 0)
        {
            // Remove item if quantity is zero or negative
            RemoveItem(itemId);
            return;
        }

        item.Quantity = quantity;
        ValidateQuantity(item);
        RecalculateTotals();

        AddDomainEvent(new CartItemUpdatedEvent(Id, item.ProductId, quantity));
    }

    /// <summary>
    /// Validates quantity against product limits
    /// </summary>
    private void ValidateQuantity(CartItem item)
    {
        if (item.Product == null)
            return;

        // Check minimum quantity
        if (item.Product.MinimumOrderQuantity.HasValue &&
            item.Quantity < item.Product.MinimumOrderQuantity.Value)
        {
            item.Quantity = item.Product.MinimumOrderQuantity.Value;
        }

        // Check maximum quantity
        if (item.Product.MaximumOrderQuantity.HasValue &&
            item.Quantity > item.Product.MaximumOrderQuantity.Value)
        {
            item.Quantity = item.Product.MaximumOrderQuantity.Value;
        }
    }

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // DOMAIN METHODS - Pricing & Discounts
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Recalculates all cart totals
    /// </summary>
    public void RecalculateTotals()
    {
        // Calculate subtotal from items
        Subtotal = Items.Sum(i => i.UnitPrice * i.Quantity);

        // Calculate item-level discounts
        Discount = Items.Sum(i => i.Discount);

        // Add coupon discount if applied
        if (CouponDiscount.HasValue)
        {
            Discount += CouponDiscount.Value;
        }

        // Add reward points discount
        if (RewardPointsDiscount > 0)
        {
            Discount += RewardPointsDiscount;
        }

        // Update timestamps
        UpdatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;

        AddDomainEvent(new CartUpdatedEvent(Id, Items.Count, Total));
    }

    /// <summary>
    /// Applies a coupon to the cart
    /// </summary>
    /// <param name="coupon">The coupon to apply</param>
    /// <exception cref="InvalidOperationException">Thrown when coupon is invalid</exception>
    public void ApplyCoupon(Coupon coupon)
    {
        // Validate coupon
        if (!coupon.IsValid())
            throw new InvalidOperationException("Coupon is not valid or has expired");

        // Check minimum order amount
        if (coupon.MinimumOrderAmount.HasValue && Subtotal < coupon.MinimumOrderAmount.Value)
            throw new InvalidOperationException(
                $"Minimum order amount of {coupon.MinimumOrderAmount:C} required for this coupon");

        // Check product restrictions
        if (coupon.ProductIds.Any())
        {
            var cartProductIds = Items.Select(i => i.ProductId).ToHashSet();
            if (!coupon.ProductIds.Intersect(cartProductIds).Any())
                throw new InvalidOperationException(
                    "No eligible products in cart for this coupon");
        }

        // Check category restrictions
        if (coupon.CategoryIds.Any())
        {
            var cartCategoryIds = Items
                .Where(i => i.Product?.CategoryId != null)
                .Select(i => i.Product!.CategoryId)
                .ToHashSet();
            if (!coupon.CategoryIds.Intersect(cartCategoryIds).Any())
                throw new InvalidOperationException(
                    "No products from eligible categories in cart");
        }

        // Remove existing coupon discount before applying new one
        if (CouponDiscount.HasValue)
        {
            Discount -= CouponDiscount.Value;
        }

        // Apply coupon
        AppliedCouponId = coupon.Id;
        AppliedCouponCode = coupon.Code;
        CouponDiscount = coupon.CalculateDiscount(Subtotal);
        Discount += CouponDiscount.Value;

        RecalculateTotals();

        AddDomainEvent(new CartCouponAppliedEvent(Id, coupon.Code, CouponDiscount.Value));
    }

    /// <summary>
    /// Removes the applied coupon from the cart
    /// </summary>
    public void RemoveCoupon()
    {
        if (!AppliedCouponId.HasValue)
            return;

        // Remove coupon discount
        if (CouponDiscount.HasValue)
        {
            Discount -= CouponDiscount.Value;
            CouponDiscount = null;
        }

        AppliedCouponId = null;
        AppliedCouponCode = null;

        RecalculateTotals();

        AddDomainEvent(new CartCouponRemovedEvent(Id));
    }

    /// <summary>
    /// Applies reward points to the cart
    /// </summary>
    /// <param name="points">Number of points to redeem</param>
    /// <param name="pointsValue">Monetary value of the points</param>
    public void ApplyRewardPoints(int points, decimal pointsValue)
    {
        // Validate customer has enough points
        if (Customer != null && points > Customer.RewardPoints)
            throw new InvalidOperationException("Not enough reward points");

        // Remove existing reward points discount
        if (RewardPointsDiscount > 0)
        {
            Discount -= RewardPointsDiscount;
        }

        AppliedRewardPoints = points;
        RewardPointsDiscount = pointsValue;
        Discount += RewardPointsDiscount;

        RecalculateTotals();

        AddDomainEvent(new CartRewardPointsAppliedEvent(Id, points, pointsValue));
    }

    /// <summary>
    /// Removes applied reward points from the cart
    /// </summary>
    public void RemoveRewardPoints()
    {
        if (AppliedRewardPoints <= 0)
            return;

        Discount -= RewardPointsDiscount;
        AppliedRewardPoints = 0;
        RewardPointsDiscount = 0;

        RecalculateTotals();
    }

    /// <summary>
    /// Sets the shipping address
    /// </summary>
    public void SetShippingAddress(Address address)
    {
        ShippingAddressId = address.Id;
        ShippingAddress = address;

        AddDomainEvent(new CartShippingAddressChangedEvent(Id, address.Id));
    }

    /// <summary>
    /// Sets the shipping method and cost
    /// </summary>
    public void SetShippingMethod(ShippingMethod method, decimal cost)
    {
        SelectedShippingMethod = method;
        ShippingCost = cost;

        RecalculateTotals();

        AddDomainEvent(new CartShippingMethodChangedEvent(Id, method, cost));
    }

    /// <summary>
    /// Calculates tax based on rate and shipping address
    /// </summary>
    public void CalculateTax(decimal taxRate)
    {
        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1");

        TaxAmount = SubtotalAfterDiscount * taxRate;
        RecalculateTotals();
    }

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // DOMAIN METHODS - Lifecycle
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Marks the cart as abandoned
    /// </summary>
    public void Abandon()
    {
        if (Status != CartStatus.Active)
            return;

        Status = CartStatus.Abandoned;

        AddDomainEvent(new CartAbandonedEvent(Id, Items.Count, Total));
    }

    /// <summary>
    /// Marks the cart as converted to an order
    /// </summary>
    public void ConvertToOrder(Guid orderId)
    {
        Status = CartStatus.Converted;

        AddDomainEvent(new CartConvertedToOrderEvent(Id, orderId));
    }

    /// <summary>
    /// Expires the cart
    /// </summary>
    public void Expire()
    {
        if (Status != CartStatus.Active)
            return;

        Status = CartStatus.Expired;

        AddDomainEvent(new CartExpiredEvent(Id));
    }

    /// <summary>
    /// Merges another cart into this one (e.g., when guest logs in)
    /// </summary>
    public void Merge(ShoppingCart otherCart)
    {
        if (otherCart == null || otherCart.IsEmpty)
            return;

        foreach (var otherItem in otherCart.Items)
        {
            var existingItem = Items.FirstOrDefault(i =>
                i.ProductId == otherItem.ProductId &&
                i.VariantId == otherItem.VariantId &&
                i.CustomAttributes == otherItem.CustomAttributes);

            if (existingItem != null)
            {
                // Combine quantities
                existingItem.Quantity += otherItem.Quantity;
                ValidateQuantity(existingItem);
            }
            else
            {
                // Add new item
                Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = Id,
                    ProductId = otherItem.ProductId,
                    VariantId = otherItem.VariantId,
                    Quantity = otherItem.Quantity,
                    UnitPrice = otherItem.UnitPrice,
                    CustomAttributes = otherItem.CustomAttributes,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        RecalculateTotals();

        AddDomainEvent(new CartMergedEvent(Id, otherCart.Id, Items.Count));
    }

    /// <summary>
    /// Clears all items and discounts from the cart
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        Discount = 0;
        TaxAmount = 0;
        ShippingCost = 0;
        AppliedCouponId = null;
        AppliedCouponCode = null;
        CouponDiscount = null;
        AppliedRewardPoints = 0;
        RewardPointsDiscount = 0;

        RecalculateTotals();

        AddDomainEvent(new CartClearedEvent(Id));
    }

    #endregion

    #region ═══════════════════════════════════════════════════════════════════
    // DOMAIN METHODS - Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates the cart for checkout
    /// </summary>
    /// <returns>List of validation errors (empty if valid)</returns>
    public List<string> ValidateForCheckout()
    {
        var errors = new List<string>();

        // Check if cart is empty
        if (IsEmpty)
            errors.Add("Cart is empty");

        // Check if cart is active
        if (Status != CartStatus.Active)
            errors.Add("Cart is not active");

        // Check if cart has expired
        if (IsExpired)
            errors.Add("Cart has expired");

        // Validate each item
        foreach (var item in Items)
        {
            if (!item.IsValid())
                errors.Add($"Item '{item.ProductName}' is invalid");

            if (!item.IsAvailable)
                errors.Add($"'{item.ProductName}' is no longer available");

            if (item.Quantity > item.AvailableStock)
                errors.Add($"Not enough stock for '{item.ProductName}'");
        }

        // Check shipping for physical items
        if (HasPhysicalItems && ShippingAddress == null)
            errors.Add("Shipping address is required for physical items");

        return errors;
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════════
// CART ITEM - Entity within Cart Aggregate
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Individual item in a shopping cart
/// </summary>
public class CartItem : Entity
{
    #region Identity
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public virtual ShoppingCart Cart { get; set; } = null!;
    #endregion

    #region Product Reference
    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public Guid? VariantId { get; set; }
    public virtual ProductVariant? Variant { get; set; }
    #endregion

    #region Quantity
    public int Quantity { get; set; }
    #endregion

    #region Pricing (at time of adding to cart)
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public decimal Discount { get; set; }
    public decimal FinalPrice => TotalPrice - Discount;
    #endregion

    #region Customization
    /// <summary>
    /// JSON string for custom attributes (e.g., engraving text, gift wrapping)
    /// </summary>
    public string? CustomAttributes { get; set; }
    #endregion

    #region Bundle
    /// <summary>
    /// Bundle ID if this item is part of a promotional bundle
    /// </summary>
    public Guid? BundleId { get; set; }
    #endregion

    #region Timestamps
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PriceChangedAt { get; set; }
    #endregion

    #region Computed Properties

    /// <summary>
    /// Product name (from product or variant)
    /// </summary>
    public string ProductName => Variant?.Name ?? Product?.Name ?? "Unknown Product";

    /// <summary>
    /// SKU of the product/variant
    /// </summary>
    public string Sku => Variant?.Sku ?? Product?.Sku ?? string.Empty;

    /// <summary>
    /// Image URL for display
    /// </summary>
    public string? ImageUrl => Variant?.ImageUrl ?? Product?.PrimaryImageUrl;

    /// <summary>
    /// Is the product still available?
    /// </summary>
    public bool IsAvailable => Product?.IsInStock == true || Product?.AllowBackorders == true;

    /// <summary>
    /// Available stock quantity
    /// </summary>
    public int AvailableStock => Product?.AvailableQuantity ?? 0;

    /// <summary>
    /// Has the price changed since adding to cart?
    /// </summary>
    public bool IsPriceChanged => Product != null && Product.CurrentPrice != UnitPrice;

    /// <summary>
    /// Price difference (positive = increased, negative = decreased)
    /// </summary>
    public decimal PriceDifference => Product != null ? Product.CurrentPrice - UnitPrice : 0;

    /// <summary>
    /// Is this a digital product?
    /// </summary>
    public bool IsDigital => Product?.ProductType == ProductType.Digital;

    /// <summary>
    /// Does this item require shipping?
    /// </summary>
    public bool RequiresShipping => Product?.RequiresShipping ?? true;

    #endregion

    #region Domain Methods

    /// <summary>
    /// Validates the cart item
    /// </summary>
    public bool IsValid()
    {
        return Product != null &&
               Quantity > 0 &&
               UnitPrice >= 0 &&
               (!Product.MaximumOrderQuantity.HasValue || Quantity <= Product.MaximumOrderQuantity);
    }

    /// <summary>
    /// Updates the quantity
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
        {
            Cart.RemoveItem(Id);
        }
        else
        {
            Quantity = newQuantity;

            if (IsPriceChanged)
            {
                PriceChangedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Applies a discount to this item
    /// </summary>
    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount > TotalPrice)
            discountAmount = TotalPrice;

        Discount = discountAmount;
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════════
// COUPON - Value Object / Reference Entity for Discounts
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Coupon for cart discounts
/// Can be used as a value object or reference entity
/// </summary>
public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    #region Type & Value
    public CouponType Type { get; set; }
    public decimal Value { get; set; } // Percentage or fixed amount
    public decimal? MaximumDiscount { get; set; } // Cap for percentage discounts
    #endregion

    #region Restrictions
    public decimal? MinimumOrderAmount { get; set; }
    public int? MinimumItemQuantity { get; set; }
    public List<Guid> ProductIds { get; set; } = new(); // Applicable product IDs
    public List<Guid> CategoryIds { get; set; } = new(); // Applicable category IDs
    public List<Guid> ExcludedProductIds { get; set; } = new(); // Excluded product IDs
    public int? UsageLimitPerUser { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int CurrentUsageCount { get; set; }
    #endregion

    #region Validity
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    #endregion

    #region Customer Restrictions
    public List<Guid> AllowedCustomerIds { get; set; } = new();
    public List<string> AllowedCustomerRoles { get; set; } = new();
    public List<string> AllowedCountries { get; set; } = new();
    #endregion

    #region Computed
    public bool IsPercentage => Type == CouponType.Percentage;
    public bool IsFixedAmount => Type == CouponType.FixedAmount;
    public bool IsValid() => IsActive &&
                             (!StartDate.HasValue || DateTime.UtcNow >= StartDate) &&
                             (!EndDate.HasValue || DateTime.UtcNow <= EndDate) &&
                             (!TotalUsageLimit.HasValue || CurrentUsageCount < TotalUsageLimit);
    #endregion

    #region Domain Methods

    /// <summary>
    /// Calculates the discount amount for a given subtotal
    /// </summary>
    public decimal CalculateDiscount(decimal subtotal)
    {
        if (!IsValid())
            return 0;

        decimal discount;

        if (IsPercentage)
        {
            discount = subtotal * (Value / 100);

            // Apply maximum discount cap if set
            if (MaximumDiscount.HasValue && discount > MaximumDiscount.Value)
                discount = MaximumDiscount.Value;
        }
        else // Fixed amount
        {
            discount = Value;

            // Don't exceed subtotal
            if (discount > subtotal)
                discount = subtotal;
        }

        return Math.Round(discount, 2);
    }

    /// <summary>
    /// Checks if this coupon can be applied to a specific product
    /// </summary>
    public bool IsApplicableToProduct(Guid productId, Guid? categoryId = null)
    {
        // Check exclusions first
        if (ExcludedProductIds.Contains(productId))
            return false;

        // If no restrictions, apply to all
        if (!ProductIds.Any() && !CategoryIds.Any())
            return true;

        // Check product restriction
        if (ProductIds.Any() && ProductIds.Contains(productId))
            return true;

        // Check category restriction
        if (CategoryIds.Any() && categoryId.HasValue && CategoryIds.Contains(categoryId.Value))
            return true;

        return false;
    }

    /// <summary>
    /// Records usage of this coupon
    /// </summary>
    public void RecordUsage()
    {
        CurrentUsageCount++;
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════════
// ENUMS
// ═══════════════════════════════════════════════════════════════════════════



/// <summary>
/// Cart status
/// </summary>
public enum CartStatus
{
    /// <summary>Cart is active and can be modified</summary>
    Active = 0,

    /// <summary>Cart has been converted to an order</summary>
    Converted = 1,

    /// <summary>Cart was abandoned (user left without purchasing)</summary>
    Abandoned = 2,

    /// <summary>Cart has expired due to inactivity</summary>
    Expired = 3,

    /// <summary>Cart was merged with another cart</summary>
    Merged = 4
}

/// <summary>
/// Shipping method options
/// </summary>
public enum ShippingMethod
{
    Standard = 0,
    Express = 1,
    NextDay = 2,
    International = 3,
    StorePickup = 4,
    DigitalDelivery = 5,
    Freight = 6
}

/// <summary>
/// Coupon type
/// </summary>
public enum CouponType
{
    /// <summary>Percentage off (e.g., 10% off)</summary>
    Percentage = 0,

    /// <summary>Fixed amount off (e.g., $10 off)</summary>
    FixedAmount = 1,

    /// <summary>Free shipping</summary>
    FreeShipping = 2,

    /// <summary>Buy X get Y free</summary>
    BuyXGetY = 3
}

// ═══════════════════════════════════════════════════════════════════════════
// DOMAIN EVENTS
// ═══════════════════════════════════════════════════════════════════════════



/// <summary>
/// Base event for cart-related events
/// </summary>
public abstract record CartEvent(Guid CartId) : IDomainEvent;

/// <summary>
/// Fired when cart is updated (items changed, totals recalculated)
/// </summary>
public record CartUpdatedEvent(Guid CartId, int ItemCount, decimal Total) : CartEvent(CartId);

/// <summary>
/// Fired when an item is added to the cart
/// </summary>
public record CartItemAddedEvent(Guid CartId, Guid ProductId, Guid? VariantId, int Quantity) : CartEvent(CartId);

/// <summary>
/// Fired when an item is updated in the cart
/// </summary>
public record CartItemUpdatedEvent(Guid CartId, Guid ProductId, int NewQuantity) : CartEvent(CartId);

/// <summary>
/// Fired when an item is removed from the cart
/// </summary>
public record CartItemRemovedEvent(Guid CartId, Guid ProductId, Guid? VariantId) : CartEvent(CartId);

/// <summary>
/// Fired when a coupon is applied
/// </summary>
public record CartCouponAppliedEvent(Guid CartId, string CouponCode, decimal Discount) : CartEvent(CartId);

/// <summary>
/// Fired when a coupon is removed
/// </summary>
public record CartCouponRemovedEvent(Guid CartId) : CartEvent(CartId);

/// <summary>
/// Fired when reward points are applied
/// </summary>
public record CartRewardPointsAppliedEvent(Guid CartId, int Points, decimal Discount) : CartEvent(CartId);

/// <summary>
/// Fired when shipping address is changed
/// </summary>
public record CartShippingAddressChangedEvent(Guid CartId, Guid AddressId) : CartEvent(CartId);

/// <summary>
/// Fired when shipping method is changed
/// </summary>
public record CartShippingMethodChangedEvent(Guid CartId, ShippingMethod Method, decimal Cost) : CartEvent(CartId);

/// <summary>
/// Fired when cart is abandoned
/// </summary>
public record CartAbandonedEvent(Guid CartId, int ItemCount, decimal Total) : CartEvent(CartId);

/// <summary>
/// Fired when cart is converted to order
/// </summary>
public record CartConvertedToOrderEvent(Guid CartId, Guid OrderId) : CartEvent(CartId);

/// <summary>
/// Fired when cart expires
/// </summary>
public record CartExpiredEvent(Guid CartId) : CartEvent(CartId);

/// <summary>
/// Fired when cart is merged with another
/// </summary>
public record CartMergedEvent(Guid TargetCartId, Guid SourceCartId, int TotalItems) : CartEvent(TargetCartId);

/// <summary>
/// Fired when cart is cleared
/// </summary>
public record CartClearedEvent(Guid CartId) : CartEvent(CartId);
