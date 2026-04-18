// src/SharedKernel/Exceptions/ForbiddenException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

[Serializable]
public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access forbidden")
        : base("AUTH.FORBIDDEN", message, ErrorType.Forbidden) { }

    public ForbiddenException(string code, string message)
        : base(code, message, ErrorType.Forbidden) { }

    protected ForbiddenException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    protected   string GetTypeUri() => "https://tools.ietf.org/html/rfc7231#section-6.5.3";
    protected   int GetHttpStatusCode() => 403;
    protected  string GetUserFriendlyMessage() => "شما مجوز دسترسی به این منبع را ندارید.";
}
