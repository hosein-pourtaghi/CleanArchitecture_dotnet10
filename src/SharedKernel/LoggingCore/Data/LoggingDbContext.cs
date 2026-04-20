// src/LoggingCore/Data/LoggingDbContext.cs
using SharedKernel.LoggingCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace SharedKernel.LoggingCore.Data;

public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options) { }

    public DbSet<ApiLog> ApiLogs => Set<ApiLog>();
    public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();
    public DbSet<PerformanceMetric> PerformanceMetrics => Set<PerformanceMetric>();
    public DbSet<QueryLog> QueryLogs => Set<QueryLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApiLog indexes
        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RequestTimestamp);
            entity.HasIndex(e => new { e.Method, e.Path, e.StatusCode });
        });

        // ExceptionLog indexes
        modelBuilder.Entity<ExceptionLog>(entity =>
        {
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.ExceptionType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
        });

        // PerformanceMetric indexes
        modelBuilder.Entity<PerformanceMetric>(entity =>
        {
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.OperationName);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IsSlowOperation);
        });

        // QueryLog indexes
        modelBuilder.Entity<QueryLog>(entity =>
        {
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IsSlowQuery);
            entity.HasIndex(e => new { e.Database, e.Timestamp });
        });

    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
