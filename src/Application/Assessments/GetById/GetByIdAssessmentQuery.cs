using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Assessments.GetById;

/// <summary>
/// Query to retrieve a Assessment by its unique identifier.
/// Inherits from IQuery&lt;AssessmentDto&gt; which returns Result&lt;AssessmentDto&gt;.
/// Handled by <see cref="GetByIdAssessmentQueryHandler"/>
/// </summary>
public sealed record GetByIdAssessmentQuery(Guid Id) : IQuery<AssessmentDto>;
