var builder = DistributedApplication.CreateBuilder(args);

var webap = builder.AddProject<Projects.WebApi>("WebApi");

builder.Build().Run();
