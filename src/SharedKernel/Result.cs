using System.Diagnostics.CodeAnalysis;

namespace SharedKernel;


// ==================== Non-Generic Result ====================
public class Result : IResult
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Successful result cannot have an error", nameof(error));
        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failed result must have an error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; } = false!;
    public bool IsFailure => !IsSuccess;
    public Error Error { get; } = Error.None;

    // Factory methods
    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(string code, string description) =>
        new(false, new Error(code, description));

    public static Result Failure(string description) =>
        new(false, new Error(string.Empty, description));

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
    public static Result<TValue> Failure<TValue>(string error) =>
        new(default, false, new Error("",error,ErrorType.Failure));

    public static Result<TValue> Failure<TValue>(string code, string description) =>
        new(default, false, new Error(code, description));

    // Extension method instead of implicit operator
    //public Result<T> ToResult<T>(this Result result)
    //{
    //    return result.IsSuccess
    //        ? Result<T>.Success(default!)
    //        : Result<T>.Failure(result.Error);
    //}
}


// ==================== Generic Result<T> ====================
public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    // Explicitly hide base properties for clarity
    public new bool IsSuccess => base.IsSuccess;
    public new bool IsFailure => base.IsFailure;
    public new Error Error => base.Error;

    /// <summary>
    /// Gets the value or throws if failure. Use ValueOrDefault for safe access.
    /// </summary>
    [NotNull]
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access Value of failed result. Error: [{Error.Code}] {Error.Description}");

    /// <summary>
    /// Safe access to value - returns default on failure instead of throwing
    /// </summary>
    public T ValueOrDefault => _value;

    /// <summary>
    /// Implicit conversion from T to Result<T>
    /// </summary>
    public static implicit operator Result<T>(T? value) =>
        value is not null ? Success(value) : Failure<T>(Error.NullValue);

    // Factory methods
    public static new Result<T> Success(T value) => new(value, true, Error.None);

    public static new Result<T> Failure(Error error) => new(default, false, error);

    public static new Result<T> Failure(string code, string description) =>
        new(default, false, new Error(code, description));

    public static Result<T> ValidationFailure(Error error) =>
        new(default, false, error);

    public static Result<T> ValidationFailure(string code, string description) =>
        new(default, false, new Error(code, description, ErrorType.Validation));

}
