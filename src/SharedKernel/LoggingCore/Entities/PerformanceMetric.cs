// src/LoggingCore/Entities/PerformanceMetric.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoggingCore.Entities;

[Table("PerformanceMetrics", Schema = "Logging")]
public class PerformanceMetric
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string TraceId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string OperationName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? UserId { get; set; }

    [MaxLength(2000)]
    public string? RequestPath { get; set; }

    public long DurationMs { get; set; }

    public long MemoryUsedBytes { get; set; }

    public int CpuTimeMs { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public bool IsSlowOperation { get; set; }

    [MaxLength(100)]
    public string? MachineName { get; set; }

    public string? Metadata { get; set; }
}
