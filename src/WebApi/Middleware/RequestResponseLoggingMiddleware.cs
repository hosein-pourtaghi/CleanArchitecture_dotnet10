using System.Text;

namespace WebApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // 1. Log Request (Always log basic info)
        // We enable buffering so we can read the body if needed later
        context.Request.EnableBuffering();

        // Read body to a string (leaveOpen=true so controller can read it)
        string requestBody = await ReadStreamAsync(context.Request.Body);

        _logger.LogInformation("Incoming Request: {Method} {Path} | Body: {Body} | {TraceIdentifier}",
            context.Request.Method,
            context.Request.Path,
            requestBody,
            context.TraceIdentifier);

        // 2. Prepare to capture Response
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context); // Execute the controller logic
        }
        finally
        {
            // 3. Check Status Code
            memoryStream.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode >= 400)
            {
                // ERROR: Read the body and log it
                string responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                _logger.LogWarning("Error Response: {StatusCode} | Body: {Body} | {TraceIdentifier}",
                    context.Response.StatusCode,
                    responseBody, 
                    context.TraceIdentifier);
            }
            else
            {
                // SUCCESS: Skip reading the body to save performance
                // We just need to copy the stream back to the original
            }

            // 4. Copy data back to the original stream (so the client receives the response)
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        stream.Seek(0, SeekOrigin.Begin); // Reset position for the controller
        return body;
    }
}
