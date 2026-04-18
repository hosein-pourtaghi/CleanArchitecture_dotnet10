// src/SharedKernel/Exceptions/ConflictException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

[Serializable]
public class ConflictException : BaseException
{
    public ConflictException(string code, string message)
        : base(code, message, ErrorType.Conflict) { }

    public ConflictException(string message)
        : base("CONFLICT.ERROR", message, ErrorType.Conflict) { }

    protected ConflictException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    protected string GetTypeUri() => "https://tools.ietf.org/html/rfc7231#section-6.5.8";
    protected int GetHttpStatusCode() => 409;
    protected string GetUserFriendlyMessage() => "تغییرات شما با وضعیت فعلی سیستم مغایرت دارد.";
}
