// src/SharedKernel/Exceptions/NotFoundException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

[Serializable]
public class NotFoundException : BaseException
{
    public string? ResourceId { get; }
    public string? ResourceType { get; }

    public NotFoundException(string code, string message)
        : base(code, message, ErrorType.NotFound) { }

    //public NotFoundException(string resourceType, string? resourceId = null)
    //    : base(
    //        $"{resourceType.ToUpperInvariant()}.NOT_FOUND",
    //        $"'{resourceType}' with id '{(resourceId?.ToString() ?? "unknown")}' was not found",
    //        ErrorType.NotFound)
    //{
    //    ResourceType = resourceType;
    //    ResourceId = resourceId;
    //}

    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ResourceId = info.GetString(nameof(ResourceId));
        ResourceType = info.GetString(nameof(ResourceType));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ResourceId), ResourceId);
        info.AddValue(nameof(ResourceType), ResourceType);
    }

    protected   string GetTypeUri() => "https://tools.ietf.org/html/rfc7231#section-6.5.4";
    protected   int GetHttpStatusCode() => 404;
    protected   string GetUserFriendlyMessage() => "منبع درخواستی یافت نشد.";
}
