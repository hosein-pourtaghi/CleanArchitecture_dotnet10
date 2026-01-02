using Domain.Users;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Customer> Customers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
