using Application.Common.Interfaces.Checklists;
using Application.Common.Interfaces.Core;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Aggregates.Checklists;
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





    //// Simple usage - just call UpdateAggregateAsync
    //public class UpdateChecklistCommandHandler : ICommandHandler<UpdateChecklistCommand>
    //{
    //    private readonly IChecklistRepository _repository;
    //    private readonly IMapper _mapper;

    //    public UpdateChecklistCommandHandler(IChecklistRepository repository, IMapper mapper)
    //    {
    //        _repository = repository;
    //        _mapper = mapper;
    //    }

    //    public async Task<Result> Handle(UpdateChecklistCommand command, CancellationToken cancellationToken)
    //    {
    //        var checklist = await _repository.GetByIdAsync(command.Id, true, false);

    //        if (checklist == null)
    //            return Result.Failure(ChecklistErrors.NotFound(command.Id));

    //        var newStructure = _mapper.Map<Checklist>(command);

    //        // This is all you need - generic update handles everything!
    //        await _repository.UpdateSimpleAsync(command.Id, newStructure);

    //        return Result.Success();
    //    }
    //}

    //// Or use the generic interface directly
    //public class GenericHandler
    //{
    //    private readonly IAdvancedRepository<ApplicationDbContext, Checklist> _repository;

    //    public GenericHandler(IAdvancedRepository<ApplicationDbContext, Checklist> repository)
    //    {
    //        _repository = repository;
    //    }

    //    public async Task UpdateChecklist(Guid id, Checklist checklist)
    //    {
    //        // Works for any aggregate!
    //        await _repository.UpdateAggregateAsync(id, checklist);
    //    }
    //}






}
