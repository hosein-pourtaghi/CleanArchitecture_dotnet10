namespace SharedKernel;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// Used for type constraint in MediatR pipeline behaviors.
/// </summary>
public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    Error Error { get; }
}
