var builder = DistributedApplication.CreateBuilder(args);

var webap = builder.AddProject<Projects.WebApi>("WebApi");

builder.AddProject<Projects.NotificationAPI>("notificationapi");

builder.Build().Run();
