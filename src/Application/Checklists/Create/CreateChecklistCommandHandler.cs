using Application.Common.Interfaces.Checklists;
using Application.Common.Interfaces.Core;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Aggregates.Checklists;
using IdentityApi.Application.Interfaces;
using IdentityApi.Infrastructure.Services;
using SharedKernel;

namespace Application.Checklists.Create;

internal sealed class CreateChecklistCommandHandler(
    IApplicationDbContext context,
    IChecklistRepository repository,
    ICurrentUserService currentUserService,
    IMapper mapper)
    : ICommandHandler<CreateChecklistCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateChecklistCommand command, CancellationToken cancellationToken)
    {
        // Create checklist entity
        var checklist = mapper.Map<Checklist>(command);

        await repository.AddAsync(checklist, cancellationToken);

        // 4. Raise domain event for audit logging and async operations
        // The event will be captured by SaveChangesAsync and dispatched
        checklist.AddDomainEvent(new ChecklistCreatedDomainEvent
        {
            ChecklistId = checklist.Id,
            Title = checklist.Title,
            Version = checklist.Version,
            IsActive = checklist.IsActive,
            IsValid = checklist.IsValid,
            TotalScore = checklist.TotalScore,
            GroupCount = checklist.Groups?.Count ?? 0,
            CreatedById = currentUserService.UserId,
            CreatedByName = currentUserService.Email,
            CreatedAtUtc = checklist.CreatedAt,
            EventSequence = 1
        });

        // 5. Save changes (this triggers domain event dispatch)
        await context.SaveChangesAsync(cancellationToken);

        return checklist.Id;
    }
}

