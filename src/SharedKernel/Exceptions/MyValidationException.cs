// src/SharedKernel/Exceptions/ValidationException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

/// <summary>
/// Exception for validation errors
/// </summary>
[Serializable]
public class MyValidationException : BaseException
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public MyValidationException(string code, string message)
        : base(code, message, ErrorType.Validation)
    {
        Errors = Array.Empty<ValidationError>();
    }

    public MyValidationException(string code, string message, IEnumerable<ValidationError> errors)
        : base(code, message, ErrorType.Validation)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public MyValidationException(IEnumerable<ValidationError> errors)
        : base("VALIDATION.ERROR", "One or more validation errors occurred", ErrorType.Validation)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    protected MyValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        var errorsArray = info.GetValue(nameof(Errors), typeof(ValidationError[])) as ValidationError[] ?? Array.Empty<ValidationError>();
        Errors = errorsArray.ToList().AsReadOnly();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Errors), Errors.ToArray());
    }

    protected  string GetTypeUri() => "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    protected  int GetHttpStatusCode() => 400;
    protected  string GetUserFriendlyMessage() => "لطفاً ورودی‌های خود را بررسی کرده و تصحیح کنید.";

    public   ProblemDetailsDto ToProblemDetails(bool showDetails = false)
    {
        return new ValidationProblemDetailsDto
        {
            Type = GetTypeUri(),
            Title = showDetails ? "Validation failed" : GetUserFriendlyMessage(),
            Status = GetHttpStatusCode(),
            Code = Code,
            TraceId = CorrelationId,
            Errors = Errors.Select(e => new ValidationErrorDto
            {
                Field = e.Field,
                Code = e.Code,
                Message = e.Message
            }).ToList()
        };
    }

    public static MyValidationException FromFluentValidation(
        IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        var errors = failures.Select(f => new ValidationError(
            f.PropertyName,
            f.ErrorCode,
            f.ErrorMessage));

        return new MyValidationException(errors);
    }

    public static MyValidationException FromResults(IEnumerable<Result> results)
    {
        var errors = results
            .Where(r => r.IsFailure)
            .Select(r => new ValidationError(
                string.Empty,
                r.Error.Code,
                r.Error.Description));

        return new MyValidationException(errors);
    }
}

public class ValidationProblemDetailsDto : ProblemDetailsDto
{
    public List<ValidationErrorDto> Errors { get; set; } = new();
}

public class ValidationErrorDto
{
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Single validation error
/// </summary>
public record ValidationError(string Field, string Code, string Message);
