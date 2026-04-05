using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel.BaseEntities;

namespace Domain.Products;



//// Domain/Entities/Orders/Order.cs 

///// <summary>
///// Order - Aggregate Root for purchases
///// </summary>
//public class Order : Entity
//{
//    public Guid Id { get; set; }
//    public string OrderNumber { get; set; } = string.Empty; // e.g., "ORD-2024-001234"

//    public Guid CustomerId { get; set; }
//    public virtual Customer Customer { get; set; } = null!;

//    public OrderStatus Status { get; set; } = OrderStatus.Pending;

//    #region Pricing
//    public decimal Subtotal { get; set; }
//    public decimal TaxAmount { get; set; }
//    public decimal ShippingCost { get; set; }
//    public decimal DiscountAmount { get; set; }
//    public decimal? GiftWrapAmount { get; set; }
//    public decimal? ServiceFee { get; set; }
//    public decimal TotalAmount { get; set; }
//    public string Currency { get; set; } = "USD";

//    public int PointsEarned { get; set; }
//    public int PointsRedeemed { get; set; }
//    public decimal PointsValue { get; set; }
//    #endregion

//    #region Discounts
//    public string? CouponCode { get; set; }
//    public decimal? CouponDiscount { get; set; }
//    public string? CampaignId { get; set; }
//    public string? CampaignName { get; set; }
//    #endregion

//    #region Payment
//    public PaymentMethod PaymentMethod { get; set; }
//    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
//    public string? PaymentReference { get; set; }
//    public string? PaymentGateway { get; set; }
//    public DateTime? PaidAt { get; set; }
//    public DateTime? RefundedAt { get; set; }
//    public decimal? RefundAmount { get; set; }
//    #endregion

//    #region Shipping
//    public ShippingMethod ShippingMethod { get; set; }
//    public string? TrackingNumber { get; set; }
//    public string? TrackingUrl { get; set; }
//    public string? CarrierName { get; set; }
//    public DateTime? ShippedAt { get; set; }
//    public DateTime? DeliveredAt { get; set; }

//    public Guid? ShippingAddressId { get; set; }
//    public virtual Address? ShippingAddress { get; set; }
//    #endregion

//    #region Billing
//    public Guid? BillingAddressId { get; set; }
//    public virtual Address? BillingAddress { get; set; }
//    public bool BillingAddressSameAsShipping { get; set; } = true;
//    #endregion

//    #region Gift
//    public bool IsGift { get; set; }
//    public string? GiftMessage { get; set; }
//    public bool GiftWrap { get; set; }
//    #endregion

//    #region Notes
//    public string? CustomerNote { get; set; }
//    public string? AdminNote { get; set; }
//    public string? InternalNote { get; set; }
//    #endregion

//    #region Dates
//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? ConfirmedAt { get; set; }
//    public DateTime? ProcessingAt { get; set; }
//    public DateTime? CancelledAt { get; set; }
//    public string? CancelledReason { get; set; }
//    public DateTime? CompletedAt { get; set; }
//    public DateTime? ExpiresAt { get; set; }
//    #endregion

//    #region Source
//    public string Source { get; set; } = "Web";
//    public string? Referrer { get; set; }
//    public string? UtmSource { get; set; }
//    public string? UtmMedium { get; set; }
//    public string? UtmCampaign { get; set; }
//    #endregion

//    #region Stats
//    public int ItemCount { get; set; }
//    public int TotalQuantity { get; set; }
//    public decimal TotalWeight { get; set; }
//    #endregion

//    // Navigation
//    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
//    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
//    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

//    #region Domain Methods
//    public void AddItem(OrderItem item)
//    {
//        item.OrderId = this.Id;
//        Items.Add(item);
//        RecalculateTotals();
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

//    public void RecalculateTotals()
//    {
//        Subtotal = Items.Sum(i => i.TotalPrice);
//        TotalQuantity = Items.Sum(i => i.Quantity);
//        ItemCount = Items.Count;

//        var discount = DiscountAmount + (CouponDiscount ?? 0) + PointsValue;
//        TotalAmount = Subtotal + TaxAmount + ShippingCost + (GiftWrapAmount ?? 0) + (ServiceFee ?? 0) - discount;
//    }

//    public void Confirm()
//    {
//        Status = OrderStatus.Confirmed;
//        ConfirmedAt = DateTime.UtcNow;
//        AddStatusHistory(OrderStatus.Confirmed, "Order confirmed");
//    }

//    public void Cancel(string reason)
//    {
//        Status = OrderStatus.Cancelled;
//        CancelledAt = DateTime.UtcNow;
//        CancelledReason = reason;
//        AddStatusHistory(OrderStatus.Cancelled, reason);
//    }

//    public void Ship(string trackingNumber, string carrier)
//    {
//        TrackingNumber = trackingNumber;
//        CarrierName = carrier;
//        Status = OrderStatus.Shipped;
//        ShippedAt = DateTime.UtcNow;
//        AddStatusHistory(OrderStatus.Shipped, $"Shipped via {carrier}");
//    }

//    public void Deliver()
//    {
//        Status = OrderStatus.Delivered;
//        DeliveredAt = DateTime.UtcNow;
//        CompletedAt = DateTime.UtcNow;
//        AddStatusHistory(OrderStatus.Delivered, "Order delivered");
//    }

//    private void AddStatusHistory(OrderStatus status, string comment)
//    {
//        StatusHistory.Add(new OrderStatusHistory
//        {
//            OrderId = Id,
//            Status = status,
//            Comment = comment,
//            CreatedAt = DateTime.UtcNow
//        });
//    }
//    #endregion
//}

//// Domain/Entities/Orders/OrderItem.cs 

///// <summary>
///// Individual item in an order
///// </summary>
//public class OrderItem : Entity
//{
//    public Guid Id { get; set; }
//    public Guid OrderId { get; set; }
//    public virtual Order Order { get; set; } = null!;

//    public Guid ProductId { get; set; }
//    public virtual Product Product { get; set; } = null!;

//    public Guid? VariantId { get; set; }
//    public virtual ProductVariant? Variant { get; set; }

//    public string ProductName { get; set; } = string.Empty;
//    public string ProductSku { get; set; } = string.Empty;
//    public string? VariantName { get; set; } // e.g., "Red / Large"

//    public int Quantity { get; set; }
//    public decimal UnitPrice { get; set; } // Price at time of purchase
//    public decimal Discount { get; set; }
//    public decimal TotalPrice => (Quantity * UnitPrice) - Discount;

//    public decimal? TaxRate { get; set; }
//    public decimal TaxAmount { get; set; }

//    public decimal Weight { get; set; }

//    public bool IsGift { get; set; }
//    public string? GiftMessage { get; set; }

//    public string? ImageUrl { get; set; }

//    // For digital products
//    public string? DownloadUrl { get; set; }
//    public int? DownloadCount { get; set; }
//    public DateTime? DownloadExpiresAt { get; set; }

//    // For subscriptions
//    public bool IsSubscription { get; set; }
//    public string? SubscriptionInterval { get; set; }
//    public DateTime? NextBillingDate { get; set; }

//    // Status
//    public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;
//    public DateTime? ShippedAt { get; set; }
//    public DateTime? DeliveredAt { get; set; }
//    public DateTime? ReturnedAt { get; set; }
//    public string? ReturnReason { get; set; }
//}

//public enum OrderItemStatus
//{
//    Pending = 0,
//    Confirmed = 1,
//    Processing = 2,
//    Shipped = 3,
//    Delivered = 4,
//    Cancelled = 5,
//    Returned = 6,
//    Refunded = 7
//}

//// Domain/Entities/Orders/OrderStatusHistory.cs 

///// <summary>
///// Tracks order status changes over time
///// </summary>
//public class OrderStatusHistory : Entity
//{
//    public Guid Id { get; set; }
//    public Guid OrderId { get; set; }
//    public virtual Order Order { get; set; } = null!;

//    public OrderStatus Status { get; set; }
//    public string? Comment { get; set; }
//    public string? InternalComment { get; set; }

//    public Guid? ChangedBy { get; set; } // User ID
//    public string? ChangedByName { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//}

//// Domain/Entities/Orders/PaymentTransaction.cs 

///// <summary>
///// Payment transaction records
///// </summary>
//public class PaymentTransaction : Entity
//{
//    public Guid Id { get; set; }
//    public Guid OrderId { get; set; }
//    public virtual Order Order { get; set; } = null!;

//    public string TransactionId { get; set; } = string.Empty; // Payment gateway transaction ID
//    public string? ParentTransactionId { get; set; } // For refunds

//    public PaymentTransactionType Type { get; set; }
//    public PaymentStatus Status { get; set; }

//    public decimal Amount { get; set; }
//    public string Currency { get; set; } = "USD";

//    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, PayPal, etc.
//    public string? CardType { get; set; } // Visa, Mastercard
//    public string? CardLast4 { get; set; }
//    public string? CardExpMonth { get; set; }
//    public string? CardExpYear { get; set; }

//    public string? Gateway { get; set; } // Stripe, PayPal, etc.
//    public string? GatewayResponse { get; set; } // JSON

//    public string? ErrorCode { get; set; }
//    public string? ErrorMessage { get; set; }

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? ProcessedAt { get; set; }
//}

//public enum PaymentTransactionType
//{
//    Authorization = 0,
//    Capture = 1,
//    Sale = 2,
//    Refund = 3,
//    Void = 4,
//    Chargeback = 5
//}
