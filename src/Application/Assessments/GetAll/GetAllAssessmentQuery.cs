
using Application.Common.DTOs;
using Application.Common.Messaging;

namespace  Application.Assessments.GetAll;
 
public sealed record GetAllAssessmentQuery() : IQuery<List<AssessmentDto>>;

