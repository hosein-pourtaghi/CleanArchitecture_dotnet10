
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Checklists.Delete;
 
public sealed record DeleteChecklistCommand(Guid Id) : ICommand;
