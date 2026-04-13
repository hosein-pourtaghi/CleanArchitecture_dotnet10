namespace SharedKernel;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static readonly Error NullValue = new(
        "GENERAL.NULL",
        "Null value was provided",
        ErrorType.Failure);

    //public static Error None2 => new(string.Empty, string.Empty, ErrorType.Failure);

    public Error(string code, string description, ErrorType type = ErrorType.Failure)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ErrorType Type { get; init; } = ErrorType.Failure;

    // Factory methods
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error NotFound(string entityName, Guid? id = null) =>
        new($"{entityName.ToUpperInvariant()}.NOT_FOUND",
            $"'{entityName}' with id '{id?.ToString() ?? "unknown"}' was not found",
            ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Unauthorized(string description = "Unauthorized access") =>
        new("AUTH.UNAUTHORIZED", description, ErrorType.Unauthorized);
}
