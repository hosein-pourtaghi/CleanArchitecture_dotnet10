var builder = DistributedApplication.CreateBuilder(args);

var webap = builder.AddProject<Projects.WebApi>("WebApi");

var notif = builder.AddProject<Projects.NotificationAPI>("notificationapi");
 

builder.AddProject<Projects.FileStorage_Api>("filestorage-api");

builder.AddProject<Projects.IdentityApi>("identityapi");

builder.Build().Run();
