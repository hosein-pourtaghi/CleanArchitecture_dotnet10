// src/SharedKernel/Exceptions/DomainException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

/// <summary>
/// Base class for domain-specific exceptions
/// </summary>
[Serializable]
public class DomainException : BaseException
{
    public DomainException(string code, string message)
        : base(code, message, ErrorType.Failure) { }

    public DomainException(string code, string message, Exception innerException)
        : base(code, message, ErrorType.Failure, innerException) { }

    protected DomainException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
