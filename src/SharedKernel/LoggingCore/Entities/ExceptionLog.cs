// src/LoggingCore/Entities/ExceptionLog.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.LoggingCore.Entities;

[Table("Exceptions", Schema = "Logging")]
public class ExceptionLog
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
    public string ExceptionType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public string? InnerException { get; set; }

    [MaxLength(50)]
    public string? UserId { get; set; }

    [MaxLength(2000)]
    public string? RequestPath { get; set; }

    [MaxLength(100)]
    public string? RequestMethod { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    [MaxLength(100)]
    public string MachineName { get; set; } = string.Empty;

    public bool IsHandled { get; set; } = true;

    [MaxLength(50)]
    public string? HandledBy { get; set; }

    public string? AdditionalData { get; set; }
}
