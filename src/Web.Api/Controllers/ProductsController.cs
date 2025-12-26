using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;

namespace Web.Api.Controllers;

/// <summary>
/// Products controller with OpenTelemetry instrumentation.
/// Logs all product operations with trace context and correlation IDs.
/// Ready for implementation with business logic.
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class ProductsController(ILogger<ProductsController> logger) : ControllerBase
{
    //private readonly IProductRepository _repo;
    //private readonly IMapper _mapper;
    //public ProductsController(IProductRepository repo, IMapper mapper, ILogger<ProductsController> logger) 
    //{ 
    //    _repo = repo; 
    //    _mapper = mapper;
    //    _logger = logger;
    //}

    //[HttpGet]
    //public async Task<IActionResult> GetAll()
    //{
    //    var operationName = "ProductsController.GetAll";
    //    var correlationId = HttpContext.TraceIdentifier;
    //    var stopwatch = Stopwatch.StartNew();

    //    using (var activity = logger.StartOperationSpan(operationName, correlationId))
    //    {
    //        try
    //        {
    //            logger.LogInformation(
    //                "Fetching all products. CorrelationId: {CorrelationId}",
    //                correlationId);

    //            var products = await _repo.GetAllAsync();

    //            stopwatch.Stop();
    //            activity?.SetTag("products.count", products?.Count() ?? 0);
    //            logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

    //            return Ok(products);
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
    //    var operationName = "ProductsController.Get";
    //    var correlationId = HttpContext.TraceIdentifier;
    //    var stopwatch = Stopwatch.StartNew();

    //    using (var activity = logger.StartOperationSpan(operationName, correlationId))
    //    {
    //        try
    //        {
    //            activity?.SetTag("product.id", id);

    //            logger.LogInformation(
    //                "Fetching product. ProductId: {ProductId}, CorrelationId: {CorrelationId}",
    //                id,
    //                correlationId);

    //            var p = await _repo.GetAsync(id);
    //            if (p == null)
    //            {
    //                activity?.SetTag("product.found", false);
    //                logger.LogWarning(
    //                    "Product not found. ProductId: {ProductId}, CorrelationId: {CorrelationId}",
    //                    id,
    //                    correlationId);
    //                return NotFound();
    //            }

    //            stopwatch.Stop();
    //            activity?.SetTag("product.found", true);
    //            logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

    //            return Ok(_mapper.Map<ProductDto>(p));
    //        }
    //        catch (Exception ex)
    //        {
    //            stopwatch.Stop();
    //            logger.LogOperationError(ex, operationName, correlationId);
    //            throw;
    //        }
    //    }
    //}

    //[HttpPost]
    //public async Task<IActionResult> Create([FromBody] ProductDto dto)
    //{
    //    var operationName = "ProductsController.Create";
    //    var correlationId = HttpContext.TraceIdentifier;
    //    var stopwatch = Stopwatch.StartNew();

    //    using (var activity = logger.StartOperationSpan(operationName, correlationId))
    //    {
    //        try
    //        {
    //            activity?.SetTag("product.name", dto.Name);

    //            logger.LogInformation(
    //                "Product creation initiated. Name: {Name}, CorrelationId: {CorrelationId}",
    //                dto.Name,
    //                correlationId);

    //            var p = _mapper.Map<Domain.Entities.Product>(dto);
    //            await _repo.AddAsync(p);

    //            stopwatch.Stop();
    //            activity?.SetTag("product.id", p.Id);
    //            logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

    //            return CreatedAtAction(nameof(Get), new { id = p.Id }, _mapper.Map<ProductDto>(p));
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
