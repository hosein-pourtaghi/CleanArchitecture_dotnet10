using Application.Common.Data;
using Application.Common.Messaging;
using Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Assessments.Delete;
 
internal sealed class DeleteAssessmentCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteAssessmentCommand>
{
    public async Task<Result> Handle(DeleteAssessmentCommand command, CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (assessment is null)
        {
            return Result.Failure(AssessmentErrors.NotFound(command.Id));
        }

        // Capture assessment data before deletion for event publishing
        //var deletedEvent = new AssessmentDeletedDomainEvent(
        //    assessmentId: assessment.Id,
        //    name: assessment.Name,
        //    email: assessment.Email,
        //    phone: assessment.Phone,
        //    address: assessment.Address);
        //assessment.Raise(deletedEvent);

        // Remove from database
        context.Assessments.Remove(assessment);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
