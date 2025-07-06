var builder = DistributedApplication.CreateBuilder(args);

// add Redis
var redis = builder.AddRedis("cache");
//    .WithRedisCommander();

var nats = builder.AddNats("nats");

builder.AddProject<Projects.WebApp>("WebApp")
    .WithReference(nats);

builder.Build().Run();
