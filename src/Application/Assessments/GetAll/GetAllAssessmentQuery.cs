
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Assessments.GetAll;
 
public sealed record GetAllAssessmentQuery() : IQuery<List<AssessmentDto>>;

