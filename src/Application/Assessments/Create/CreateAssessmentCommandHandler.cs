
using Application.Common.Data;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Assessments.Create;

internal sealed class CreateAssessmentCommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<CreateAssessmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAssessmentCommand command, CancellationToken cancellationToken)
    { 
        // Create assessment entity
        var assessment = mapper.Map<Assessment>(command); 


        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //assessment.Raise(new AssessmentCreatedDomainEvent(
        //    assessmentId: assessment.Id,
        //    name: assessment.Name,
        //    email: assessment.Email,
        //    phone: assessment.Phone,
        //    address: assessment.Address));

        // Persist to database
        context.Assessments.Add(assessment);
        await context.SaveChangesAsync(cancellationToken);

        return assessment.Id;
    }
}

