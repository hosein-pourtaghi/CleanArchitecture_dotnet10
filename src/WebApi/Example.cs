//// Example: Using the exception system without manual throwing

//using Application.Common.Data;
//using Application.Common.Interfaces;
//using FluentValidation;
//using MediatR;
//using SharedKernel;

//namespace Application.Products.Commands;

//public class CreateProductCommand : IRequest<Result<Guid>>
//{
//    public string Name { get; set; } = string.Empty;
//    public decimal Price { get; set; }
//    public int Stock { get; set; }
//}

//public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IProductRepository _repository;

//    public CreateProductCommandHandler(
//        IApplicationDbContext context,
//        IProductRepository repository)
//    {
//        _context = context;
//        _repository = repository;
//    }

//    public async Task<Result<Guid>> Handle(
//        CreateProductCommand request,
//        CancellationToken cancellationToken)
//    {
//        // Validation is handled automatically by FluentValidation + ValidationBehavior

//        // Business logic - use Result pattern instead of throwing exceptions
//        if (request.Stock < 0)
//        {
//            return Result.Failure<Guid>(
//                "PRODUCT.INVALID_STOCK",
//                "Stock cannot be negative");
//        }

//        if (request.Price <= 0)
//        {
//            return Result.Failure<Guid>(
//                "PRODUCT.INVALID_PRICE",
//                "Price must be greater than zero");
//        }

//        // Check for duplicate (returns Result, not throws)
//        var existingProduct = await _repository.GetByNameAsync(request.Name, cancellationToken);
//        if (existingProduct != null)
//        {
//            return Result.Failure<Guid>(
//                "PRODUCT.DUPLICATE",
//                $"Product with name '{request.Name}' already exists");
//        }

//        // Create product
//        var product = new Product
//        {
//            Id = Guid.NewGuid(),
//            Name = request.Name,
//            Price = request.Price,
//            Stock = request.Stock,
//            CreatedAt = DateTime.UtcNow
//        };

//        await _repository.AddAsync(product, cancellationToken);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result.Success<Guid>(product.Id);
//    }
//}

//// FluentValidation validator
//public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
//{
//    public CreateProductCommandValidator()
//    {
//        RuleFor(x => x.Name)
//            .NotEmpty()
//            .WithMessage("نام محصول الزامی است")
//            .MaximumLength(200)
//            .WithMessage("نام محصول نمی‌تواند بیش از 200 کاراکتر باشد");

//        RuleFor(x => x.Price)
//            .GreaterThan(0)
//            .WithMessage("قیمت باید بزرگتر از صفر باشد");

//        RuleFor(x => x.Stock)
//            .GreaterThanOrEqualTo(0)
//            .WithMessage("موجودی نمی‌تواند منفی باشد");
//    }
//}
