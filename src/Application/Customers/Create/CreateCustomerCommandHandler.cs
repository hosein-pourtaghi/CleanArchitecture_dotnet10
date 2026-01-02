using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Create;

internal sealed class CreateCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateCustomerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        bool emailExists = await context.Customers.AnyAsync(c => c.Email == command.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure<Guid>(Error.Conflict("Customers.EmailExists", $"Customer with email '{command.Email}' already exists."));
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            Phone = command.Phone,
            Address = command.Address,
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}