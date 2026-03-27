
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Assessments.Delete;
 
public sealed record DeleteAssessmentCommand(Guid Id) : ICommand;
