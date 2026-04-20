using Domain.Aggregates.Carts;
using Domain.Aggregates.Customers;
using Domain.Aggregates.Payments;
using Domain.Aggregates.Products;
using Domain.Aggregates.Shippings;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Orders;


/// <summary>
/// Order Aggregate Root
/// Manages the complete order lifecycle from creation to fulfillment
/// </summary>
public class Order : Entity
{
    #region Identity
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty; // e.g., "ORD-2024-001234"
    #endregion

    #region Customer
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
    public string? CustomerEmail { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    #endregion

    #region Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;
    #endregion

    #region Pricing
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal? GiftWrapAmount { get; private set; }
    public decimal? ServiceFee { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; set; } = "USD";

    // Loyalty
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public decimal PointsValue { get; set; }
    #endregion

    #region Discounts Applied
    public string? CouponCode { get; set; }
    public Guid? CouponId { get; set; }
    public decimal? CouponDiscount { get; set; }
    public string? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    #endregion

    #region Payment
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentReference { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    #endregion

    #region Shipping
    public ShippingMethod ShippingMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? CarrierName { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }

    public Guid? ShippingAddressId { get; set; }
    public virtual Address? ShippingAddress { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingInstructions { get; set; }
    #endregion

    #region Billing
    public Guid? BillingAddressId { get; set; }
    public virtual Address? BillingAddress { get; set; }
    public bool BillingAddressSameAsShipping { get; set; } = true;
    #endregion

    #region Gift
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }
    public bool GiftWrap { get; set; }
    #endregion

    #region Notes
    public string? CustomerNote { get; set; }
    public string? AdminNote { get; set; }
    public string? InternalNote { get; set; }
    #endregion

    #region Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ProcessingAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    #endregion

    #region Source Tracking
    public string Source { get; set; } = "Web";
    public string? Referrer { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    #endregion

    #region Order Stats
    public int ItemCount { get; private set; }
    public int TotalQuantity { get; private set; }
    public decimal TotalWeight { get; private set; }
    #endregion

    #region Navigation
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    #endregion

    #region Computed
    public bool IsPaid => PaymentStatus == PaymentStatus.Captured;
    public bool IsShipped => ShippedAt.HasValue;
    public bool IsDelivered => DeliveredAt.HasValue;
    public bool IsCompleted => Status == OrderStatus.Delivered || Status == OrderStatus.Completed;
    public bool IsCancelled => Status == OrderStatus.Cancelled || Status == OrderStatus.Refunded;
    public bool CanCancel => Status == OrderStatus.Pending ||
                             Status == OrderStatus.Confirmed ||
                             Status == OrderStatus.Processing;
    public bool CanModify => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
    public bool IsOverdue => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt && PaymentStatus != PaymentStatus.Captured;
    #endregion

    #region Domain Methods - Item Management

    public OrderItem AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        item.OrderId = Id;
        Items.Add(item);
        RecalculateTotals();

        AddStatusHistory(OrderStatus.Pending, $"Added item: {item.ProductName}");
        return item;
    }

    public void RemoveItem(Guid itemId, string reason)
    {
        if (!CanModify)
            throw new InvalidOperationException("Cannot remove items from this order");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotals();
            AddStatusHistory(Status, $"Removed item: {item.ProductName} - {reason}");
        }
    }

    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        if (!CanModify)
            throw new InvalidOperationException("Cannot modify this order");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            if (newQuantity <= 0)
            {
                Items.Remove(item);
                AddStatusHistory(Status, $"Removed item: {item.ProductName}");
            }
            else
            {
                item.Quantity = newQuantity;
                AddStatusHistory(Status, $"Updated quantity for {item.ProductName} to {newQuantity}");
            }
            RecalculateTotals();
        }
    }

    #endregion

    #region Domain Methods - Pricing

    public void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.TotalPrice);
        TotalQuantity = Items.Sum(i => i.Quantity);
        ItemCount = Items.Count;
        TotalWeight = Items.Sum(i => i.Weight * i.Quantity);

        var totalDiscount = DiscountAmount + (CouponDiscount ?? 0) + PointsValue;
        TotalAmount = Subtotal + TaxAmount + ShippingCost +
                      (GiftWrapAmount ?? 0) + (ServiceFee ?? 0) - totalDiscount;

        // Calculate loyalty points earned (e.g., 1 point per dollar)
        PointsEarned = (int)Math.Floor(TotalAmount);

        AddDomainEvent(new OrderTotalRecalculatedEvent(Id, TotalAmount));
    }

    public void SetShippingCost(decimal cost)
    {
        ShippingCost = cost;
        RecalculateTotals();
    }

    public void SetTax(decimal taxAmount)
    {
        TaxAmount = taxAmount;
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal amount, string reason)
    {
        DiscountAmount += amount;
        RecalculateTotals();
        AddStatusHistory(Status, $"Applied discount of {amount}: {reason}");
    }

    public void ApplyCoupon(string code, Guid couponId, decimal discount)
    {
        CouponCode = code;
        CouponId = couponId;
        CouponDiscount = discount;
        RecalculateTotals();
        AddStatusHistory(Status, $"Applied coupon {code} for {discount}");
    }

    #endregion

    #region Domain Methods - Order Lifecycle

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order must be pending to confirm");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount));
        AddStatusHistory(OrderStatus.Confirmed, "Order confirmed");
    }

    public void BeginProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Order must be confirmed to process");

        Status = OrderStatus.Processing;
        ProcessingAt = DateTime.UtcNow;
        AddDomainEvent(new OrderProcessingStartedEvent(Id));
        AddStatusHistory(OrderStatus.Processing, "Order is being processed");
    }

    public void Cancel(string reason, Guid? cancelledBy = null)
    {
        if (!CanCancel)
            throw new InvalidOperationException("Order cannot be cancelled in current status");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelledReason = reason;
        CancelledBy = cancelledBy;

        // Release reserved inventory
        foreach (var item in Items)
        {
            item.Status = OrderItemStatus.Cancelled;
        }

        // If paid, trigger refund logic
        if (IsPaid)
        {
            AddDomainEvent(new OrderCancelledWithPaymentEvent(Id, TotalAmount));
        }

        AddStatusHistory(OrderStatus.Cancelled, reason);
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidOperationException("Order must be delivered to complete");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AddStatusHistory(OrderStatus.Completed, "Order completed successfully");
        AddDomainEvent(new OrderCompletedEvent(Id, CustomerId));
    }

    public void MarkAsDelivered()
    {
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        CompletedAt = DateTime.UtcNow;

        foreach (var item in Items)
        {
            item.Status = OrderItemStatus.Delivered;
            item.DeliveredAt = DateTime.UtcNow;
        }

        AddStatusHistory(OrderStatus.Delivered, "Order delivered");
        AddDomainEvent(new OrderDeliveredEvent(Id, CustomerId));
    }

    public void MarkAsExpired()
    {
        if (Status == OrderStatus.Pending && IsOverdue)
        {
            Status = OrderStatus.Expired;
            AddStatusHistory(OrderStatus.Expired, "Order payment expired");
            AddDomainEvent(new OrderExpiredEvent(Id));
        }
    }

    #endregion

    #region Domain Methods - Payment

    public void ConfirmPayment(string transactionId, string gateway)
    {
        PaymentStatus = PaymentStatus.Captured;
        PaymentReference = transactionId;
        PaymentGateway = gateway;
        PaidAt = DateTime.UtcNow;

        AddStatusHistory(Status, $"Payment confirmed: {transactionId}");
        AddDomainEvent(new OrderPaymentConfirmedEvent(Id, TotalAmount, transactionId));
    }

    public void FailPayment(string errorCode, string errorMessage)
    {
        PaymentStatus = PaymentStatus.Failed;
        AddStatusHistory(Status, $"Payment failed: {errorCode} - {errorMessage}");
        AddDomainEvent(new OrderPaymentFailedEvent(Id, errorCode, errorMessage));
    }

    public void InitiateRefund(decimal amount, string reason)
    {
        if (!IsPaid)
            throw new InvalidOperationException("Cannot refund unpaid order");

        RefundAmount = amount;
        RefundedAt = DateTime.UtcNow;
        AddStatusHistory(Status, $"Refund initiated: {amount} - {reason}");
        AddDomainEvent(new OrderRefundInitiatedEvent(Id, amount, reason));
    }

    #endregion

    #region Domain Methods - Shipping

    public void UpdateShippingAddress(Address address)
    {
        if (!CanModify)
            throw new InvalidOperationException("Cannot modify shipping address");

        ShippingAddressId = address.Id;
        ShippingAddress = address;
        ShippingPhone = address.PhoneNumber;
        ShippingInstructions = address.DeliveryInstructions;

        AddStatusHistory(Status, "Shipping address updated");
    }

    public void UpdateShippingMethod(ShippingMethod method, decimal cost)
    {
        if (!CanModify)
            throw new InvalidOperationException("Cannot modify shipping method");

        ShippingMethod = method;
        ShippingCost = cost;
        RecalculateTotals();

        AddStatusHistory(Status, $"Shipping method changed to {method}");
    }

    public void Ship(string trackingNumber, string carrierName, string? trackingUrl = null)
    {
        if (Status != OrderStatus.Processing && Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Order must be processing or confirmed to ship");

        TrackingNumber = trackingNumber;
        CarrierName = carrierName;
        TrackingUrl = trackingUrl;
        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;

        foreach (var item in Items)
        {
            item.Status = OrderItemStatus.Shipped;
            item.ShippedAt = DateTime.UtcNow;
        }

        AddStatusHistory(OrderStatus.Shipped, $"Shipped via {carrierName}, tracking: {trackingNumber}");
        AddDomainEvent(new OrderShippedEvent(Id, trackingNumber, carrierName));
    }

    public void AddTrackingInfo(string trackingNumber, string carrierName)
    {
        TrackingNumber = trackingNumber;
        CarrierName = carrierName;
        TrackingUrl = GenerateTrackingUrl(carrierName, trackingNumber);
    }

    private string GenerateTrackingUrl(string carrier, string tracking)
    {
        // Generate carrier-specific tracking URL
        return carrier.ToLower() switch
        {
            "fedex" => $"https://www.fedex.com/fedextrack/?trknbr={tracking}",
            "ups" => $"https://www.ups.com/track?tracknum={tracking}",
            "usps" => $"https://tools.usps.com/go/TrackConfirmAction?tLabels={tracking}",
            "dhl" => $"https://www.dhl.com/en/express/tracking.html?AWB={tracking}",
            _ => $"https://track.example.com/{tracking}"
        };
    }

    #endregion

    #region Domain Methods - Notes

    public void AddCustomerNote(string note)
    {
        CustomerNote = note;
        AddStatusHistory(Status, $"Customer note added: {note}");
    }

    public void AddAdminNote(string note, Guid adminId, string adminName)
    {
        AdminNote = note;
        AddStatusHistory(Status, $"Admin ({adminName}) note: {note}");
    }

    public void AddInternalNote(string note)
    {
        InternalNote = note;
        AddStatusHistory(Status, $"Internal note: {note}");
    }

    #endregion

    #region Domain Methods - Fulfillment

    /// <summary>
    /// Mark specific items as ready for shipping (for partial fulfillment)
    /// </summary>
    public void MarkItemsReadyForShipping(List<Guid> itemIds)
    {
        foreach (var item in Items.Where(i => itemIds.Contains(i.Id)))
        {
            item.Status = OrderItemStatus.ReadyToShip;
        }
        AddStatusHistory(Status, "Items marked ready for shipping");
    }

    /// <summary>
    /// Mark specific items as shipped
    /// </summary>
    public void MarkItemsShipped(List<Guid> itemIds, DateTime shippedAt)
    {
        foreach (var item in Items.Where(i => itemIds.Contains(i.Id)))
        {
            item.Status = OrderItemStatus.Shipped;
            item.ShippedAt = shippedAt;
        }
        AddStatusHistory(Status, "Items shipped");
    }

    /// <summary>
    /// Handle return of items
    /// </summary>
    public void ReturnItems(List<Guid> itemIds, string reason)
    {
        foreach (var item in Items.Where(i => itemIds.Contains(i.Id)))
        {
            item.Status = OrderItemStatus.Returned;
            item.ReturnedAt = DateTime.UtcNow;
            item.ReturnReason = reason;
        }

        Status = OrderStatus.Returned;
        AddStatusHistory(Status, $"Items returned: {reason}");
        AddDomainEvent(new OrderItemsReturnedEvent(Id, itemIds, reason));
    }

    #endregion

    #region Private Methods

    private void AddStatusHistory(OrderStatus status, string? comment = null)
    {
        var history = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            Status = status,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };
        StatusHistory.Add(history);
    }

    #endregion

    #region Validation

    public List<string> ValidateForPayment()
    {
        var errors = new List<string>();

        if (!Items.Any())
            errors.Add("Order has no items");

        if (TotalAmount <= 0)
            errors.Add("Order total must be greater than zero");

        if (ShippingAddress == null && Items.Any(i => i.Product?.RequiresShipping == true))
            errors.Add("Shipping address is required");

        //if (PaymentMethod.Type == PaymentMethod.Type.CreditCard && string.IsNullOrEmpty(PaymentReference))
        //    errors.Add("Payment reference is required");

        return errors;
    }

    #endregion
}

/// <summary>
/// Individual item in an order
/// Part of Order aggregate
/// </summary>
public class OrderItem : Entity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    #region Product Reference (Snapshot at time of purchase)
    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }
    public Guid? VariantId { get; set; }
    public virtual ProductVariant? Variant { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? VariantName { get; set; } // e.g., "Red / Large"
    public string? ImageUrl { get; set; }
    #endregion

    #region Quantity & Pricing
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Price at time of purchase
    public decimal Discount { get; set; }
    public decimal TotalPrice => (Quantity * UnitPrice) - Discount;
    #endregion

    #region Tax
    public decimal? TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string TaxCategory { get; set; } = "default";
    #endregion

    #region Shipping
    public decimal Weight { get; set; }
    public decimal TotalWeight => Weight * Quantity;
    public bool RequiresShipping { get; set; } = true;
    #endregion

    #region Gift
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }
    #endregion

    #region Digital Product
    public string? DownloadUrl { get; set; }
    public int? DownloadCount { get; set; }
    public DateTime? DownloadExpiresAt { get; set; }
    public string? LicenseKey { get; set; }
    #endregion

    #region Subscription
    public bool IsSubscription { get; set; }
    public string? SubscriptionInterval { get; set; } // "monthly", "yearly"
    public DateTime? NextBillingDate { get; set; }
    public Guid? SubscriptionId { get; set; }
    #endregion

    #region Status
    public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? ReturnReason { get; set; }
    #endregion

    #region Domain Methods

    public bool CanReturn() => Status == OrderItemStatus.Delivered &&
                                Product?.IsReturnable == true &&
                                (Product?.ReturnDays == null ||
                                 DeliveredAt.HasValue &&
                                 DateTime.UtcNow <= DeliveredAt.Value.AddDays(Product.ReturnDays.Value));

    public void MarkAsDelivered()
    {
        Status = OrderItemStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkAsReturned(string reason)
    {
        Status = OrderItemStatus.Returned;
        ReturnedAt = DateTime.UtcNow;
        ReturnReason = reason;
    }

    public void IncrementDownloadCount()
    {
        if (DownloadCount.HasValue)
            DownloadCount++;
        else
            DownloadCount = 1;
    }

    #endregion
}

/// <summary>
/// Tracks order status changes over time
/// Part of Order aggregate
/// </summary>
public class OrderStatusHistory : Entity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public string? InternalComment { get; set; }

    public Guid? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public string? ChangedByType { get; set; } // Customer, Admin, System

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    #region Email Notification
    public bool CustomerNotified { get; set; }
    public DateTime? CustomerNotifiedAt { get; set; }
    #endregion
}

/// <summary>
/// Payment transaction records for an order
/// Part of Order aggregate (links to Payment aggregate)
/// </summary>
public class PaymentTransaction : Entity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    #region Transaction Identity
    public string TransactionId { get; set; } = string.Empty; // Payment gateway transaction ID
    public string? ParentTransactionId { get; set; } // For refunds (links to original transaction)
    public Guid? PaymentId { get; set; } // Reference to Payment aggregate
    #endregion

    #region Transaction Type & Status
    public PaymentTransactionType Type { get; set; }
    public PaymentStatus Status { get; set; }
    #endregion

    #region Amount
    public decimal Amount { get; set; }
    public decimal? RefundedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    #endregion

    #region Payment Method Details
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, PayPal, etc.
    public string? CardType { get; set; } // Visa, Mastercard
    public string? CardLast4 { get; set; }
    public string? CardExpMonth { get; set; }
    public string? CardExpYear { get; set; }
    public string? CardBrand { get; set; }
    #endregion

    #region Gateway Info
    public string? Gateway { get; set; } // Stripe, PayPal, Square, etc.
    public string? GatewayResponse { get; set; } // Raw JSON response
    public string? GatewayErrorCode { get; set; }
    public string? GatewayErrorMessage { get; set; }
    #endregion

    #region Authorization
    public string? AuthorizationCode { get; set; }
    public DateTime? AuthorizationExpiresAt { get; set; }
    #endregion

    #region Processing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    #endregion

    #region Domain Methods

    public bool IsSuccessful() => Status == PaymentStatus.Captured || Status == PaymentStatus.Authorized;

    public bool IsRefund() => Type == PaymentTransactionType.Refund;

    public bool IsPending() => Status == PaymentStatus.Pending;

    public void MarkAsCaptured(string? captureId = null)
    {
        Status = PaymentStatus.Captured;
        ProcessedAt = DateTime.UtcNow;
        if (captureId != null)
            TransactionId = captureId;
    }

    public void MarkAsFailed(string errorCode, string errorMessage)
    {
        Status = PaymentStatus.Failed;
        GatewayErrorCode = errorCode;
        GatewayErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
    }

    public void ProcessRefund(decimal amount, string? parentTransactionId)
    {
        Type = PaymentTransactionType.Refund;
        Amount = amount;
        ParentTransactionId = parentTransactionId;
        Status = PaymentStatus.Refunded;
        ProcessedAt = DateTime.UtcNow;
    }

    #endregion
}

// ============================================================================
// ENUMS - Order Aggregate
// ============================================================================

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Completed = 6,
    Cancelled = 7,
    Refunded = 8,
    Returned = 9,
    Expired = 10,
    Failed = 11
}

public enum OrderItemStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    ReadyToShip = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7,
    Refunded = 8
}

public enum OrderPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
 
 
public enum PaymentTransactionType
{
    Authorization = 0,
    Capture = 1,
    Sale = 2,
    Refund = 3,
    Void = 4,
    Chargeback = 5,
    ChargebackReversal = 6
}

// ============================================================================
// DOMAIN EVENTS - Order Aggregate
// ============================================================================

public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent;
public record OrderConfirmedEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent;
public record OrderProcessingStartedEvent(Guid OrderId) : IDomainEvent;
public record OrderShippedEvent(Guid OrderId, string TrackingNumber, string Carrier) : IDomainEvent;
public record OrderDeliveredEvent(Guid OrderId, Guid CustomerId) : IDomainEvent;
public record OrderCompletedEvent(Guid OrderId, Guid CustomerId) : IDomainEvent;
public record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent;
public record OrderCancelledWithPaymentEvent(Guid OrderId, decimal Amount) : IDomainEvent;
public record OrderExpiredEvent(Guid OrderId) : IDomainEvent;
public record OrderTotalRecalculatedEvent(Guid OrderId, decimal NewTotal) : IDomainEvent;
public record OrderPaymentConfirmedEvent(Guid OrderId, decimal Amount, string TransactionId) : IDomainEvent;
public record OrderPaymentFailedEvent(Guid OrderId, string ErrorCode, string ErrorMessage) : IDomainEvent;
public record OrderRefundInitiatedEvent(Guid OrderId, decimal Amount, string Reason) : IDomainEvent;
public record OrderItemsReturnedEvent(Guid OrderId, List<Guid> ItemIds, string Reason) : IDomainEvent;
