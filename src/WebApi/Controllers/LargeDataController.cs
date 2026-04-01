// src/WebApi/Controllers/LargeDataController.cs
using System.Text.Json;
using Application.Common.DTOs.Shared;
using Application.Streams.GetLargeData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
[Tags("Large Data")]
[Produces("application/json")]
[Consumes("application/json")]
public class LargeDataController(IMediator mediator) : ApiController
{

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> StreamLargeData([FromQuery] PaginatedRequest filter, CancellationToken cancellationToken)
    {
        Response.ContentType = "application/json; charset=utf-8";

        // IMPORTANT: disable automatic buffering to stream immediately
        HttpContext.Response.Headers["Cache-Control"] = "no-store";

        var query = new GetLargeDataQuery(filter);

        var result = await mediator.Send(query, cancellationToken);

        //return result.Value; // ASP.NET will stream automatically

        if (!result.IsSuccess)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }

        var stream = result.Value;   // IAsyncEnumerable<Dto>

        await Response.WriteAsync("[", cancellationToken);

        bool first = true;

        await foreach (var item in stream.WithCancellation(cancellationToken))
        {
            if (!first)
                await Response.WriteAsync(",", cancellationToken);

            first = false;

            // serialize each object as you go
            var json = JsonSerializer.Serialize(item);
            await Response.WriteAsync(json, cancellationToken);

            // flush to client continuously
            await Response.Body.FlushAsync(cancellationToken);
        }
         

        return new EmptyResult();
    }

    private static readonly List<string> Checklists = new()
    {
        "Task 1", "Task 2", "Task 3", "Task 4", "Task 5"
    };

    [HttpGet]
    public async Task GetEvents()
    {
        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers["Cache-Control"] = "no-cache";
        HttpContext.Response.Headers["Connection"] = "keep-alive";

        var checklists = new List<string>
        {
            "Task 1", "Task 2", "Task 3", "Task 4", "Task 5"
        };

        foreach (var checklist in checklists)
        {
        var line = $"data: {checklist} \n\n";
        await HttpContext.Response.WriteAsync(line, System.Text.Encoding.UTF8);
            await Task.Delay(1000); // Simulate a delay between events
        }

        HttpContext.Response.Body.Flush();
    }
     


}

