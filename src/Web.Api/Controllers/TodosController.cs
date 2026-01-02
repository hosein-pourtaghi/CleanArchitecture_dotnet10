using Application.Abstractions.Messaging;
using Application.Todos.Complete;
using Application.Todos.Copy;
using Application.Todos.Create;
using Application.Todos.Delete;
using Application.Todos.Get;
using Application.Todos.GetById;
using Domain.Todos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;
using GetTodoResponse = Application.Todos.Get.TodoResponse;
using GetByIdTodoResponse = Application.Todos.GetById.TodoResponse;

namespace Web.Api.Controllers;

[ApiController]
[Route("todos")]
[Authorize]
[Tags("todos")]
public class TodosController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTodos(
        [FromQuery] Guid userId,
        IQueryHandler<GetTodosQuery, List<GetTodoResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTodosQuery(userId);
        Result<List<GetTodoResponse>> result = await handler.Handle(query, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTodoById(
        Guid id,
        IQueryHandler<GetTodoByIdQuery, GetByIdTodoResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTodoByIdQuery(id);
        Result<GetByIdTodoResponse> result = await handler.Handle(query, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo(
        CreateTodoRequest request,
        ICommandHandler<CreateTodoCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateTodoCommand
        {
            UserId = request.UserId,
            Description = request.Description,
            DueDate = request.DueDate,
            Labels = request.Labels,
            Priority = (Priority)request.Priority
        };

        Result<Guid> result = await handler.Handle(command, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTodo(
        Guid id,
        ICommandHandler<CompleteTodoCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTodoCommand(id);
        Result result = await handler.Handle(command, cancellationToken);
        return result.Match(_ => NoContent(), CustomResults.Problem);
    }

    [HttpPost("{todoId:guid}/copy")]
    public async Task<IActionResult> CopyTodo(
        Guid todoId,
        CopyTodoRequest request,
        ICommandHandler<CopyTodoCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new CopyTodoCommand
        {
            UserId = request.UserId,
            TodoId = todoId
        };

        Result<Guid> result = await handler.Handle(command, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTodo(
        Guid id,
        ICommandHandler<DeleteTodoCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTodoCommand(id);
        Result result = await handler.Handle(command, cancellationToken);
        return result.Match(_ => NoContent(), CustomResults.Problem);
    }
}

public sealed class CreateTodoRequest
{
    public Guid UserId { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
    public int Priority { get; set; }
}

public sealed class CopyTodoRequest
{
    public Guid UserId { get; set; }
}
