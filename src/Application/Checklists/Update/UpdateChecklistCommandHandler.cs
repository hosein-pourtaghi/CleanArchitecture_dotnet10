
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
        //var checklist = await repository.GetByIdAsync(command.Id, cancellationToken);
        //if (checklist is null)
        //{
        //    return Result.Failure(ChecklistErrors.NotFound(command.Id));
        //}

        var newChecklist = mapper.Map<Checklist>(command);
        await repository.UpdateAsync(command.Id, newChecklist);

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

