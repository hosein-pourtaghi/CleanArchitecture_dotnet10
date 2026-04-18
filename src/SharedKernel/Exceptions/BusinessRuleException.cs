// src/SharedKernel/Exceptions/BusinessRuleException.cs
using System.Runtime.Serialization;

namespace SharedKernel.Exceptions;

[Serializable]
public class BusinessRuleException : BaseException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message)
        : base($"RULE.{ruleName.ToUpperInvariant()}", message, ErrorType.Failure)
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(string ruleName, string code, string message)
        : base(code, message, ErrorType.Failure)
    {
        RuleName = ruleName;
    }

    protected BusinessRuleException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        RuleName = info.GetString(nameof(RuleName)) ?? string.Empty;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(RuleName), RuleName);
    }

    protected   string GetUserFriendlyMessage() => "قانون کسب‌وکار نقض شده است. لطفاً با پشتیبانی تماس بگیرید.";
}
