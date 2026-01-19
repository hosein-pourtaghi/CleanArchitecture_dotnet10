using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.Generate;

internal sealed class GenerateCartCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<GenerateCartCommand>
{
    public List<string> strings = new List<string>() { "one", "two", "three", "four", "five", "six", "seven",
            "ten", "ten1", "ten2", "ten3", "ten4", "ten5", "ten6", "ten7", "ten8", "ten9", "ten10", "ten11", "ten12",
            "ten13", "ten14", "one one","two two","three three","four four","five five","six six","seven seven","ten ten",
            "ten1 ten1","ten2 ten2","ten3 ten3","ten4 ten4","ten5 ten5","ten6 ten6","ten7 ten7","ten8 ten8","ten9 ten9",
            "ten10 ten10","ten11 ten11","ten12 ten12","ten13 ten13",
        };

    public async Task<Result> Handle(GenerateCartCommand command, CancellationToken cancellationToken)
    {
        var customerIds = context.Customers.AsNoTracking().Select(x => x.Id).Skip(1000).Take(1000).ToArray();
        var productIds = context.Products.AsNoTracking().Select(x => x.Id).Skip(1000).Take(1000).ToArray();

        var index = 0;
        for (int j = 0; j < 1_000; j++)
        {
            var chunk = 1_000;
            var carts = new List<Cart>();

            for (int i = index; i < index + chunk; i++)
            {
                var cart = new Cart
                {
                    CustomerId = customerIds[MyRandom(999)],
                    Currency = strings[MyRandom(strings.Count)],
                    TransactionId = strings[MyRandom(strings.Count)],
                    PaymentType = strings[MyRandom(strings.Count)],
                    Code = strings[MyRandom(strings.Count)],
                    PurchaseDate = MyRandomDate()


                };
                AddCartItems(cart, productIds);

                carts.Add(cart);
            }
            context.Carts.AddRange(carts);
            carts.Clear();

            await context.SaveChangesAsync(cancellationToken);
            index += chunk;
        }

        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //cart.Raise(new CartGeneratedDomainEvent(
        //    cartId: cart.Id,
        //    name: cart.Name,
        //    email: cart.Email,
        //    phone: cart.Phone,
        //    address: cart.Address));


        return Result.Success();
    }

    private void AddCartItems(Cart cart, Guid[] productIds)
    {


        var rand = MyRandom(100);
        for (int i = 0; i < rand; i++)
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                Quantity = MyRandom(1000),
                OriginalPrice = MyRandom(1000),
                DiscountedPrice = MyRandom(1000),
                DiscountAmount = MyRandom(1000),
                TaxPrice = MyRandom(1000),
                TransactionId = strings[MyRandom(strings.Count)],
                CouponCode = strings[MyRandom(strings.Count)],
                ProductId = productIds[MyRandom(999)]

            };
            cart.CartItems.Add(cartItem);
        }

    }

    private static int MyRandom(int max)
    {
        var rand = new Random();

#pragma warning disable CA5394 // Do not use insecure randomness
        var num = rand.Next(0, maxValue: max);
#pragma warning restore CA5394 // Do not use insecure randomness
        return num;
    }

    private static DateTime MyRandomDate()
    {
        var rand = new Random();

#pragma warning disable CA5394 // Do not use insecure randomness
        var year = rand.Next(2000, maxValue: 2026);
        var month = rand.Next(1, maxValue: 12);
        var day = rand.Next(1, maxValue: 28);
#pragma warning restore CA5394 // Do not use insecure randomness
        var date = new DateTime(year, month, day).ToUniversalTime();
        return date;

    }




}

