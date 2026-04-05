
using Application.Common.Messaging;
using Application.Common.DTOs;

namespace  Application.Assessments.Delete;
 
public sealed record DeleteAssessmentCommand(Guid Id) : ICommand;
