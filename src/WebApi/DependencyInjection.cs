using Microsoft.OpenApi;
using WebApi.Infrastructure;
using WebApi.Telemetry;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using WebApi.Http;


namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
       {
           options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));
           options.SwaggerDoc("v1", new OpenApiInfo
           {
               Version = "v1",
               Title = "WebApi",
               Description = "HSE project",
               //    TermsOfService = new Uri("https://my.msc.com"),
               //    Contact = new OpenApiContact
               //    {
               //        Name = "MSC TEAM",
               //        Url = new Uri("https://msc.com/")
               //    }
           });

           options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
           {
               Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
               Name = "Authorization",
               In = ParameterLocation.Header,
               Type = SecuritySchemeType.Http,
               Scheme = "Bearer"
           });

           options.AddSecurityRequirement((doc) => new OpenApiSecurityRequirement
           {
               {
                   new OpenApiSecuritySchemeReference("Bearer"),
                   new List<string>()
               }
           });

           // Include XML documentation comments
           var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
           var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
           if (File.Exists(xmlPath))
           {
               options.IncludeXmlComments(xmlPath);
           }

           // Document all endpoints
           options.DocInclusionPredicate((_, _) => true);

           // Sort endpoints by route
           options.OrderActionsBy(api => api.RelativePath);
       });


        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.AddProblemDetails();

        // Register ActivitySource for dependency injection if needed
        services.AddSingleton(TelemetryActivitySource.Instance);

        // Configure high-throughput HttpClient(s) using IHttpClientFactory and SocketsHttpHandler
        // - Use typed clients (IExternalHttpClient) for strong typing and testability
        // - Configure SocketsHttpHandler to enable connection pooling and HTTP/2 multiplexing
        // - Default resilience and service-discovery handlers are applied globally by ProjectSetup.ServiceDefaults
        const int defaultMaxConnectionsPerServer = 100;
        //services.AddHttpClient("ExternalService")
        //    .ConfigureHttpClient(client =>
        //    {
        //        client.Timeout = TimeSpan.FromSeconds(30);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    })
        //    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        //    {
        //        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        //        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        //        MaxConnectionsPerServer = defaultMaxConnectionsPerServer,
        //        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        //        EnableMultipleHttp2Connections = true
        //    });

        // Register a typed wrapper for convenient, testable HTTP calls
        services.AddHttpClient<IExternalHttpClient, ExternalHttpClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = defaultMaxConnectionsPerServer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                EnableMultipleHttp2Connections = true
            });

        return services;
    }
}
