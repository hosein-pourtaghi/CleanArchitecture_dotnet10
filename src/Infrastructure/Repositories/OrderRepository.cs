
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
//
// public class CartRepository : ICartRepository
// {
//     private readonly AppDbContext _db;
//     public CartRepository(AppDbContext db) => _db = db;
//
//     public async Task AddAsync(Cart cart)
//     {
//             _db.Carts.Add(cart);
//         await _db.SaveChangesAsync();
//     }
//
//     public async Task<Cart?> GetByIdAsync(Guid id) =>
//         await _db.Carts.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
//
//     public async Task<List<Cart>> GetByUserAsync(string userId) =>
//         await _db.Carts.AsNoTracking().Where(o => o.UserId == userId).ToListAsync();
// }
