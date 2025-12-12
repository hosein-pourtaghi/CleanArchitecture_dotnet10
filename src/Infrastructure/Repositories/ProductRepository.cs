using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;
    public async Task AddAsync(Product p) { _db.Products.Add(p); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(Guid id) { var e = await _db.Products.FindAsync(id); if (e!=null){ _db.Products.Remove(e); await _db.SaveChangesAsync(); } }
    public async Task<List<Product>> GetAllAsync() => await _db.Products.AsNoTracking().ToListAsync();
    public async Task<Product?> GetAsync(Guid id) => await _db.Products.AsNoTracking().FirstOrDefaultAsync(p=>p.Id==id);
    public async Task UpdateAsync(Product p) { _db.Products.Update(p); await _db.SaveChangesAsync(); }
}
