
using Application.Common.Messaging;
using Application.Common.DTOs;

namespace  Application.Checklists.Delete;
 
public sealed record DeleteChecklistCommand(Guid Id) : ICommand;
