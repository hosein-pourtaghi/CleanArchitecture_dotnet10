// src/LoggingCore/Mapping/ExceptionResponseMapper.cs
// No MVC dependency - returns raw response data

using System.Text.Json;
using System.Text.Json.Serialization;
using SharedKernel.Exceptions;

namespace LoggingCore.Mapping;

/// <summary>
/// Maps exceptions to HTTP response format
/// </summary>
public static class ExceptionResponseMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Map exception to HTTP status code
    /// </summary>
    public static int GetStatusCode(this Exception exception)
    {
        return exception switch
        {
            MyValidationException => 400,
            NotFoundException => 404,
            UnauthorizedException => 401,
            ForbiddenException => 403,
            ConflictException => 409,
            BusinessRuleException => 400,
            DomainException => 400,
            _ => 500
        };
    }

    /// <summary>
    /// Map exception to response body
    /// </summary>
    public static BaseResponse ToResponse(this Exception exception, bool showDetails = false)
    {
        return exception switch
        {
            MyValidationException validationEx => validationEx.ToValidationResponse(showDetails),
            BaseException baseEx => baseEx.ToBaseResponse(showDetails),
            _ => exception.ToGenericResponse(showDetails)
        };
    }

    private static ValidationResponse ToValidationResponse(this MyValidationException exception, bool showDetails)
    {
        return new ValidationResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = showDetails ? "Validation failed" : "لطفاً ورودی‌های خود را بررسی کرده و تصحیح کنید.",
            Status = 400,
            Code = exception.Code,
            TraceId = exception.CorrelationId,
            Errors = exception.Errors.Select(e => new ValidationErrorItem
            {
                Field = e.Field,
                Code = e.Code,
                Message = e.Message
            }).ToList()
        };
    }

    private static BaseResponse ToBaseResponse(this BaseException exception, bool showDetails)
    {
        return new BaseResponse
        {
            Type = exception.GetTypeUri(),
            Title = showDetails ? exception.Message : exception.GetUserFriendlyMessage(),
            Status = exception.GetHttpStatusCode(),
            Code = exception.Code,
            TraceId = exception.CorrelationId,
            Detail = showDetails ? exception.Message : null
        };
    }

    private static BaseResponse ToGenericResponse(this Exception exception, bool showDetails)
    {
        return new BaseResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = showDetails ? exception.Message : "خطای غیرمنتظره‌ای رخ داده است.",
            Status = 500,
            Code = "INTERNAL.ERROR",
            Detail = showDetails ? exception.Message : null
        };
    }

    /// <summary>
    /// Serialize response to JSON bytes
    /// </summary>
    public static byte[] ToJsonBytes(this BaseResponse response)
    {
        return JsonSerializer.SerializeToUtf8Bytes(response, JsonOptions);
    }
}

// Response DTOs
public class BaseResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Code { get; set; }
    public string? TraceId { get; set; }
    public string? Detail { get; set; }
}

public class ValidationResponse : BaseResponse
{
    public List<ValidationErrorItem> Errors { get; set; } = new();
}

public class ValidationErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
