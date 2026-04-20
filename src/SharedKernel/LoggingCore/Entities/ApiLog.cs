// src/LoggingCore/Entities/ApiLog.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.LoggingCore.Entities;

[Table("ApiLogs", Schema = "Logging")]
public class ApiLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string TraceId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Method { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Path { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? QueryString { get; set; }

    public string? RequestBody { get; set; }

    [MaxLength(50)]
    public string? UserId { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public string? ResponseBody { get; set; }

    [Required]
    public int StatusCode { get; set; }

    public long RequestDurationMs { get; set; }

    [Required]
    public DateTime RequestTimestamp { get; set; }

    public DateTime ResponseTimestamp { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(100)]
    public string? MachineName { get; set; }

    [MaxLength(500)]
    public string? ExceptionMessage { get; set; }

    [MaxLength(100)]
    public string? ExceptionType { get; set; }
}
