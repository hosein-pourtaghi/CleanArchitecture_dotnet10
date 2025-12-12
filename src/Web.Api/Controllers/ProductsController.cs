using Application.Interfaces;
using Application.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
namespace Web.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;
    public ProductsController(IProductRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _repo.GetAsync(id);
        if (p==null) return NotFound();
        return Ok(_mapper.Map<ProductDto>(p));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductDto dto)
    {
        var p = _mapper.Map<Domain.Entities.Product>(dto);
        await _repo.AddAsync(p);
        return CreatedAtAction(nameof(Get), new { id = p.Id }, _mapper.Map<ProductDto>(p));
    }
}
