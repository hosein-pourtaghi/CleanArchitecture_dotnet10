using SharedKernel.BaseEntities;

namespace Domain.Entities.Products;
 

///// <summary>
///// Main product entity - Aggregate Root
///// Represents a sellable product with all its details
///// </summary>
//public class ProductAg : Entity
//{
//    #region Basic Properties
//    public Guid Id { get; set; }
//    public string Name { get; set; } = string.Empty;
//    public string Slug { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string ShortDescription { get; set; } = string.Empty;
//    public string Manufacturer { get; set; } = string.Empty;
//    public string ManufacturerPartNumber { get; set; } = string.Empty;
//    public string UniversalProductCode { get; set; } = string.Empty; // UPC
//    public string EuropeanArticleNumber { get; set; } = string.Empty; // EAN
//    public string GlobalTradeItemNumber { get; set; } = string.Empty; // GTIN

//    public ProductType ProductType { get; set; } = ProductType.Physical;
//    public ProductStatus Status { get; set; } = ProductStatus.Draft;
//    #endregion

//    #region Pricing
//    public decimal BasePrice { get; set; }
//    public decimal? CompareAtPrice { get; set; } // Original price before discount
//    public decimal CostPerItem { get; set; } // Your cost
//    public decimal? DiscountPercentage { get; set; }
//    public decimal? MinimumOrderQuantity { get; set; } = 1;
//    public decimal? MaximumOrderQuantity { get; set; }
//    public string Currency { get; set; } = "USD";

//    // Calculated
//    public decimal CurrentPrice => DiscountPercentage.HasValue
//        ? BasePrice * (1 - DiscountPercentage.Value / 100)
//        : BasePrice;
//    public bool IsOnSale => DiscountPercentage.HasValue && DiscountPercentage > 0;
//    public decimal ProfitMargin => BasePrice - CostPerItem;
//    public decimal ProfitPercentage => CostPerItem > 0 ? (ProfitMargin / CostPerItem) * 100 : 0;
//    #endregion

//    #region Shipping & Dimensions
//    public decimal? Weight { get; set; }
//    public WeightUnit WeightUnit { get; set; } = WeightUnit.Gram;
//    public decimal? Length { get; set; }
//    public decimal? Width { get; set; }
//    public decimal? Height { get; set; }
//    public DimensionUnit DimensionUnit { get; set; } = DimensionUnit.Centimeter;
//    public bool IsFreeShipping { get; set; }
//    public decimal? ShippingCost { get; set; }
//    public decimal? FreeShippingThreshold { get; set; }
//    public bool IsHazardousMaterial { get; set; }
//    public bool IsFragile { get; set; }
//    public bool RequiresShipping { get; set; } = true;
//    #endregion

//    #region Tax & Legal
//    public string TaxCategory { get; set; } = "default";
//    public decimal? TaxRate { get; set; }
//    public bool IsTaxable { get; set; } = true;
//    public string CountryOfOrigin { get; set; } = string.Empty;
//    public string HarmonizedSystemCode { get; set; } = string.Empty; // HS Code
//    #endregion

//    #region SEO
//    public string MetaTitle { get; set; } = string.Empty;
//    public string MetaDescription { get; set; } = string.Empty;
//    public string MetaKeywords { get; set; } = string.Empty;
//    public string CanonicalUrl { get; set; } = string.Empty;
//    #endregion

//    #region Ratings & Stats
//    public decimal AverageRating { get; set; }
//    public int TotalRatings { get; set; }
//    public int TotalReviews { get; set; }
//    public int TotalQuestions { get; set; }
//    public int TotalSold { get; set; }
//    public int TotalViews { get; set; }
//    public int TotalWishlists { get; set; }
//    #endregion

//    #region Timestamps
//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? UpdatedAt { get; set; }
//    public DateTime? PublishedAt { get; set; }
//    public DateTime? ExpiresAt { get; set; }
//    #endregion

//    #region Flags
//    public bool IsFeatured { get; set; }
//    public bool IsBestseller { get; set; }
//    public bool IsNewArrival { get; set; }
//    public bool IsReturnable { get; set; } = true;
//    public int? ReturnDays { get; set; } = 30;
//    public bool IsVerified { get; set; }
//    public bool IsOrganic { get; set; }
//    public bool IsFairTrade { get; set; }
//    #endregion

//    #region Search & Tags
//    public string Tags { get; set; } = string.Empty;
//    public string SearchKeywords { get; set; } = string.Empty;
//    #endregion

//    #region Inventory
//    public int StockQuantity { get; set; }
//    public int ReservedQuantity { get; set; }
//    public int AvailableQuantity => StockQuantity - ReservedQuantity;
//    public bool TrackInventory { get; set; } = true;
//    public bool AllowBackorders { get; set; }
//    public int? LowStockThreshold { get; set; } = 10;
//    public bool IsInStock => StockQuantity > 0;
//    public bool IsLowStock => StockQuantity <= (LowStockThreshold ?? 10);
//    #endregion

//    #region Digital Product
//    public string? DownloadUrl { get; set; }
//    public int? DownloadLimit { get; set; }
//    public int? DownloadExpiryDays { get; set; }
//    public string? SampleFileUrl { get; set; }
//    public long? FileSizeBytes { get; set; }
//    public string? FileType { get; set; }
//    #endregion

//    #region Subscription
//    public bool IsSubscription { get; set; }
//    public decimal? SubscriptionPrice { get; set; }
//    public int? SubscriptionIntervalDays { get; set; }
//    public decimal? SubscriptionDiscountPercentage { get; set; }
//    #endregion

//    #region Navigation Properties
//    public Guid CategoryId { get; set; }
//    public virtual ProductCategory Category { get; set; } = null!;

//    public Guid? BrandId { get; set; }
//    public virtual ProductBrand? Brand { get; set; }

//    public Guid? ProductGroupId { get; set; }
//    public virtual ProductGroup? ProductGroup { get; set; }

//    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
//    public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
//    public virtual ICollection<ProductInventory> InventoryHistory { get; set; } = new List<ProductInventory>();
//    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
//    public virtual ICollection<ProductQuestion> Questions { get; set; } = new List<ProductQuestion>();
//    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
//    public virtual ICollection<RelatedProduct> RelatedProducts { get; set; } = new List<RelatedProduct>();
//    public virtual ICollection<RelatedProduct> RelatedToProducts { get; set; } = new List<RelatedProduct>();
//    #endregion

//    #region Domain Methods
//    public void AddImage(ProductImage image)
//    {
//        image.ProductId = this.Id;
//        Images.Add(image);
//    }

//    public void AddAttribute(ProductAttribute attribute)
//    {
//        attribute.ProductId = this.Id;
//        Attributes.Add(attribute);
//    }

//    public void UpdateStock(int quantity, string reason)
//    {
//        StockQuantity += quantity;
//        InventoryHistory.Add(new ProductInventory
//        {
//            ProductId = Id,
//            Quantity = quantity,
//            Reason = reason,
//            CreatedAt = DateTime.UtcNow,
//            StockAfterTransaction = StockQuantity
//        });
//    }

//    public void ReserveStock(int quantity)
//    {
//        if (AvailableQuantity < quantity)
//            throw new InvalidOperationException("Insufficient stock");
//        ReservedQuantity += quantity;
//    }

//    public void ReleaseReservedStock(int quantity)
//    {
//        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
//    }

//    public void ConfirmReservation(int quantity)
//    {
//        ReservedQuantity -= quantity;
//        StockQuantity -= quantity;
//    }

//    public void UpdateRating()
//    {
//        if (Reviews.Any())
//        {
//            AverageRating = Reviews.Average(r => r.Rating);
//            TotalRatings = Reviews.Count(r => r.Rating > 0);
//            TotalReviews = Reviews.Count;
//        }
//    }

//    public void Publish()
//    {
//        Status = ProductStatus.Active;
//        PublishedAt = DateTime.UtcNow;
//    }

//    public void Archive() => Status = ProductStatus.Archived;
//    public void Discontinue() => Status = ProductStatus.Discontinued;
//    #endregion
//}






//// Domain/Entities/Products/ProductCategory.cs 

///// <summary>
///// Product categories for organizing products (hierarchical structure)
///// </summary>
//public class ProductCategory : Entity
//{
//    public Guid Id { get; set; }
//    public string Name { get; set; } = string.Empty;
//    public string Slug { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string ImageUrl { get; set; } = string.Empty;
//    public string Icon { get; set; } = string.Empty;

//    public int DisplayOrder { get; set; }
//    public bool IsActive { get; set; } = true;
//    public bool ShowInMenu { get; set; }
//    public bool ShowInHomePage { get; set; }

//    public string MetaTitle { get; set; } = string.Empty;
//    public string MetaDescription { get; set; } = string.Empty;

//    public int ProductCount { get; set; }

//    // Hierarchical structure
//    public Guid? ParentId { get; set; }
//    public virtual ProductCategory? Parent { get; set; }
//    public virtual ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();

//    public virtual ICollection<ProductAg> Products { get; set; } = new List<ProductAg>();

//    public string GetBreadcrumb()
//    {
//        var parts = new List<string> { Name };
//        var current = Parent;
//        while (current != null)
//        {
//            parts.Insert(0, current.Name);
//            current = current.Parent;
//        }
//        return string.Join(" > ", parts);
//    }
//}




//// Domain/Entities/Products/ProductBrand.cs 

///// <summary>
///// Product brand/manufacturer
///// </summary>
//public class ProductBrand : Entity
//{
//    public Guid Id { get; set; }
//    public string Name { get; set; } = string.Empty;
//    public string Slug { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string LogoUrl { get; set; } = string.Empty;
//    public string WebsiteUrl { get; set; } = string.Empty;
//    public string Country { get; set; } = string.Empty;

//    public bool IsVerified { get; set; }
//    public bool IsActive { get; set; } = true;
//    public int DisplayOrder { get; set; }

//    public string MetaTitle { get; set; } = string.Empty;
//    public string MetaDescription { get; set; } = string.Empty;

//    public int ProductCount { get; set; }

//    // Social media
//    public string FacebookUrl { get; set; } = string.Empty;
//    public string InstagramUrl { get; set; } = string.Empty;
//    public string TwitterUrl { get; set; } = string.Empty;
//    public string YouTubeUrl { get; set; } = string.Empty;

//    // Contact
//    public string ContactEmail { get; set; } = string.Empty;
//    public string ContactPhone { get; set; } = string.Empty;

//    public virtual ICollection<ProductAg> Products { get; set; } = new List<ProductAg>();
//}





//// Domain/Entities/Products/ProductGroup.cs 

///// <summary>
///// Groups related product variants (e.g., iPhone 15, iPhone 15 Pro)
///// </summary>
//public class ProductGroup : Entity
//{
//    public Guid Id { get; set; }
//    public string Name { get; set; } = string.Empty; // e.g., "iPhone 15"
//    public string SkuPrefix { get; set; } = string.Empty; // e.g., "IPHONE15"

//    public Guid CategoryId { get; set; }
//    public virtual ProductCategory Category { get; set; } = null!;

//    public Guid? BrandId { get; set; }
//    public virtual ProductBrand? Brand { get; set; }

//    public bool IsActive { get; set; } = true;
//    public string BaseDescription { get; set; } = string.Empty;
//    public string BaseImageUrl { get; set; } = string.Empty;

//    public virtual ICollection<ProductAg> Variants { get; set; } = new List<ProductAg>();
//    public virtual ICollection<ProductAttribute> CommonAttributes { get; set; } = new List<ProductAttribute>();
//}

//// Domain/Entities/Products/ProductVariant.cs 

///// <summary>
///// Product variant (specific size/color combination)
///// </summary>
//public class ProductVariant : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public string Sku { get; set; } = string.Empty;
//    public string Name { get; set; } = string.Empty;

//    // Variant attributes
//    public string? Color { get; set; }
//    public string? Size { get; set; }
//    public string? Material { get; set; }
//    public string? Pattern { get; set; }
//    public string? Flavor { get; set; }
//    public string? Scent { get; set; }
//    public string? Capacity { get; set; }
//    public string? Storage { get; set; }
//    public string? Ram { get; set; }
//    public string? ScreenSize { get; set; }
//    public string? Edition { get; set; }
//    public string? CustomAttributes { get; set; } // JSON

//    public decimal Price { get; set; }
//    public decimal? CompareAtPrice { get; set; }

//    public int StockQuantity { get; set; }
//    public bool IsActive { get; set; } = true;

//    public string? ImageUrl { get; set; }
//    public string? Barcode { get; set; }

//    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
//}

//// Domain/Entities/Products/ProductImage.cs

///// <summary>
///// Product images including multiple sizes and types
///// </summary>
//public class ProductImage : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public Guid? VariantId { get; set; }
//    public virtual ProductVariant? Variant { get; set; }

//    public string Url { get; set; } = string.Empty;
//    public string? ThumbnailUrl { get; set; }
//    public string? MediumUrl { get; set; }
//    public string? LargeUrl { get; set; }

//    public string? AltText { get; set; }
//    public string? Title { get; set; }

//    public int DisplayOrder { get; set; }
//    public bool IsPrimary { get; set; }
//    public bool IsHover { get; set; }
//    public bool Is360 { get; set; }
//    public bool IsVideo { get; set; }
//    public string? VideoUrl { get; set; }
//}

//// Domain/Entities/Products/ProductAttribute.cs

///// <summary>
///// Product attributes (specifications)
///// </summary>
//public class ProductAttribute : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public Guid? ProductGroupId { get; set; }
//    public virtual ProductGroup? ProductGroup { get; set; }

//    public string Name { get; set; } = string.Empty; // e.g., "Color"
//    public string Value { get; set; } = string.Empty; // e.g., "Red"
//    public string? Unit { get; set; } // e.g., "GB"

//    public int DisplayOrder { get; set; }
//    public bool IsFilterable { get; set; }
//    public bool IsComparable { get; set; }
//    public bool IsRequired { get; set; }
//    public bool IsVariantAttribute { get; set; }
//}

//// Domain/Entities/Products/ProductInventory.cs

///// <summary>
///// Inventory transaction history
///// </summary>
//public class ProductInventory : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public int Quantity { get; set; } // Positive = add, Negative = remove
//    public string Reason { get; set; } = string.Empty; // "Purchase", "Sale", "Return"

//    public string? ReferenceNumber { get; set; }
//    public string? Notes { get; set; }
//    public Guid? ProcessedBy { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public int StockAfterTransaction { get; set; }
//}

//// Domain/Entities/Products/ProductReview.cs

 

///// <summary>
///// Product review from customers
///// </summary>
//public class ProductReview : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    public Guid? OrderId { get; set; }
//    public virtual Order? Order { get; set; }

//    public int Rating { get; set; } // 1-5
//    public string Title { get; set; } = string.Empty;
//    public string Content { get; set; } = string.Empty;

//    public string? Pros { get; set; }
//    public string? Cons { get; set; }

//    public bool IsVerifiedPurchase { get; set; }
//    public bool IsFeatured { get; set; }

//    public int HelpfulCount { get; set; }
//    public int NotHelpfulCount { get; set; }

//    public string? Images { get; set; } // JSON array
//    public string? Videos { get; set; } // JSON array

//    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
//    public string? RejectionReason { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? UpdatedAt { get; set; }
//    public DateTime? VerifiedAt { get; set; }

//    public string? SellerResponse { get; set; }
//    public DateTime? SellerResponseAt { get; set; }
//}

//// Domain/Entities/Products/ProductQuestion.cs

///// <summary>
///// Customer questions about products
///// </summary>
//public class ProductQuestion : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public Guid AskedByCustomerId { get; set; }
//    public virtual Customer AskedByCustomer { get; set; } = null!;

//    public string Question { get; set; } = string.Empty;

//    public bool IsAnswered { get; set; }
//    public string? Answer { get; set; }
//    public Guid? AnsweredById { get; set; }
//    public DateTime? AnsweredAt { get; set; }

//    public bool IsCommunityAnswer { get; set; }
//    public Guid? CommunityAnsweredById { get; set; }

//    public int UpvoteCount { get; set; }
//    public int DownvoteCount { get; set; }

//    public bool IsHidden { get; set; }
//    public string? HiddenReason { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//}

//// Domain/Entities/Products/RelatedProduct.cs

///// <summary>
///// Related products (frequently bought together, similar items, etc.)
///// </summary>
//public class RelatedProduct : Entity
//{
//    public Guid Id { get; set; }
//    public Guid ProductId { get; set; }
//    public virtual ProductAg Product { get; set; } = null!;

//    public Guid RelatedProductId { get; set; }
//    public virtual ProductAg RelatedProductNavigation { get; set; } = null!;

//    public RelatedProductType Type { get; set; } = RelatedProductType.FrequentlyBoughtTogether;
//    public int DisplayOrder { get; set; }
//}

//public enum RelatedProductType
//{
//    FrequentlyBoughtTogether = 0,
//    Similar = 1,
//    Recommended = 2,
//    AlsoViewed = 3,
//    CompareWith = 4
//}
