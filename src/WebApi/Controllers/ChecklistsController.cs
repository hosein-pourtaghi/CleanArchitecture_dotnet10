using System.Text.Json;
using Application.Abstractions.Interfaces.Checklists;
using Application.Checklists.Create;
using Application.Checklists.Delete;
using Application.Checklists.GetAll;
using Application.Checklists.GetById;
using Application.Checklists.Update;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using Domain.Checklists;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Utilities;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class ChecklistsController(IMediator mediator, IChecklistRepository _checklistRepository) : ApiController
{

    [HttpPost]
    [ProducesResponseType(typeof(List<ChecklistDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetAll([FromBody] PaginatedRequest filter)
    {
        // Client uses filter parameter for ALL conditions:
        // ?filter=IsActive,true,Equal,And&filter=Title,Security,Contains,And
        var result = await mediator.Send(new GetAllChecklistQuery(filter));
        return HandleResult(result);
        //return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetByIdChecklistQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateChecklistCommand command,
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
        [FromBody] UpdateChecklistCommand command,
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
        var result = await mediator.Send(new DeleteChecklistCommand(id), cancellationToken);
        return HandleResult(result);
    }


    // POST: api/FakeChecklist/Generate
    [HttpPost("Generate")]
    public async Task<IActionResult> GenerateFakeChecklists()
    {
        for (int j = 0; j < 10_000; j++)
        {
            var lst = new List<Checklist>();
            for (int i = 0; i < 100; i++)
            {
                var checklist = new Checklist
                {
                    Id = Guid.NewGuid(),
                    IsActive = MockDataGenerator.Instance.GenerateIsActive(),
                    IsValid = true,
                    Title = MockDataGenerator.Instance.GenerateTitle(100)
                };

                GenerateFakeGroups(checklist);
                lst.Add(checklist);
            }
            await _checklistRepository.AddRangeAsync(lst, default);
            lst.Clear();
        }

        return Ok("100 checklists generated and saved successfully.");
    }

    private void GenerateFakeGroups(Checklist checklist)
    {
        for (int i = 0; i < MockDataGenerator.Instance.GeneratePriority(5); i++)
        {
            var group = new ChecklistGroup
            {
                Id = Guid.NewGuid(),
                IsActive = MockDataGenerator.Instance.GenerateIsActive(),
                Title = MockDataGenerator.Instance.GenerateGroupTitle(),
                ChecklistId = checklist.Id,
                ParentId = null,
                Priority = MockDataGenerator.Instance.GeneratePriority(10),
                IsShow = true
            };

            GenerateFakeQuestions(group);

            // Add the group to the checklist's groups collection
            checklist.Groups.Add(group);
        }
    }

    private void GenerateFakeQuestions(ChecklistGroup group)
    {
        for (int i = 0; i < MockDataGenerator.Instance.GeneratePriority(5); i++)
        {
            var questionType = MockDataGenerator.Instance.GenerateQuestionType();
            var question = new ChecklistQuestion
            {
                Id = Guid.NewGuid(),
                IsActive = MockDataGenerator.Instance.GenerateIsActive(),
                Title = MockDataGenerator.Instance.GenerateQuestionTitle(),
                GroupId = group.Id,
                Score = MockDataGenerator.Instance.GenerateScore(questionType),
                Priority = MockDataGenerator.Instance.GeneratePriority(10),
                IsRequiredAnswer = true,
                Type = questionType
            };

            // Add the question to the group's questions collection
            group.Questions.Add(question);

            if (questionType == ChecklistQuestionType.RadioButton || questionType == ChecklistQuestionType.CheckBox)
            {
                for (int j = 0; j < MockDataGenerator.Instance.GeneratePriority(5); j++)
                {
                    var optionType = MockDataGenerator.Instance.GenerateOptionType();
                    var option = new ChecklistQuestionOption
                    {
                        Id = Guid.NewGuid(),
                        Title = MockDataGenerator.Instance.GenerateTitle(100),
                        Description = MockDataGenerator.Instance.GenerateTitle(200),
                        Type = optionType,
                        ChecklistQuestionId = question.Id
                    };

                    // Add the option to the question's options collection
                    question.Options.Add(option);
                }
            }
        }
    }






    //[HttpGet("stream")]
    //public async Task GetStream()
    //{
    //    HttpContext.Response.ContentType = "text/event-stream";
    //    HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
    //    HttpContext.Response.Headers.Add("Connection", "keep-alive");

    //    while (true)
    //    {
    //        var checklist = await _checklistRepository.StreamAllAsync(new PaginatedRequest { Page = 1, PageSize = 100 },default) ;
    //        if (checklist != null)
    //        {
    //            string data = $"data: {JsonSerializer.Serialize(checklist)}\n\n";
    //            await HttpContext.Response.WriteAsync(data);
    //            await HttpContext.Response.Body.FlushAsync();
    //        }

    //        // You can control the frequency of updates here
    //        await Task.Delay(100); // Update every 5 seconds
    //    }
    //}


}

