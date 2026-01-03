namespace SharedKernel;

/// <summary>
/// Represents a void return type for commands that don't return a specific value.
/// Used as the response type for ICommand (without a type parameter).
/// Equivalent to void but can be used in generic contexts.
/// </summary>
public sealed record Unit
{
    /// <summary>
    /// Singleton instance of Unit.
    /// </summary>
    public static readonly Unit Value = new();

    private Unit() { }
}
