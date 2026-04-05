using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Products; 

//public enum ProductStatus
//{
//    Draft = 0,
//    PendingApproval = 1,
//    Active = 2,
//    Inactive = 3,
//    Archived = 4,
//    Discontinued = 5
//}

//public enum ProductType
//{
//    Physical = 0,
//    Digital = 1,
//    Service = 2,
//    Subscription = 3
//}

//public enum OrderStatus
//{
//    Pending = 0,
//    Confirmed = 1,
//    Processing = 2,
//    Shipped = 3,
//    OutForDelivery = 4,
//    Delivered = 5,
//    Cancelled = 6,
//    Refunded = 7,
//    Returned = 8
//}

//public enum PaymentMethod
//{
//    CreditCard = 0,
//    DebitCard = 1,
//    PayPal = 2,
//    ApplePay = 3,
//    GooglePay = 4,
//    CashOnDelivery = 5,
//    BankTransfer = 6,
//    GiftCard = 7,
//    StoreCredit = 8
//}

//public enum PaymentStatus
//{
//    Pending = 0,
//    Authorized = 1,
//    Captured = 2,
//    Failed = 3,
//    Refunded = 4,
//    PartiallyRefunded = 5
//}

//public enum ShippingMethod
//{
//    Standard = 0,
//    Express = 1,
//    NextDay = 2,
//    International = 3,
//    StorePickup = 4,
//    DigitalDelivery = 5
//}

//public enum WeightUnit
//{
//    Gram = 0,
//    Kilogram = 1,
//    Pound = 2,
//    Ounce = 3
//}

//public enum DimensionUnit
//{
//    Centimeter = 0,
//    Meter = 1,
//    Inch = 2,
//    Foot = 3
//}

//public enum ReviewStatus
//{
//    Pending = 0,
//    Approved = 1,
//    Rejected = 2,
//    Flagged = 3,
//    Spam = 4
//}

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
//    Banned = 3
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
//    Both = 2
//}

//// Domain/ValueObjects/Money.cs
//namespace YourProject.Domain.ValueObjects;

//public record Money
//{
//    public decimal Amount { get; init; }
//    public string Currency { get; init; } = "USD";

//    private Money() { }

//    public Money(decimal amount, string currency = "USD")
//    {
//        if (amount < 0)
//            throw new ArgumentException("Amount cannot be negative", nameof(amount));
//        Amount = amount;
//        Currency = currency;
//    }

//    public static Money Zero => new(0);

//    public Money Add(Money other)
//    {
//        if (Currency != other.Currency)
//            throw new InvalidOperationException("Cannot add money with different currencies");
//        return new Money(Amount + other.Amount, Currency);
//    }

//    public Money Subtract(Money other)
//    {
//        if (Currency != other.Currency)
//            throw new InvalidOperationException("Cannot subtract money with different currencies");
//        return new Money(Amount - other.Amount, Currency);
//    }

//    public Money Multiply(decimal multiplier) => new(Amount * multiplier, Currency);

//    public override string ToString() => $"{Currency} {Amount:N2}";
//}
