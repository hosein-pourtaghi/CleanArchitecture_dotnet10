// src/LoggingCore/Entities/QueryLog.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.LoggingCore.Entities;

[Table("QueryLogs", Schema = "Logging")]
public class QueryLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string TraceId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    [Required]
    public string Sql { get; set; } = string.Empty;

    public string? Parameters { get; set; }

    [Required]
    public int DurationMs { get; set; }

    [Required]
    [MaxLength(20)]
    public string CommandType { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Database { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public bool IsSlowQuery { get; set; }

    [MaxLength(100)]
    public string? MachineName { get; set; }

    [MaxLength(500)]
    public string? UserId { get; set; }
}
