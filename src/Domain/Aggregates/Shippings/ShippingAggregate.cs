using Domain.Aggregates.Carts;
using Domain.Aggregates.Customers;
using Domain.Aggregates.Orders;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Shippings;



/// <summary>
/// Shipment Aggregate Root
/// Manages shipping lifecycle, tracking, and delivery confirmation
/// </summary>
public class Shipment : Entity
{
    #region Identity
    public Guid Id { get; set; }
    public string ShipmentNumber { get; set; } = string.Empty; // e.g., "SHP-2024-001234"
    #endregion

    #region Order Reference
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    public string OrderNumber { get; set; } = string.Empty;
    #endregion

    #region Status
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public ShipmentType Type { get; set; } = ShipmentType.Delivery;
    #endregion

    #region Shipping Method
    public ShippingMethod ShippingMethod { get; set; }
    public string ShippingMethodName { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public string Currency { get; set; } = "USD";
    #endregion

    #region Carrier
    public Guid? CarrierId { get; set; }
    public virtual Carrier? Carrier { get; set; }
    public string? CarrierName { get; set; }
    public string? CarrierCode { get; set; } // e.g., "fedex", "ups", "usps"
    #endregion

    #region Tracking
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ReturnTrackingNumber { get; set; }
    #endregion

    #region Package Info
    public int PackageCount { get; set; } = 1;
    public decimal TotalWeight { get; set; }
    public string? WeightUnit { get; set; } = "kg";
    public string? Dimensions { get; set; } // JSON: { "length": 10, "width": 10, "height": 10 }
    #endregion

    #region Origin (Warehouse/Fulfillment Center)
    public Guid? WarehouseId { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
    public string? WarehouseName { get; set; }
    #endregion

    #region Destination
    public Guid AddressId { get; set; }
    public virtual Address Address { get; set; } = null!;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    #endregion

    #region Dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LabelPrintedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? InTransitAt { get; set; }
    public DateTime? OutForDeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    #endregion

    #region Delivery Confirmation
    public bool RequiresSignature { get; set; }
    public string? SignatureUrl { get; set; }
    public string? ProofOfDeliveryUrl { get; set; }
    public string? ReceivedBy { get; set; } // Name of person who received
    #endregion

    #region Items in Shipment
    public virtual ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    #endregion

    #region Events/History
    public virtual ICollection<ShipmentEvent> Events { get; set; } = new List<ShipmentEvent>();
    #endregion

    #region Notes
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public string? CancellationReason { get; set; }
    #endregion

    #region Refunds
    public bool IsReturnShipment { get; set; }
    public Guid? ReturnReasonId { get; set; }
    public string? ReturnReason { get; set; }
    public decimal? RefundAmount { get; set; }
    #endregion

    #region Computed
    public bool IsDelivered => Status == ShipmentStatus.Delivered;
    public bool IsInTransit => Status == ShipmentStatus.InTransit ||
                              Status == ShipmentStatus.OutForDelivery;
    public bool IsDelayed => EstimatedDeliveryDate.HasValue &&
                              DateTime.UtcNow > EstimatedDeliveryDate &&
                              Status != ShipmentStatus.Delivered;
    public int TotalItems => Items.Sum(i => i.Quantity);
    public int DeliveredItems => Items.Where(i => i.IsDelivered).Sum(i => i.Quantity);
    public int RemainingItems => TotalItems - DeliveredItems;
    #endregion

    #region Domain Methods - Lifecycle

    public void CreateLabel(string trackingNumber, string carrierName, string? trackingUrl = null)
    {
        TrackingNumber = trackingNumber;
        CarrierName = carrierName;
        TrackingUrl = trackingUrl ?? GenerateTrackingUrl(carrierName, trackingNumber);
        LabelPrintedAt = DateTime.UtcNow;

        AddEvent(ShipmentEventType.LabelCreated, "Shipping label created");
        AddDomainEvent(new ShipmentLabelCreatedEvent(Id, OrderId, trackingNumber));
    }

    public void MarkAsPickedUp()
    {
        Status = ShipmentStatus.PickedUp;
        PickedUpAt = DateTime.UtcNow;
        AddEvent(ShipmentEventType.PickedUp, "Package picked up by carrier");
    }

    public void Ship()
    {
        if (Status != ShipmentStatus.PickedUp && Status != ShipmentStatus.Ready)
            throw new InvalidOperationException("Shipment must be picked up or ready to ship");

        Status = ShipmentStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        AddEvent(ShipmentEventType.Shipped, "Shipment dispatched");
        AddDomainEvent(new ShipmentShippedEvent(Id, OrderId, TrackingNumber, CarrierName));
    }

    public void MarkInTransit()
    {
        Status = ShipmentStatus.InTransit;
        InTransitAt = DateTime.UtcNow;
        AddEvent(ShipmentEventType.InTransit, "Package in transit");
    }

    public void MarkOutForDelivery()
    {
        Status = ShipmentStatus.OutForDelivery;
        OutForDeliveryAt = DateTime.UtcNow;
        AddEvent(ShipmentEventType.OutForDelivery, "Out for delivery");
        AddDomainEvent(new ShipmentOutForDeliveryEvent(Id, OrderId));
    }

    public void MarkAsDelivered(string? receivedBy = null, string? signatureUrl = null)
    {
        Status = ShipmentStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        ActualDeliveryDate = DateTime.UtcNow;
        ReceivedBy = receivedBy;
        SignatureUrl = signatureUrl;

        foreach (var item in Items)
        {
            item.MarkAsDelivered();
        }

        AddEvent(ShipmentEventType.Delivered, $"Delivered to {receivedBy ?? "recipient"}");
        AddDomainEvent(new ShipmentDeliveredEvent(Id, OrderId, DeliveredAt.Value));
    }

    public void MarkAsDeliveryFailed(string reason)
    {
        Status = ShipmentStatus.DeliveryFailed;
        InternalNote = $"Delivery failed: {reason}";
        AddEvent(ShipmentEventType.DeliveryFailed, reason);
        AddDomainEvent(new ShipmentDeliveryFailedEvent(Id, OrderId, reason));
    }

    public void AttemptDelivery(string attemptDetails)
    {
        AddEvent(ShipmentEventType.DeliveryAttempted, attemptDetails);
    }

    public void Cancel(string reason)
    {
        if (Status == ShipmentStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered shipment");

        Status = ShipmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        AddEvent(ShipmentEventType.Cancelled, reason);
        AddDomainEvent(new ShipmentCancelledEvent(Id, OrderId, reason));
    }

    public void ReturnToSender(string reason)
    {
        Status = ShipmentStatus.ReturnedToSender;
        ReturnedAt = DateTime.UtcNow;
        AddEvent(ShipmentEventType.ReturnedToSender, reason);
    }

    #endregion

    #region Domain Methods - Tracking

    public void UpdateTrackingInfo(string trackingNumber, string? carrierName = null)
    {
        TrackingNumber = trackingNumber;
        if (carrierName != null)
            CarrierName = carrierName;
        TrackingUrl = GenerateTrackingUrl(CarrierName ?? carrierName ?? "", trackingNumber);
    }

    public void UpdateEstimatedDelivery(DateTime estimatedDate)
    {
        EstimatedDeliveryDate = estimatedDate;
        AddEvent(ShipmentEventType.EstimatedDeliveryUpdated,
            $"Estimated delivery updated to {estimatedDate:yyyy-MM-dd}");
    }

    public void AddTrackingEvent(ShipmentEvent shipmentEvent)
    {
        Events.Add(shipmentEvent);

        // Update status based on event
        switch (shipmentEvent.Type)
        {
            case ShipmentEventType.InTransit:
                if (Status == ShipmentStatus.Shipped)
                    MarkInTransit();
                break;
            case ShipmentEventType.OutForDelivery:
                if (Status == ShipmentStatus.InTransit)
                    MarkOutForDelivery();
                break;
            case ShipmentEventType.Delivered:
                if (Status == ShipmentStatus.OutForDelivery)
                    MarkAsDelivered(shipmentEvent.Location);
                break;
            case ShipmentEventType.DeliveryFailed:
                MarkAsDeliveryFailed(shipmentEvent.Description);
                break;
            case ShipmentEventType.ReturnedToSender:
                ReturnToSender(shipmentEvent.Description);
                break;
        }
    }

    public void SyncWithCarrier(TrackingInfo trackingInfo)
    {
        // Update from carrier webhook data
        EstimatedDeliveryDate = trackingInfo.EstimatedDelivery;

        foreach (var eventData in trackingInfo.Events)
        {
            if (!Events.Any(e => e.CarrierEventId == eventData.EventId))
            {
                AddTrackingEvent(new ShipmentEvent
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = Id,
                    Type = MapCarrierEventType(eventData.Status),
                    Description = eventData.Description,
                    Location = eventData.Location,
                    OccurredAt = eventData.OccurredAt,
                    CarrierEventId = eventData.EventId,
                    RawData = eventData.RawData,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private ShipmentEventType MapCarrierEventType(string carrierStatus)
    {
        return carrierStatus.ToLower() switch
        {
            "in_transit" or "transit" => ShipmentEventType.InTransit,
            "out_for_delivery" or "out for delivery" => ShipmentEventType.OutForDelivery,
            "delivered" => ShipmentEventType.Delivered,
            "delivery_attempted" or "delivery_failed" => ShipmentEventType.DeliveryFailed,
            "returned" or "return_to_sender" => ShipmentEventType.ReturnedToSender,
            _ => ShipmentEventType.LocationUpdated
        };
    }

    private string GenerateTrackingUrl(string carrier, string tracking)
    {
        if (string.IsNullOrEmpty(carrier) || string.IsNullOrEmpty(tracking))
            return string.Empty;

        return carrier.ToLower() switch
        {
            "fedex" => $"https://www.fedex.com/fedextrack/?trknbr={tracking}",
            "ups" => $"https://www.ups.com/track?tracknum={tracking}",
            "usps" => $"https://tools.usps.com/go/TrackConfirmAction?tLabels={tracking}",
            "dhl" => $"https://www.dhl.com/en/express/tracking.html?AWB={tracking}",
            "royalmail" => $"https://www.royalmail.com/track-your-item?trackingNumber={tracking}",
            "australia_post" => $"https://auspost.com.au/mypost/track/details/{tracking}",
            _ => $"https://track.example.com/{carrier}/{tracking}"
        };
    }

    #endregion

    #region Domain Methods - Items

    public void AddItem(ShipmentItem item)
    {
        item.ShipmentId = Id;
        Items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
            Items.Remove(item);
    }

    #endregion

    #region Private Methods

    private void AddEvent(ShipmentEventType type, string description, string? location = null)
    {
        Events.Add(new ShipmentEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = Id,
            Type = type,
            Description = description,
            Location = location,
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
    }

    #endregion
}

/// <summary>
/// Individual item in a shipment
/// Part of Shipping aggregate
/// </summary>
public class ShipmentItem : Entity
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public virtual Shipment Shipment { get; set; } = null!;

    #region Order Item Reference
    public Guid OrderItemId { get; set; }
    public virtual OrderItem? OrderItem { get; set; }
    #endregion

    #region Product Info (Snapshot)
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Guid? VariantId { get; set; }
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    #endregion

    #region Quantity
    public int Quantity { get; set; }
    public int DeliveredQuantity { get; set; }
    public int RemainingQuantity => Quantity - DeliveredQuantity;
    #endregion

    #region Status
    public ShipmentItemStatus Status { get; set; } = ShipmentItemStatus.Pending;
    public DateTime? ShippedAt { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    #endregion

    #region Domain Methods

    public void MarkAsShipped()
    {
        Status = ShipmentItemStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = ShipmentItemStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        DeliveredQuantity = Quantity;
    }

    public void MarkAsDeliveryFailed(string reason)
    {
        Status = ShipmentItemStatus.DeliveryFailed;
    }

    public void MarkAsReturned(string reason)
    {
        Status = ShipmentItemStatus.Returned;
    }

    #endregion
}

/// <summary>
/// Tracking event for a shipment
/// Part of Shipping aggregate
/// </summary>
public class ShipmentEvent : Entity
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public virtual Shipment Shipment { get; set; } = null!;

    public ShipmentEventType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    public DateTime OccurredAt { get; set; }
    public string? CarrierEventId { get; set; } // External carrier event ID
    public string? RawData { get; set; } // Raw JSON from carrier API

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; } // "carrier_api", "webhook", "manual"

    #region Computed
    public string FormattedLocation => string.Join(", ",
        new[] { City, State, PostalCode, Country }.Where(s => !string.IsNullOrEmpty(s)));
    public string TimeAgo => GetTimeAgo();
    #endregion

    private string GetTimeAgo()
    {
        var span = DateTime.UtcNow - OccurredAt;
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} minutes ago";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} hours ago";
        if (span.TotalDays < 30)
            return $"{(int)span.TotalDays} days ago";
        return OccurredAt.ToString("MMM dd, yyyy");
    }
}

/// <summary>
/// Shipping carrier configuration
/// Part of Shipping aggregate (Reference entity)
/// </summary>
public class Carrier : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // e.g., "fedex", "ups"
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public bool SupportsTracking { get; set; } = true;
    public bool SupportsLabelPrinting { get; set; } = true;
    public bool SupportsReturnLabels { get; set; }

    #region API Configuration
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? AccountNumber { get; set; }
    #endregion

    #region Service Types
    public string AvailableServices { get; set; } = string.Empty; // JSON array
    public string? DefaultService { get; set; }
    #endregion

    #region Tracking
    public string? TrackingUrlTemplate { get; set; } // e.g., "https://track.example.com/{tracking}"
    public int TrackingPollingIntervalMinutes { get; set; } = 60;
    #endregion

    #region Rates
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
    #endregion

    #region Domain Methods

    public string GenerateTrackingUrl(string trackingNumber)
    {
        if (string.IsNullOrEmpty(TrackingUrlTemplate))
            return string.Empty;

        return TrackingUrlTemplate.Replace("{tracking}", trackingNumber);
    }

    #endregion
}

/// <summary>
/// Warehouse/Fulfillment center
/// Part of Shipping aggregate (Reference entity)
/// </summary>
public class Warehouse : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // e.g., "WH-NYC-01"

    #region Address
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    #endregion

    #region Contact
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    #endregion

    #region Configuration
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int ProcessingTimeHours { get; set; } = 24; // Time to prepare shipment
    public int MaxDailyOrders { get; set; }
    public int CurrentDailyOrders { get; set; }
    #endregion

    #region Operating Hours
    public string? OperatingHours { get; set; } // JSON: { "monday": "9:00-18:00", ... }
    public string? TimeZone { get; set; }
    #endregion

    #region Shipping Zones
    public string SupportedCountries { get; set; } = string.Empty; // JSON array of country codes
    #endregion

    #region Computed
    public string FullAddress => $"{AddressLine1}, {AddressLine2}, {City}, {State} {PostalCode}, {Country}";
    public bool IsOpenNow => IsWithinOperatingHours(DateTime.UtcNow);
    #endregion

    #region Domain Methods

    public bool IsWithinOperatingHours(DateTime utcNow)
    {
        // Simplified - would need proper timezone handling
        return true; // Placeholder
    }

    public bool SupportsCountry(string countryCode)
    {
        if (string.IsNullOrEmpty(SupportedCountries))
            return true; // Supports all if not specified
        return SupportedCountries.Contains(countryCode);
    }

    #endregion
}

/// <summary>
/// Tracking information from carrier API
/// Value object for carrier tracking data
/// </summary>
public class TrackingInfo
{
    public string TrackingNumber { get; init; } = string.Empty;
    public string CarrierCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? EstimatedDelivery { get; init; }
    public DateTime? ActualDelivery { get; init; }
    public List<TrackingEventInfo> Events { get; init; } = new();
    public string? SignedBy { get; init; }
    public string? ProofOfDeliveryUrl { get; init; }
}

public class TrackingEventInfo
{
    public string EventId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Location { get; init; }
    public DateTime OccurredAt { get; init; }
    public string? RawData { get; init; }
}

// ============================================================================
// ENUMS - Shipping Aggregate
// ============================================================================


public enum ShipmentStatus
{
    Pending = 0,
    Ready = 1,
    LabelCreated = 2,
    PickedUp = 3,
    Shipped = 4,
    InTransit = 5,
    OutForDelivery = 6,
    Delivered = 7,
    DeliveryFailed = 8,
    ReturnedToSender = 9,
    Cancelled = 10
}

public enum ShipmentType
{
    Delivery = 0,
    Return = 1,
    Exchange = 2
}

public enum ShipmentItemStatus
{
    Pending = 0,
    Shipped = 1,
    Delivered = 2,
    DeliveryFailed = 3,
    Returned = 4
}

public enum ShipmentEventType
{
    Created = 0,
    LabelCreated = 1,
    Ready = 2,
    PickedUp = 3,
    Shipped = 4,
    InTransit = 5,
    LocationUpdated = 6,
    OutForDelivery = 7,
    DeliveryAttempted = 8,
    Delivered = 9,
    DeliveryFailed = 10,
    ReturnedToSender = 11,
    Cancelled = 12,
    EstimatedDeliveryUpdated = 13
}

// ============================================================================
// DOMAIN EVENTS - Shipping Aggregate
// ============================================================================


public record ShipmentCreatedEvent(Guid ShipmentId, Guid OrderId) : IDomainEvent;
public record ShipmentLabelCreatedEvent(Guid ShipmentId, Guid OrderId, string TrackingNumber) : IDomainEvent;
public record ShipmentShippedEvent(Guid ShipmentId, Guid OrderId, string TrackingNumber, string Carrier) : IDomainEvent;
public record ShipmentOutForDeliveryEvent(Guid ShipmentId, Guid OrderId) : IDomainEvent;
public record ShipmentDeliveredEvent(Guid ShipmentId, Guid OrderId, DateTime DeliveredAt) : IDomainEvent;
public record ShipmentDeliveryFailedEvent(Guid ShipmentId, Guid OrderId, string Reason) : IDomainEvent;
public record ShipmentCancelledEvent(Guid ShipmentId, Guid OrderId, string Reason) : IDomainEvent;
public record ShipmentTrackingUpdatedEvent(Guid ShipmentId, string Status, string Location) : IDomainEvent;
