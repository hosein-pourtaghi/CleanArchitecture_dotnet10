using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Delete;

internal sealed class DeleteCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteCustomerCommand>
{
    public async Task<Result> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(Error.NotFound("Customers.NotFound", $"Customer with Id '{command.Id}' was not found"));
        }

        context.Customers.Remove(customer);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}