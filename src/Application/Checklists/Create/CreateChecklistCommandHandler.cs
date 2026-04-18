using Application.Common.Interfaces.Checklists;
using Application.Common.Interfaces.Core;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Entities.Checklists;
using SharedKernel;

namespace Application.Checklists.Create;

internal sealed class CreateChecklistCommandHandler(
    IApplicationDbContext context,
    IChecklistRepository repository,
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

        await repository.AddAsync(checklist, cancellationToken);

        return checklist.Id;
    }
}

