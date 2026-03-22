
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Checklists;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Checklists.Create;

internal sealed class CreateChecklistCommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<CreateChecklistCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateChecklistCommand command, CancellationToken cancellationToken)
    { 
        // Create checklist entity
        var checklist = mapper.Map<Checklist>(command); 


        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //checklist.Raise(new ChecklistCreatedDomainEvent(
        //    checklistId: checklist.Id,
        //    name: checklist.Name,
        //    email: checklist.Email,
        //    phone: checklist.Phone,
        //    address: checklist.Address));

        // Persist to database
        context.Checklists.Add(checklist);
        await context.SaveChangesAsync(cancellationToken);

        return checklist.Id;
    }
}

