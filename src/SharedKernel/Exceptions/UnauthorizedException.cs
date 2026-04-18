// src/SharedKernel/Exceptions/UnauthorizedException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

[Serializable]
public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base("AUTH.UNAUTHORIZED", message, ErrorType.Unauthorized) { }

    public UnauthorizedException(string code, string message)
        : base(code, message, ErrorType.Unauthorized) { }

    protected UnauthorizedException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    protected   string GetTypeUri() => "https://tools.ietf.org/html/rfc7235#section-3.1";
    protected   int GetHttpStatusCode() => 401;

    protected   string GetUserFriendlyMessage() =>
        "لطفاً برای دسترسی به این منبع احراز هویت شوید.";
}
