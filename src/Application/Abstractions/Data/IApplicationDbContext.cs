using Domain.Assessments;
using Domain.Carts;
using Domain.Checklists;
using Domain.Customers;
using Domain.Products;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Cart> Carts { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Checklist> Checklists { get; set; }
    DbSet<Assessment> Assessments { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
