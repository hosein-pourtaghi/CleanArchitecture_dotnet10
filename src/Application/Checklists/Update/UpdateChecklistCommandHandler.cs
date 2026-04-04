
using Application.Abstractions.Data;
using Application.Abstractions.Interfaces.Checklists;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Checklists;
using SharedKernel;

namespace Application.Checklists.Update;

internal sealed class UpdateChecklistCommandHandler(
    IApplicationDbContext context,
    IChecklistRepository repository,
    IMapper mapper
    )
    : ICommandHandler<UpdateChecklistCommand>
{
    public async Task<Result> Handle(UpdateChecklistCommand command, CancellationToken cancellationToken)
    {
        //var checklist = await repository.GetByIdAsync(command.Id, true, true);

        //if (checklist == null)
        //{
        //    return Result.Failure(ChecklistErrors.NotFound(command.Id));
        //}

        // Map command to new structure (without tracking)
        var newStructure = mapper.Map<Checklist>(command);

        // Update with versioning
        await repository.UpdateAsync(command.Id, newStructure);
 
        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //checklist.Raise(new ChecklistUpdatedDomainEvent(
        //    checklistId: checklist.Id,
        //    name: checklist.Name,
        //    email: checklist.Email,
        //    phone: checklist.Phone,
        //    address: checklist.Address));

        return Result.Success();
    }
    
}
