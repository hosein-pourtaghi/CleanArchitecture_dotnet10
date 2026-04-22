var builder = DistributedApplication.CreateBuilder(args);

var webap = builder.AddProject<Projects.WebApi>("WebApi");

var notif = builder.AddProject<Projects.NotificationAPI>("notificationapi");
var loader = builder.AddProject<Projects.LoadSimulator>("LoadSimulator")
    .WithReference(webap) ;

builder.AddProject<Projects.FileStorage_Api>("filestorage-api");

builder.Build().Run();
