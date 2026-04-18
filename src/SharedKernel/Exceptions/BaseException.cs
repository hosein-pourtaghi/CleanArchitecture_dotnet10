// src/SharedKernel/Exceptions/BaseException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

/// <summary>
/// Base exception class for all domain exceptions
/// </summary>
[Serializable]
public class BaseException : Exception
{
    public string Code { get; }
    public ErrorType ErrorType { get; }
    public Dictionary<string, object> Metadata { get; } = new();
    public string? CorrelationId { get; set; }

    protected BaseException(string code, string message, ErrorType errorType)
        : base(message)
    {
        Code = code;
        ErrorType = errorType;
    }

    protected BaseException(string code, string message, ErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        ErrorType = errorType;
    }

    protected BaseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Code = info.GetString(nameof(Code)) ?? "UNKNOWN";
        ErrorType = (ErrorType)info.GetInt32(nameof(ErrorType));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Code), Code);
        info.AddValue(nameof(ErrorType), (int)ErrorType);
    }

    public string GetTypeUri() => "https://tools.ietf.org/html/rfc7231#section-6.6.1";

    public virtual int GetHttpStatusCode() => ErrorType switch
    {
        ErrorType.Validation => 400,
        ErrorType.NotFound => 404,
        ErrorType.Unauthorized => 401,
        ErrorType.Forbidden => 403,
        ErrorType.Conflict => 409,
        _ => 500
    };

    public virtual string GetUserFriendlyMessage() => Message;

    /// <summary>
    /// Convert to problem details (call this in ASP.NET Core layer)
    /// </summary>
    public ProblemDetailsDto ToProblemDetails(bool showDetails = false)
    {
        return new ProblemDetailsDto
        {
            Type = GetTypeUri(),
            Title = ErrorType.ToString(),
            Status = GetHttpStatusCode(),
            Detail = showDetails ? Message : GetUserFriendlyMessage(),
            Code = Code,
            TraceId = CorrelationId
        };
    }
}

/// <summary>
/// Simple DTO for problem details (no ASP.NET Core dependency)
/// </summary>
public class ProblemDetailsDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Code { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, object?> Extensions { get; set; } = new();
}
