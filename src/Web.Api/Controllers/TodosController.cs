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
using GetTodoResponse = Application.Todos.Get.TodoResponse;
using GetByIdTodoResponse = Application.Todos.GetById.TodoResponse;

namespace Web.Api.Controllers;

/// <summary>
/// Todos API endpoints for managing todo item resources.
/// </summary>
[Route("todos")]
[Authorize]
[Tags("todos")]
public class TodosController : ApiController
{
    /// <summary>
    /// Retrieves all todos for a specific user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTodos(
        [FromQuery] Guid userId,
        IQueryHandler<GetTodosQuery, List<GetTodoResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTodosQuery(userId);
        var result = await handler.Handle(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific todo by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        IQueryHandler<GetTodoByIdQuery, GetByIdTodoResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTodoByIdQuery(id);
        var result = await handler.Handle(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new todo item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
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

        var result = await handler.Handle(command, cancellationToken);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Value });
    }

    /// <summary>
    /// Marks a todo as complete.
    /// </summary>
    [HttpPut("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Complete(
        Guid id,
        ICommandHandler<CompleteTodoCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTodoCommand(id);
        var result = await handler.Handle(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a copy of an existing todo.
    /// </summary>
    [HttpPost("{todoId:guid}/copy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Copy(
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

        var result = await handler.Handle(command, cancellationToken);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Value });
    }

    /// <summary>
    /// Deletes a todo item by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        ICommandHandler<DeleteTodoCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTodoCommand(id);
        var result = await handler.Handle(command, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for creating a new todo.
/// </summary>
public sealed class CreateTodoRequest
{
    /// <summary>The ID of the user creating the todo.</summary>
    public Guid UserId { get; set; }

    /// <summary>The todo description.</summary>
    public string Description { get; set; }

    /// <summary>Optional due date for the todo.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Optional labels/tags for the todo.</summary>
    public List<string> Labels { get; set; } = [];

    /// <summary>Priority level of the todo (0-3).</summary>
    public int Priority { get; set; }
}

/// <summary>
/// Request model for copying a todo.
/// </summary>
public sealed class CopyTodoRequest
{
    /// <summary>The ID of the user copying the todo.</summary>
    public Guid UserId { get; set; }
}
