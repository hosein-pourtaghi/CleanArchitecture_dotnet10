
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Assessments.Update;
 
internal sealed class UpdateAssessmentCommandHandler(
    IApplicationDbContext context,
    IMapper mapper
    )
    : ICommandHandler<UpdateAssessmentCommand>
{
    public async Task<Result> Handle(UpdateAssessmentCommand command, CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (assessment is null)
        {
            return Result.Failure(AssessmentErrors.NotFound(command.Id));
        }

        // Update entity with new values 
        assessment = mapper.Map<Assessment>(command);


        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //assessment.Raise(new AssessmentUpdatedDomainEvent(
        //    assessmentId: assessment.Id,
        //    name: assessment.Name,
        //    email: assessment.Email,
        //    phone: assessment.Phone,
        //    address: assessment.Address));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

