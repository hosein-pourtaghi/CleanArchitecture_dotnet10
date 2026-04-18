using Application.Common.DTOs.Checklists;
using Application.Common.Messaging;

namespace  Application.Assessments.GetAll;
 
public sealed record GetAllAssessmentQuery() : IQuery<List<AssessmentDto>>;

