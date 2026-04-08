namespace Domain.Entities.Identities;


public class AuditLog
{
    public string UserId { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public string QueryString { get; set; }
    public string Body { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}
