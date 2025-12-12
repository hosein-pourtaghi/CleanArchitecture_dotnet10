using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;
    public async Task AddAsync(Order order) { _db.Orders.Add(order); await _db.SaveChangesAsync(); }
    public async Task<Order?> GetByIdAsync(Guid id) => await _db.Orders.AsNoTracking().Include(o=>o.Items).FirstOrDefaultAsync(o=>o.Id==id);
    public async Task<List<Order>> GetByUserAsync(string userId) => await _db.Orders.AsNoTracking().Where(o=>o.UserId==userId).ToListAsync();
}
