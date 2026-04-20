using Domain.Aggregates.Customers;
using Domain.Aggregates.Orders;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Domain.Aggregates.Payments;

/// <summary>
/// Payment Aggregate Root
/// Manages payment lifecycle, methods, and refunds
/// </summary>
public class Payment : Entity
{
    #region Identity
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty; // e.g., "PAY-2024-001234"
    #endregion

    #region Order Reference
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    public string OrderNumber { get; set; } = string.Empty;
    #endregion

    #region Customer
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
    #endregion

    #region Amount
    public decimal Amount { get; set; }
    public decimal AuthorizedAmount { get; set; }
    public decimal CapturedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal PendingRefundAmount { get; set; }
    public string Currency { get; set; } = "USD";

    public decimal RemainingAmount => Amount - CapturedAmount - RefundedAmount;
    public bool IsFullyCaptured => CapturedAmount >= Amount;
    public bool IsFullyRefunded => RefundedAmount >= Amount;
    #endregion

    #region Status
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentType Type { get; set; } = PaymentType.Charge;
    #endregion

    #region Payment Method
    public Guid? PaymentMethodId { get; set; }
    public virtual PaymentMethod? PaymentMethod { get; set; } 
    public string MethodName { get; set; } = string.Empty; // "Visa ****1234"
    #endregion

    #region Gateway
    public string Gateway { get; set; } = string.Empty; // "stripe", "paypal", "square"
    public string? GatewayPaymentId { get; set; } // Payment ID from gateway
    public string? GatewayCustomerId { get; set; } // Customer ID in gateway
    #endregion

    #region Authorization
    public string? AuthorizationCode { get; set; }
    public DateTime? AuthorizationExpiresAt { get; set; }
    #endregion

    #region Transactions
    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    #endregion

    #region 3D Secure
    public bool Requires3DSecure { get; set; }
    public bool Is3DSecureCompleted { get; set; }
    public string? ThreeDSecureStatus { get; set; }
    #endregion

    #region Risk & Fraud
    public string? RiskScore { get; set; }
    public bool IsFlaggedForReview { get; set; }
    public string? FraudCheckResult { get; set; }
    #endregion

    #region Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    #endregion

    #region Error Handling
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    #endregion

    #region Metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; } // JSON for additional data
    #endregion

    #region Domain Methods - Lifecycle

    public void Authorize(string authorizationCode, DateTime? expiresAt = null)
    {
        Status = PaymentStatus.Authorized;
        AuthorizationCode = authorizationCode;
        AuthorizationExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7);
        ProcessedAt = DateTime.UtcNow;

        AddTransaction(PaymentTransactionType.Authorization, Amount, PaymentStatus.Authorized,
            authorizationCode);

        AddDomainEvent(new PaymentAuthorizedEvent(Id, OrderId, Amount, authorizationCode));
    }

    public void Capture(decimal? amount = null)
    {
        var captureAmount = amount ?? AuthorizedAmount;

        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException("Payment must be authorized to capture");

        if (captureAmount > AuthorizedAmount)
            throw new InvalidOperationException("Cannot capture more than authorized amount");

        CapturedAmount += captureAmount;

        if (CapturedAmount >= Amount)
        {
            Status = PaymentStatus.Captured;
            CapturedAt = DateTime.UtcNow;
        }
        else
        {
            Status = PaymentStatus.PartiallyCaptured;
        }

        AddTransaction(PaymentTransactionType.Capture, captureAmount, PaymentStatus.Captured);
        AddDomainEvent(new PaymentCapturedEvent(Id, OrderId, captureAmount));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == PaymentStatus.Captured || Status == PaymentStatus.PartiallyCaptured)
            throw new InvalidOperationException("Cannot cancel captured payment. Use refund instead.");

        Status = PaymentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        AddTransaction(PaymentTransactionType.Void, 0, PaymentStatus.Cancelled);
        AddDomainEvent(new PaymentCancelledEvent(Id, OrderId, reason));
    }

    public void Fail(string errorCode, string errorMessage)
    {
        Status = PaymentStatus.Failed;
        FailedAt = DateTime.UtcNow;
        LastErrorCode = errorCode;
        LastErrorMessage = errorMessage;

        AddTransaction(PaymentTransactionType.Sale, 0, PaymentStatus.Failed, errorCode, errorMessage);
        AddDomainEvent(new PaymentFailedEvent(Id, OrderId, errorCode, errorMessage));
    }

    public void Retry()
    {
        if (Status != PaymentStatus.Failed)
            throw new InvalidOperationException("Can only retry failed payments");

        if (RetryCount >= 3)
            throw new InvalidOperationException("Maximum retry attempts exceeded");

        RetryCount++;
        NextRetryAt = DateTime.UtcNow.AddHours(Math.Pow(2, RetryCount)); // Exponential backoff
        Status = PaymentStatus.Pending;

        AddDomainEvent(new PaymentRetryScheduledEvent(Id, RetryCount, NextRetryAt.Value));
    }

    #endregion

    #region Domain Methods - Refunds

    public Refund InitiateRefund(decimal amount, string reason, RefundType type = RefundType.Full)
    {
        if (!IsFullyCaptured && CapturedAmount < amount)
            throw new InvalidOperationException("Cannot refund more than captured amount");

        if (RefundedAmount + amount > CapturedAmount)
            throw new InvalidOperationException("Refund amount exceeds available captured amount");

        var refund = new Refund
        {
            Id = Guid.NewGuid(),
            PaymentId = Id,
            Amount = amount,
            Reason = reason,
            Type = type,
            Status = RefundStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        Refunds.Add(refund);
        PendingRefundAmount += amount;

        AddDomainEvent(new RefundInitiatedEvent(refund.Id, Id, OrderId, amount, reason));
        return refund;
    }

    public void CompleteRefund(Guid refundId, string? gatewayRefundId = null)
    {
        var refund = Refunds.FirstOrDefault(r => r.Id == refundId);
        if (refund == null)
            throw new InvalidOperationException("Refund not found");

        refund.MarkAsCompleted(gatewayRefundId);
        RefundedAmount += refund.Amount;
        PendingRefundAmount -= refund.Amount;

        if (RefundedAmount >= Amount)
        {
            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }

        AddTransaction(PaymentTransactionType.Refund, refund.Amount, PaymentStatus.Refunded);
        AddDomainEvent(new RefundCompletedEvent(refundId, Id, OrderId, refund.Amount));
    }

    public void FailRefund(Guid refundId, string errorCode, string errorMessage)
    {
        var refund = Refunds.FirstOrDefault(r => r.Id == refundId);
        if (refund == null)
            throw new InvalidOperationException("Refund not found");

        refund.MarkAsFailed(errorCode, errorMessage);
        PendingRefundAmount -= refund.Amount;
    }

    #endregion

    #region Domain Methods - 3D Secure

    public void Set3DSecureRequired()
    {
        Requires3DSecure = true;
    }

    public void Complete3DSecure(string status)
    {
        Is3DSecureCompleted = true;
        ThreeDSecureStatus = status;
    }

    #endregion

    #region Domain Methods - Fraud

    public void FlagForReview(string reason)
    {
        IsFlaggedForReview = true;
        FraudCheckResult = reason;
        AddDomainEvent(new PaymentFlaggedForReviewEvent(Id, OrderId, reason));
    }

    public void ClearFraudFlag()
    {
        IsFlaggedForReview = false;
    }

    #endregion

    #region Private Methods

    private void AddTransaction(PaymentTransactionType type, decimal amount, PaymentStatus status,
        string? reference = null, string? errorMessage = null)
    {
        Transactions.Add(new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentId = Id,
            Type = type,
            Amount = amount,
            Status = status,
            TransactionId = reference ?? Guid.NewGuid().ToString(),
            Gateway = Gateway,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            GatewayErrorCode = errorMessage
        });
    }

    #endregion

    #region Navigation
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    #endregion
}

/// <summary>
/// Stored payment method for a customer
/// Part of Payment aggregate
/// </summary>
public class PaymentMethod : Entity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    #region Type
    public PaymentMethodType Type { get; set; }
    public string DisplayName { get; set; } = string.Empty; // "Visa ****1234"
    #endregion

    #region Card Details (for credit/debit cards)
    public string? CardBrand { get; set; } // Visa, Mastercard, Amex
    public string? CardLast4 { get; set; }
    public string? CardExpMonth { get; set; }
    public string? CardExpYear { get; set; }
    public string? CardFunding { get; set; } // Credit, Debit, Prepaid
    public string? CardCountry { get; set; }
    public bool Is3DSecureEnabled { get; set; }
    #endregion

    #region Billing Address
    public Guid? BillingAddressId { get; set; }
    public virtual Address? BillingAddress { get; set; }
    #endregion

    #region Gateway Token
    public string Gateway { get; set; } = string.Empty; // "stripe", "paypal"
    public string? GatewayToken { get; set; } // Tokenized payment method ID
    public string? GatewayCustomerId { get; set; } // Customer ID in gateway
    #endregion
    #region Status
    public bool IsDefault { get; set; }
    public bool IsExpired => !string.IsNullOrWhiteSpace(CardExpYear) && !string.IsNullOrWhiteSpace(CardExpMonth) &&
                              new DateTime(int.Parse(CardExpYear), int.Parse(CardExpMonth), 1) < DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    #endregion

    #region Verification
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationStatus { get; set; }
    #endregion

    #region Metadata
    public string? Nickname { get; set; } // e.g., "Personal Visa", "Business Card"
    public string? Metadata { get; set; } // JSON for additional data
    #endregion

    #region Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public int TimesUsed { get; set; }
    #endregion

    #region Computed
    public string ExpiryDisplay => $"{CardExpMonth}/{CardExpYear}";
    public bool IsExpiringSoon => !string.IsNullOrWhiteSpace(CardExpYear) && !string.IsNullOrWhiteSpace(CardExpMonth) &&
                                   new DateTime(int.Parse(CardExpYear), int.Parse(CardExpMonth), 1)
                                       .AddMonths(-2) < DateTime.UtcNow;

    #endregion

    #region Domain Methods

    public void SetAsDefault()
    {
        if (Customer != null)
        {
            foreach (var method in Customer.PaymentMethods)
                method.IsDefault = false;
        }
        IsDefault = true;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
        TimesUsed++;
    }

    public void Deactivate(string? reason = null)
    {
        IsActive = false;
    }

    public void UpdateExpiry(string month, string year)
    {
        CardExpMonth = month;
        CardExpYear = year;
    }

    #endregion
}

/// <summary>
/// Refund record
/// Part of Payment aggregate
/// </summary>
public class Refund : Entity
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public virtual Payment Payment { get; set; } = null!;

    #region Amount & Type
    public decimal Amount { get; set; }
    public RefundType Type { get; set; } = RefundType.Full;
    #endregion

    #region Status
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    #endregion

    #region Reason
    public string Reason { get; set; } = string.Empty;
    public string? ReasonDetail { get; set; }
    public RefundReasonCategory Category { get; set; }
    #endregion

    #region Order Reference
    public Guid? OrderId { get; set; }
    public Guid? OrderItemId { get; set; }
    #endregion

    #region Processing
    public string? GatewayRefundId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    #endregion

    #region Initiator
    public Guid? InitiatedBy { get; set; } // Customer or Admin ID
    public string? InitiatedByType { get; set; } // "Customer", "Admin", "System"
    #endregion

    #region Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    #endregion

    #region Domain Methods

    public void MarkAsCompleted(string? gatewayRefundId = null)
    {
        Status = RefundStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ProcessedAt = DateTime.UtcNow;
        GatewayRefundId = gatewayRefundId;
    }

    public void MarkAsFailed(string errorCode, string errorMessage)
    {
        Status = RefundStatus.Failed;
        FailureReason = $"{errorCode}: {errorMessage}";
        ProcessedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        Status = RefundStatus.Cancelled;
    }

    #endregion
}

// ============================================================================
// ENUMS - Payment Aggregate
// ============================================================================

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    PartiallyCaptured = 3,
    Failed = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    Cancelled = 7,
    Voided = 8,
    Expired = 9
}

public enum PaymentType
{
    Charge = 0,
    Authorization = 1,
    Recurring = 2
}

public enum PaymentMethodType
{
    CreditCard = 0,
    DebitCard = 1,
    PayPal = 2,
    ApplePay = 3,
    GooglePay = 4,
    BankTransfer = 5,
    Cash = 6,
    GiftCard = 7,
    StoreCredit = 8,
    Crypto = 9,
    BuyNowPayLater = 10
}

public enum RefundStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

public enum RefundType
{
    Full = 0,
    Partial = 1
}

public enum RefundReasonCategory
{
    CustomerRequest = 0,
    Damaged = 1,
    WrongItem = 2,
    NotAsDescribed = 3,
    LateDelivery = 4,
    OrderCancelled = 5,
    DuplicateOrder = 6,
    FraudPrevention = 7,
    Other = 8
}

// ============================================================================
// DOMAIN EVENTS - Payment Aggregate
// ============================================================================

public record PaymentAuthorizedEvent(Guid PaymentId, Guid OrderId, decimal Amount, string AuthCode) : IDomainEvent;
public record PaymentCapturedEvent(Guid PaymentId, Guid OrderId, decimal Amount) : IDomainEvent;
public record PaymentCancelledEvent(Guid PaymentId, Guid OrderId, string? Reason) : IDomainEvent;
public record PaymentFailedEvent(Guid PaymentId, Guid OrderId, string ErrorCode, string ErrorMessage) : IDomainEvent;
public record PaymentRetryScheduledEvent(Guid PaymentId, int Attempt, DateTime NextRetryAt) : IDomainEvent;
public record RefundInitiatedEvent(Guid RefundId, Guid PaymentId, Guid OrderId, decimal Amount, string Reason) : IDomainEvent;
public record RefundCompletedEvent(Guid RefundId, Guid PaymentId, Guid OrderId, decimal Amount) : IDomainEvent;
public record RefundFailedEvent(Guid RefundId, string ErrorCode, string ErrorMessage) : IDomainEvent;
public record PaymentFlaggedForReviewEvent(Guid PaymentId, Guid OrderId, string Reason) : IDomainEvent;
