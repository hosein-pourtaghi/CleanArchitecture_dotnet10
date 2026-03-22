
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Checklists;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Checklists.Update;
 
internal sealed class UpdateChecklistCommandHandler(
    IApplicationDbContext context,
    IMapper mapper
    )
    : ICommandHandler<UpdateChecklistCommand>
{
    public async Task<Result> Handle(UpdateChecklistCommand command, CancellationToken cancellationToken)
    {
        var checklist = await context.Checklists.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (checklist is null)
        {
            return Result.Failure(ChecklistErrors.NotFound(command.Id));
        }

        // Update entity with new values 
        checklist = mapper.Map<Checklist>(command);


        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //checklist.Raise(new ChecklistUpdatedDomainEvent(
        //    checklistId: checklist.Id,
        //    name: checklist.Name,
        //    email: checklist.Email,
        //    phone: checklist.Phone,
        //    address: checklist.Address));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

