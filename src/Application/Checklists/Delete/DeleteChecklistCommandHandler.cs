using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Checklists;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Checklists.Delete;
 
internal sealed class DeleteChecklistCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteChecklistCommand>
{
    public async Task<Result> Handle(DeleteChecklistCommand command, CancellationToken cancellationToken)
    {
        var checklist = await context.Checklists.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (checklist is null)
        {
            return Result.Failure(ChecklistErrors.NotFound(command.Id));
        }

        // Capture checklist data before deletion for event publishing
        //var deletedEvent = new ChecklistDeletedDomainEvent(
        //    checklistId: checklist.Id,
        //    name: checklist.Name,
        //    email: checklist.Email,
        //    phone: checklist.Phone,
        //    address: checklist.Address);
        //checklist.Raise(deletedEvent);

        // Remove from database
        context.Checklists.Remove(checklist);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
