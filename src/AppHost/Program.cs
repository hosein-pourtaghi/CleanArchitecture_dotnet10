using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add the web API service
// var webApi = builder.AddProject<Projects.WebApi>("webapi");
builder.AddProject("webapi", "../WebApi/WebApi.csproj");

builder.Build().Run();
