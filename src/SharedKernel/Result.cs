using System.Diagnostics.CodeAnalysis;

namespace SharedKernel;

/// <summary>
/// Base interface for Result types
/// </summary>
public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    Error Error { get; }
}

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

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // Factory methods
    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(string error) => new(false, new Error("", error));
    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(string code, string description) =>
        new(false, new Error(code, description));

    public static Result<TValue> Failure<TValue>(string error) => new(default!, false, new Error("", error));
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);

    public static Result<TValue> Failure<TValue>(string code, string description) =>
        new(default!, false, new Error(code, description));

    // Functional methods
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    public Result Tap(Action onSuccess, Action<Error>? onFailure = null)
    {
        if (IsSuccess)
            onSuccess();
        else if (onFailure != null)
            onFailure(Error);
        return this;
    }

    public Result Tap(Func<Result> action)
    {
        if (IsSuccess)
            return action();
        return this;
    }
}

// ==================== Generic Result<T> ====================
public class Result<T> : Result, IResult<T>
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public new bool IsSuccess => base.IsSuccess;
    public new bool IsFailure => base.IsFailure;
    public new Error Error => base.Error;

    /// <summary>
    /// Gets the value or throws if failure
    /// </summary>
    [NotNull]
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access Value of failed result. Error: [{Error.Code}] {Error.Description}");

    /// <summary>
    /// Safe access to value - returns default on failure
    /// </summary>
    public T ValueOrDefault => _value!;

    /// <summary>
    /// Implicit conversion from T to Result<T>
    /// </summary>
    public static implicit operator Result<T>(T? value) =>
        value is not null ? Success(value) : Failure<T>(Error.NullValue);

    // Factory methods
    public static new Result<T> Success(T value) => new(value, true, Error.None);

    public static new Result<T> Failure(Error error) => new(default!, false, error);

    public static new Result<T> Failure(string code, string description) =>
        new(default!, false, new Error(code, description));

    public static Result<T> ValidationFailure(Error error) =>
        new(default!, false, error);

    public static Result<T> ValidationFailure(string code, string description) =>
        new(default!, false, new Error(code, description, ErrorType.Validation));

    // Functional methods
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error);

    public Result Tap(Action<T> onSuccess, Action<Error>? onFailure = null)
    {
        if (IsSuccess)
            onSuccess(_value!);
        else if (onFailure != null)
            onFailure(Error);
        return this;
    }

    //public Result<T> Tap(Func<T, Result> action)
    //{
    //    if (IsSuccess)
    //        return action(_value);
    //    return this;
    //}

    public Result Map<TOutput>(Func<T, TOutput> mapper) =>
        IsSuccess ? Result.Success(mapper(_value!)) : Result.Failure<TOutput>(Error);

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        if (IsSuccess && predicate(_value!))
            return this;
        return Failure<T>(error);
    }
}

public interface IResult<out T> : IResult
{
    T Value { get; }
    T ValueOrDefault { get; }
}
