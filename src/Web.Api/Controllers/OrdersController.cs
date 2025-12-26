using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;

namespace Web.Api.Controllers;

/// <summary>
/// Orders controller with OpenTelemetry instrumentation.
/// Logs all order operations with trace context and correlation IDs.
/// Ready for implementation with business logic.
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController(ILogger<OrdersController> logger) : ControllerBase
{
    //private readonly IOrderRepository _orders;
    //private readonly IMessagePublisher _publisher;
    //public OrdersController(IOrderRepository orders, IMessagePublisher publisher, ILogger<OrdersController> logger) 
    //{ 
    //    _orders = orders; 
    //    _publisher = publisher; 
    //    _logger = logger;
    //}

    //[HttpPost]
    //public async Task<IActionResult> Create([FromBody] Order order)
    //{
    //    var operationName = "OrdersController.Create";
    //    var correlationId = HttpContext.TraceIdentifier;
    //    var stopwatch = Stopwatch.StartNew();

    //    using (var activity = logger.StartOperationSpan(operationName, correlationId))
    //    {
    //        try
    //        {
    //            activity?.SetTag("order.id", order.Id);
    //            activity?.SetTag("order.user_id", order.UserId);
    //            activity?.SetTag("order.total", order.Total);

    //            logger.LogInformation(
    //                "Order creation initiated. OrderId: {OrderId}, UserId: {UserId}, Total: {Total}, CorrelationId: {CorrelationId}",
    //                order.Id,
    //                order.UserId,
    //                order.Total,
    //                correlationId);

    //            await _orders.AddAsync(order);
    //            var payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
    //                new { order.Id, order.UserId, order.Total });
    //            _publisher.Publish("orders", "order.created", payload);

    //            stopwatch.Stop();
    //            logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

    //            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    //        }
    //        catch (Exception ex)
    //        {
    //            stopwatch.Stop();
    //            logger.LogOperationError(ex, operationName, correlationId);
    //            throw;
    //        }
    //    }
    //}

    //[HttpGet("{id}")]
    //public async Task<IActionResult> Get(Guid id)
    //{
    //    var operationName = "OrdersController.Get";
    //    var correlationId = HttpContext.TraceIdentifier;
    //    var stopwatch = Stopwatch.StartNew();

    //    using (var activity = logger.StartOperationSpan(operationName, correlationId))
    //    {
    //        try
    //        {
    //            activity?.SetTag("order.id", id);

    //            logger.LogInformation(
    //                "Fetching order. OrderId: {OrderId}, CorrelationId: {CorrelationId}",
    //                id,
    //                correlationId);

    //            var order = await _orders.GetByIdAsync(id);

    //            stopwatch.Stop();
    //            logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

    //            return Ok(order);
    //        }
    //        catch (Exception ex)
    //        {
    //            stopwatch.Stop();
    //            logger.LogOperationError(ex, operationName, correlationId);
    //            throw;
    //        }
    //    }
    //}
}
