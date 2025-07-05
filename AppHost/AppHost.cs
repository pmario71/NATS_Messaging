var builder = DistributedApplication.CreateBuilder(args);

// add Redis
builder.AddRedis("cache")
       .WithRedisCommander();

builder.Build().Run();
