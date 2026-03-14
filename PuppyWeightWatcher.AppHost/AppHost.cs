var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sqldata");

var puppyDb = sqlServer.AddDatabase("puppydb");

var apiService = builder.AddProject<Projects.PuppyWeightWatcher_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(puppyDb)
    .WaitFor(puppyDb);

builder.AddProject<Projects.PuppyWeightWatcher_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
