// FileStorage.Infrastructure/Persistence/FileStorageDbContext.cs
using System.Collections.Generic;
using System.Reflection.Emit;
using FileStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Persistence;

public class FileStorageDbContext : DbContext
{
    public FileStorageDbContext(DbContextOptions<FileStorageDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<FileHistory> FileHistories => Set<FileHistory>();
    public DbSet<FileAccessLog> FileAccessLogs => Set<FileAccessLog>();
    public DbSet<FileAccessPermission> FileAccessPermissions => Set<FileAccessPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileStorageDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
