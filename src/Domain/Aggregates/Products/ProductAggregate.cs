using Domain.Aggregates.Customers;
using Domain.Aggregates.Orders;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Products;

/// <summary>
/// Product Aggregate Root
/// Manages products, variants, images, attributes, and related catalog data
/// </summary>
public class Product : Entity
{
    #region Identity
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    #endregion

    #region Basic Info
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public ProductType ProductType { get; set; } = ProductType.Physical;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    #endregion

    #region Identification Codes
    public string? Manufacturer { get; set; }
    public string? ManufacturerPartNumber { get; set; }
    public string? UniversalProductCode { get; set; } // UPC
    public string? EuropeanArticleNumber { get; set; } // EAN
    public string? GlobalTradeItemNumber { get; set; } // GTIN
    public string? Barcode { get; set; }
    #endregion

    #region Pricing
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; } // Original price before discount
    public decimal CostPerItem { get; set; } // Your cost
    public decimal? DiscountPercentage { get; set; }
    public int? MinimumOrderQuantity { get; set; } = 1;
    public int? MaximumOrderQuantity { get; set; }
    public string Currency { get; set; } = "USD";

    // Computed
    public decimal CurrentPrice => DiscountPercentage.HasValue
        ? Math.Round(BasePrice * (1 - DiscountPercentage.Value / 100), 2)
        : BasePrice;
    public bool IsOnSale => DiscountPercentage.HasValue && DiscountPercentage > 0 && Status == ProductStatus.Active;
    public decimal Savings => CompareAtPrice.HasValue ? CompareAtPrice.Value - CurrentPrice : 0;
    public decimal ProfitMargin => CurrentPrice - CostPerItem;
    public decimal ProfitPercentage => CostPerItem > 0 ? (ProfitMargin / CostPerItem) * 100 : 0;
    #endregion

    #region Shipping & Dimensions
    public decimal? Weight { get; set; }
    public WeightUnit WeightUnit { get; set; } = WeightUnit.Gram;
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public DimensionUnit DimensionUnit { get; set; } = DimensionUnit.Centimeter;
    public bool IsFreeShipping { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    public bool IsHazardousMaterial { get; set; }
    public bool IsFragile { get; set; }
    public bool RequiresShipping { get; set; } = true;
    #endregion

    #region Tax
    public string TaxCategory { get; set; } = "default";
    public decimal? TaxRate { get; set; }
    public bool IsTaxable { get; set; } = true;
    #endregion

    #region Origin
    public string CountryOfOrigin { get; set; } = string.Empty;
    public string? HarmonizedSystemCode { get; set; } // HS Code
    #endregion

    #region SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }
    #endregion

    #region Flags
    public bool IsFeatured { get; set; }
    public bool IsBestseller { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsReturnable { get; set; } = true;
    public int? ReturnDays { get; set; } = 30;
    public bool IsVerified { get; set; }
    public bool IsOrganic { get; set; }
    public bool IsFairTrade { get; set; }
    public bool AllowBackorders { get; set; }
    #endregion

    #region Inventory
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public bool TrackInventory { get; set; } = true;
    public int? LowStockThreshold { get; set; } = 10;
    public bool IsInStock => AllowBackorders || StockQuantity > 0;
    public bool IsLowStock => LowStockThreshold.HasValue && StockQuantity <= LowStockThreshold.Value && StockQuantity > 0;
    public bool IsOutOfStock => StockQuantity <= 0 && !AllowBackorders;
    #endregion

    #region Digital Product
    public string? DownloadUrl { get; set; }
    public int? DownloadLimit { get; set; }
    public int? DownloadExpiryDays { get; set; }
    public string? SampleFileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileType { get; set; }
    #endregion

    #region Subscription
    public bool IsSubscription { get; set; }
    public decimal? SubscriptionPrice { get; set; }
    public int? SubscriptionIntervalDays { get; set; }
    public decimal? SubscriptionDiscountPercentage { get; set; }
    #endregion

    #region Stats
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int TotalReviews { get; set; }
    public int TotalSold { get; set; }
    public int TotalViews { get; set; }
    public int TotalWishlists { get; set; }
    public DateTime? LastSoldAt { get; set; }
    #endregion

    #region Tags & Search
    public string Tags { get; set; } = string.Empty;
    public string SearchKeywords { get; set; } = string.Empty;
    #endregion

    #region Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    #endregion

    #region Navigation
    public Guid CategoryId { get; set; }
    public virtual ProductCategory Category { get; set; } = null!;
    public Guid? BrandId { get; set; }
    public virtual ProductBrand? Brand { get; set; }
    public Guid? ProductGroupId { get; set; }
    public virtual ProductGroup? ProductGroup { get; set; }

    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductInventory> InventoryHistory { get; set; } = new List<ProductInventory>();
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<ProductQuestion> Questions { get; set; } = new List<ProductQuestion>();
    #endregion

    #region Computed
    public string? PrimaryImageUrl => Images.FirstOrDefault(i => i.IsPrimary)?.Url ??
                                      Images.FirstOrDefault()?.Url;
    public List<string> ImageUrls => Images.Select(i => i.Url).ToList();
    public bool HasVariants => Variants.Any();
    public bool HasDiscount => IsOnSale;
    #endregion

    #region Domain Methods - Status

    public void Publish()
    {
        if (Status != ProductStatus.Draft && Status != ProductStatus.Inactive)
            throw new InvalidOperationException("Can only publish draft or inactive products");

        Status = ProductStatus.Active;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductPublishedEvent(Id, Name));
    }

    public void Unpublish()
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductUnpublishedEvent(Id));
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductDiscontinuedEvent(Id, Name));
    }

    public void Delete()
    {
        Status = ProductStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - Inventory

    public void UpdateStock(int quantityDelta, string reason, Guid? orderId = null)
    {
        if (quantityDelta < 0 && AvailableQuantity < Math.Abs(quantityDelta))
            throw new InvalidOperationException("Insufficient stock");

        StockQuantity += quantityDelta;

        // Record inventory history
        InventoryHistory.Add(new ProductInventory
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Quantity = quantityDelta,
            Reason = reason,
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            StockAfterTransaction = StockQuantity
        });

        // Check for low stock
        if (IsLowStock)
        {
            AddDomainEvent(new ProductLowStockEvent(Id, StockQuantity, Name));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity, Guid orderId)
    {
        if (AvailableQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock to reserve");

        ReservedQuantity += quantity;

        InventoryHistory.Add(new ProductInventory
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Quantity = -quantity,
            Reason = $"Reserved for order {orderId}",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            StockAfterTransaction = AvailableQuantity
        });

        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmReservation(int quantity, Guid orderId)
    {
        if (ReservedQuantity < quantity)
            throw new InvalidOperationException("Not enough reserved quantity");

        ReservedQuantity -= quantity;
        StockQuantity -= quantity;

        InventoryHistory.Add(new ProductInventory
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Quantity = -quantity,
            Reason = $"Confirmed order {orderId}",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            StockAfterTransaction = StockQuantity
        });

        TotalSold += quantity;
        LastSoldAt = DateTime.UtcNow;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(int quantity, string reason)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);

        InventoryHistory.Add(new ProductInventory
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Quantity = quantity,
            Reason = $"Released: {reason}",
            CreatedAt = DateTime.UtcNow,
            StockAfterTransaction = AvailableQuantity
        });

        UpdatedAt = DateTime.UtcNow;
    }

    public void Restock(int quantity, string reason, Guid? receivedFromOrderId = null)
    {
        UpdateStock(quantity, $"Restocked: {reason}", receivedFromOrderId);
    }

    #endregion

    #region Domain Methods - Pricing

    public void ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100");

        DiscountPercentage = percentage;

        // Set compare at price if not already set
        CompareAtPrice ??= BasePrice;

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductDiscountAppliedEvent(Id, Name, percentage));
    }

    public void RemoveDiscount()
    {
        DiscountPercentage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductPriceChangedEvent(Id, Name, newPrice));
    }

    #endregion

    #region Domain Methods - Child Entities

    public ProductImage AddImage(string url, bool isPrimary = false, string? altText = null)
    {
        if (isPrimary)
        {
            foreach (var img in Images)
                img.IsPrimary = false;
        }

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Url = url,
            AltText = altText,
            IsPrimary = isPrimary || !Images.Any(),
            DisplayOrder = Images.Count,
        };

        Images.Add(image);
        return image;
    }

    public ProductVariant AddVariant(Action<ProductVariant> configure)
    {
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Sku = $"{Sku}-VAR{Images.Count + 1}",
        };

        configure(variant);
        Variants.Add(variant);
        return variant;
    }

    public ProductAttribute AddAttribute(string name, string value, string? unit = null)
    {
        var attribute = new ProductAttribute
        {
            Id = Guid.NewGuid(),
            ProductId = Id,
            Name = name,
            Value = value,
            Unit = unit,
            DisplayOrder = Attributes.Count,
        };

        Attributes.Add(attribute);
        return attribute;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            Images.Remove(image);
            if (image.IsPrimary && Images.Any())
                Images.First().IsPrimary = true;
        }
    }

    public void RemoveVariant(Guid variantId)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant != null)
            Variants.Remove(variant);
    }

    #endregion

    #region Domain Methods - Rating

    public void UpdateRating()
    {
        var approvedReviews = Reviews.Where(r => r.Status == ReviewStatus.Approved && r.Rating > 0).ToList();

        if (approvedReviews.Any())
        {
            AverageRating = (decimal)Math.Round(approvedReviews.Average(r => r.Rating), 1);
            TotalRatings = approvedReviews.Count;
            TotalReviews = approvedReviews.Count(r => !string.IsNullOrEmpty(r.Content));
        }
        else
        {
            AverageRating = 0;
            TotalRatings = 0;
            TotalReviews = 0;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - Views

    public void RecordView()
    {
        TotalViews++;
    }

    #endregion
}

/// <summary>
/// Product category for organizing products (hierarchical)
/// Part of Product aggregate
/// </summary>
public class ProductCategory : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInMenu { get; set; }
    public bool ShowInHomePage { get; set; }

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public int ProductCount { get; private set; }

    // Hierarchy
    public Guid? ParentId { get; set; }
    public virtual ProductCategory? Parent { get; set; }
    public virtual ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();

    // SEO
    public string? CanonicalSlug { get; set; }

    #region Computed
    public string Breadcrumb => GetBreadcrumb();
    public int Depth => Parent == null ? 0 : 1 + (Parent.Depth);
    public bool IsRoot => ParentId == null;
    public bool IsLeaf => !Children.Any();
    #endregion

    #region Domain Methods

    public string GetBreadcrumb()
    {
        var parts = new List<string> { Name };
        var current = Parent;
        while (current != null)
        {
            parts.Insert(0, current.Name);
            current = current.Parent;
        }
        return string.Join(" > ", parts);
    }

    public IEnumerable<ProductCategory> GetAllAncestors()
    {
        var ancestors = new List<ProductCategory>();
        var current = Parent;
        while (current != null)
        {
            ancestors.Add(current);
            current = current.Parent;
        }
        return ancestors;
    }

    public IEnumerable<ProductCategory> GetAllDescendants()
    {
        var descendants = new List<ProductCategory>();
        foreach (var child in Children)
        {
            descendants.Add(child);
            descendants.AddRange(child.GetAllDescendants());
        }
        return descendants;
    }

    public void UpdateProductCount()
    {
        ProductCount = Children.Sum(c => c.ProductCount) +
                       Children.SelectMany(c => c.GetAllDescendants()).Sum(c => c.ProductCount);
    }

    #endregion
}

/// <summary>
/// Product brand/manufacturer
/// Part of Product aggregate
/// </summary>
public class ProductBrand : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string Country { get; set; } = string.Empty;

    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public int ProductCount { get; private set; }

    #region Social Media
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? YouTubeUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    #endregion

    #region Contact
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? SupportEmail { get; set; }
    #endregion
}

/// <summary>
/// Groups related product variants (e.g., iPhone 15 family)
/// Part of Product aggregate
/// </summary>
public class ProductGroup : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SkuPrefix { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public virtual ProductCategory Category { get; set; } = null!;

    public Guid? BrandId { get; set; }
    public virtual ProductBrand? Brand { get; set; }

    public bool IsActive { get; set; } = true;
    public string? BaseDescription { get; set; }
    public string? BaseImageUrl { get; set; }
    public string? BaseVideoUrl { get; set; }

    public virtual ICollection<Product> Variants { get; set; } = new List<Product>();
    public virtual ICollection<ProductAttribute> CommonAttributes { get; set; } = new List<ProductAttribute>();

    #region Computed
    public decimal? MinPrice => Variants.Any() ? Variants.Min(v => v.CurrentPrice) : null;
    public decimal? MaxPrice => Variants.Any() ? Variants.Max(v => v.CurrentPrice) : null;
    public int TotalStock => Variants.Sum(v => v.StockQuantity);
    public bool IsInStock => Variants.Any(v => v.IsInStock);
    #endregion
}

/// <summary>
/// Product variant (specific combination like "Red / Large")
/// Part of Product aggregate
/// </summary>
public class ProductVariant : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // e.g., "Red - Large"
    public bool IsActive { get; set; } = true;

    // Variant Attributes (specific to this variant)
    public string? Color { get; set; }
    public string? ColorHex { get; set; } // e.g., "#FF0000"
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Pattern { get; set; }
    public string? Flavor { get; set; }
    public string? Scent { get; set; }
    public string? Capacity { get; set; }
    public string? Storage { get; set; }
    public string? Ram { get; set; }
    public string? ScreenSize { get; set; }
    public string? Edition { get; set; }
    public string? Style { get; set; }
    public string? CustomAttributes { get; set; } // JSON for additional custom attributes

    // Pricing (can override product price)
    public decimal? PriceOverride { get; set; }
    public decimal? CompareAtPriceOverride { get; set; }
    public decimal Price => PriceOverride ?? Product.BasePrice;
    public decimal? CompareAtPrice => CompareAtPriceOverride ?? Product.CompareAtPrice;
    public bool IsOnSale => Product.IsOnSale || (CompareAtPriceOverride.HasValue && CompareAtPriceOverride > Price);

    // Inventory
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public string? Barcode { get; set; }

    // Media
    public string? ImageUrl { get; set; }
    public virtual ICollection<ProductImage> AdditionalImages { get; set; } = new List<ProductImage>();

    // Weight (can override product weight for shipping calculation)
    public decimal? WeightOverride { get; set; }
    public decimal Weight => WeightOverride ?? Product.Weight ?? 0;

    #region Computed
    public string VariantDescription => string.Join(" / ",
        new[] { Color, Size, Material, Storage, Ram }.Where(s => !string.IsNullOrEmpty(s)));

    public bool IsInStock => Product.AllowBackorders || StockQuantity > 0;
    public bool IsLowStock => Product.LowStockThreshold.HasValue &&
                               StockQuantity <= Product.LowStockThreshold.Value &&
                               StockQuantity > 0;
    #endregion

    #region Domain Methods

    public void UpdatePrice(decimal newPrice)
    {
        PriceOverride = newPrice;
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity = quantity;
    }

    public void Reserve(int quantity)
    {
        if (AvailableQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");
        ReservedQuantity += quantity;
    }

    public void ReleaseReservation(int quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
    }

    public void ConfirmSale(int quantity)
    {
        ReservedQuantity -= quantity;
        StockQuantity -= quantity;
    }

    #endregion
}

/// <summary>
/// Product image
/// Part of Product aggregate
/// </summary>
public class ProductImage : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public Guid? VariantId { get; set; }
    public virtual ProductVariant? Variant { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? MediumUrl { get; set; }
    public string? LargeUrl { get; set; }
    public string? ZoomUrl { get; set; }

    public string? AltText { get; set; }
    public string? Title { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsHover { get; set; }
    public bool Is360 { get; set; } // 360-degree view
    public bool IsVideo { get; set; }
    public string? VideoUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
}

/// <summary>
/// Product attribute/specification
/// Part of Product aggregate
/// </summary>
public class ProductAttribute : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public Guid? ProductGroupId { get; set; }
    public virtual ProductGroup? ProductGroup { get; set; }

    public string Name { get; set; } = string.Empty; // e.g., "Color", "Material"
    public string Value { get; set; } = string.Empty; // e.g., "Red", "Cotton"
    public string? Unit { get; set; } // e.g., "GB", "oz"

    public int DisplayOrder { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsComparable { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVariantAttribute { get; set; } // True if this defines a variant (e.g., Color)
}

/// <summary>
/// Inventory transaction history
/// Part of Product aggregate
/// </summary>
public class ProductInventory : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int Quantity { get; set; } // Positive = add, Negative = remove
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; } // Order ID, PO number, etc.
    public Guid? OrderId { get; set; }
    public string? Notes { get; set; }
    public Guid? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int StockAfterTransaction { get; set; }

    #region Common Reasons
    public static class Reasons
    {
        public const string InitialStock = "Initial Stock";
        public const string Purchase = "Purchase Order Received";
        public const string Sale = "Order Fulfilled";
        public const string Return = "Customer Return";
        public const string Adjustment = "Inventory Adjustment";
        public const string Damaged = "Damaged Goods";
        public const string Lost = "Lost/Missing";
        public const string Reserved = "Reserved for Order";
        public const string Released = "Reservation Released";
        public const string Correction = "Stock Correction";
    }
}

/// <summary>
/// Product review from customers
/// Part of Product aggregate
/// </summary>
public class ProductReview : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    #region Review Content
    public int Rating { get; set; } // 1-5
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    #endregion

    #region Verification
    public bool IsVerifiedPurchase { get; set; }
    public DateTime? VerifiedAt { get; set; }
    #endregion

    #region Media
    public string? Images { get; set; } // JSON array of image URLs
    public string? Videos { get; set; } // JSON array of video URLs
    #endregion

    #region Feedback
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public bool IsFeatured { get; set; }
    #endregion

    #region Moderation
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public string? RejectionReason { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    #endregion

    #region Seller Response
    public string? SellerResponse { get; set; }
    public DateTime? SellerResponseAt { get; set; }
    public Guid? SellerResponseBy { get; set; }
    #endregion

    #region Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    #endregion

    #region Domain Methods

    public void Approve(Guid moderatedBy)
    {
        Status = ReviewStatus.Approved;
        ModeratedBy = moderatedBy;
        ModeratedAt = DateTime.UtcNow;
        PublishedAt = DateTime.UtcNow;

        Product.UpdateRating();
    }

    public void Reject(string reason, Guid moderatedBy)
    {
        Status = ReviewStatus.Rejected;
        RejectionReason = reason;
        ModeratedBy = moderatedBy;
        ModeratedAt = DateTime.UtcNow;
    }

    public void Flag(string reason)
    {
        Status = ReviewStatus.Flagged;
    }

    public void MarkAsSpam()
    {
        Status = ReviewStatus.Spam;
    }

    public void AddSellerResponse(string response, Guid sellerId)
    {
        SellerResponse = response;
        SellerResponseBy = sellerId;
        SellerResponseAt = DateTime.UtcNow;
    }

    public void RecordHelpfulVote(bool isHelpful)
    {
        if (isHelpful)
            HelpfulCount++;
        else
            NotHelpfulCount++;
    }

    #endregion
}

/// <summary>
/// Customer questions about products
/// Part of Product aggregate
/// </summary>
public class ProductQuestion : Entity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public Guid AskedByCustomerId { get; set; }
    public virtual Customer AskedByCustomer { get; set; } = null!;

    public string Question { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }

    // Official Answer
    public string? Answer { get; set; }
    public Guid? AnsweredById { get; set; }
    public DateTime? AnsweredAt { get; set; }

    // Community Answers
    public bool IsCommunityAnswer { get; set; }
    public Guid? CommunityAnsweredById { get; set; }
    public string? CommunityAnswer { get; set; }
    public DateTime? CommunityAnsweredAt { get; set; }

    // Voting
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }

    // Visibility
    public bool IsHidden { get; set; }
    public string? HiddenReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    #region Domain Methods

    public void AddOfficialAnswer(string answer, Guid answeredById)
    {
        Answer = answer;
        AnsweredById = answeredById;
        AnsweredAt = DateTime.UtcNow;
        IsAnswered = true;
    }

    public void AddCommunityAnswer(string answer, Guid customerId)
    {
        CommunityAnswer = answer;
        CommunityAnsweredById = customerId;
        CommunityAnsweredAt = DateTime.UtcNow;
        IsAnswered = true;
    }

    public void Hide(string reason)
    {
        IsHidden = true;
        HiddenReason = reason;
    }

    public void Show()
    {
        IsHidden = false;
        HiddenReason = null;
    }

    public void Vote(bool isUpvote)
    {
        if (isUpvote)
            UpvoteCount++;
        else
            DownvoteCount++;
    }

    #endregion
}

// ============================================================================
// ENUMS - Product Aggregate
// ============================================================================

public enum ProductStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    Inactive = 3,
    Archived = 4,
    Discontinued = 5,
    Deleted = 6
}

public enum ProductType
{
    Physical = 0,
    Digital = 1,
    Service = 2,
    Subscription = 3
}

public enum WeightUnit
{
    Gram = 0,
    Kilogram = 1,
    Pound = 2,
    Ounce = 3
}

public enum DimensionUnit
{
    Centimeter = 0,
    Meter = 1,
    Inch = 2,
    Foot = 3
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Flagged = 3,
    Spam = 4
}

// ============================================================================
// DOMAIN EVENTS - Product Aggregate
// ============================================================================

public record ProductPublishedEvent(Guid ProductId, string ProductName) : IDomainEvent;
public record ProductUnpublishedEvent(Guid ProductId) : IDomainEvent;
public record ProductDiscontinuedEvent(Guid ProductId, string ProductName) : IDomainEvent;
public record ProductLowStockEvent(Guid ProductId, int CurrentStock, string ProductName) : IDomainEvent;
public record ProductDiscountAppliedEvent(Guid ProductId, string ProductName, decimal Percentage) : IDomainEvent;
public record ProductPriceChangedEvent(Guid ProductId, string ProductName, decimal NewPrice) : IDomainEvent;
#endregion
