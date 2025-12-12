using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
namespace Web.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orders;
    private readonly IMessagePublisher _publisher;
    public OrdersController(IOrderRepository orders, IMessagePublisher publisher) { _orders = orders; _publisher = publisher; }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Order order)
    {
        await _orders.AddAsync(order);
        // publish minimal payload
        var payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { order.Id, order.UserId, order.Total });
        _publisher.Publish("orders", "order.created", payload);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id) => Ok(await _orders.GetByIdAsync(id));
}
