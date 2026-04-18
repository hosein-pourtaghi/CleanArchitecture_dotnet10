using Application.Common.Interfaces.Checklists;
using Application.Common.Interfaces.Core;
using Application.Common.Messaging;
using SharedKernel;

namespace Application.Checklists.Delete;

internal sealed class DeleteChecklistCommandHandler(
    IApplicationDbContext context,
    IChecklistRepository repository
    )
    : ICommandHandler<DeleteChecklistCommand>
{
    public async Task<Result> Handle(DeleteChecklistCommand command, CancellationToken cancellationToken)
    {


        // Capture checklist data before deletion for event publishing
        //var deletedEvent = new ChecklistDeletedDomainEvent(
        //    checklistId: checklist.Id,
        //    name: checklist.Name,
        //    email: checklist.Email,
        //    phone: checklist.Phone,
        //    address: checklist.Address);
        //checklist.Raise(deletedEvent);

        // Remove from database
        await repository.DeleteAsync(command.Id);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
