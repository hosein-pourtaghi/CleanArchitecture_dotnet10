using Application.Assessments.Create;
using Application.Assessments.Delete;
using Application.Assessments.GetAll;
using Application.Assessments.GetById;
using Application.Assessments.Update;
using Application.Common.DTOs.Checklists;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedApi.Controllers;

namespace WebApi.Controllers;

[Area("Checklist")]
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
[ResponseCache(Duration = 0, NoStore = true, VaryByHeader = "*")]
public class AssessmentsController(IMediator mediator) : ApiController
{

    [HttpGet]
    [ProducesResponseType(typeof(List<AssessmentDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllError(CancellationToken cancellationToken)
    {
        throw new Exception("this is a test Exception");
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AssessmentDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAssessmentQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetByIdAssessmentQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult<Guid>(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAssessmentCommand(id), cancellationToken);
        return HandleResult(result);
    }

}

