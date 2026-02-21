using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Generate;

internal sealed class GenerateProductCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<GenerateProductCommand>
{
    public async Task<Result> Handle(GenerateProductCommand command, CancellationToken cancellationToken)
    {
        List<string> strings = new List<string>() { "one", "two", "three", "four", "five", "six", "seven",
            "ten", "ten1", "ten2", "ten3", "ten4", "ten5", "ten6", "ten7", "ten8", "ten9", "ten10", "ten11", "ten12",
            "ten13", "ten14", "one one","two two","three three","four four","five five","six six","seven seven","ten ten",
            "ten1 ten1","ten2 ten2","ten3 ten3","ten4 ten4","ten5 ten5","ten6 ten6","ten7 ten7","ten8 ten8","ten9 ten9",
            "ten10 ten10","ten11 ten11","ten12 ten12","ten13 ten13",
        };

        var index = 0;
        for (int j = 0; j < 1_000; j++)
        {
            var chunk = 1_000;
            var products = new List<Product>();

            for (int i = index; i < index + chunk; i++)
            {
                var product = new Product
                {
                    Number = strings[MyRandom(strings.Count)],
                    Name = strings[MyRandom(strings.Count)],
                    Description = strings[MyRandom(strings.Count)],
                    ManufacturePart = strings[MyRandom(strings.Count)],
                    ShortDesc = strings[MyRandom(strings.Count)],
                    Barcode = strings[MyRandom(strings.Count)],
                    DLC = true,
                    UL = true,
                    ETL = true,
                    ES = true,
                    CUL = true,
                    CETL = true,
                    IP = strings[MyRandom(strings.Count)],
                    SKU = strings[MyRandom(strings.Count)],
                    SPC = strings[MyRandom(strings.Count)],
                    ManufactorerPartNumber = strings[MyRandom(strings.Count)],
                    Image = strings[MyRandom(strings.Count)],
                    Taxable = true,
                    Salable = true,
                    Enable = true,
                    BatchTraceable = true,
                    EccomerceViewable = true,
                    EccomerceSalable = true,
                    Notes = strings[MyRandom(strings.Count)],
                    AlternateName = strings[MyRandom(strings.Count)],
                    MaximumStockLevel = 1,
                    MinimumStockLevel = 2,
                    PurchasePrice = 3,
                };
                products.Add(product);

            }
            context.Products.AddRange(products);
            products.Clear();

            await context.SaveChangesAsync(cancellationToken);
            index += chunk;
        }

        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //product.Raise(new ProductGeneratedDomainEvent(
        //    productId: product.Id,
        //    name: product.Name,
        //    email: product.Email,
        //    phone: product.Phone,
        //    address: product.Address));


        return Result.Success();
    }

    private static int MyRandom(int max)
    {
        var rand = new Random();

#pragma warning disable CA5394 // Do not use insecure randomness
        var num = rand.Next(0, maxValue: max);
#pragma warning restore CA5394 // Do not use insecure randomness
        return num;
    }

   

}

