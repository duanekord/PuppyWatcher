var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sqldata");

var puppyDb = sqlServer.AddDatabase("puppydb");
var identityDb = sqlServer.AddDatabase("identitydb");

var keyVault = builder.AddAzureKeyVault("keyvault");

var apiService = builder.AddProject<Projects.PuppyWeightWatcher_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(puppyDb)
    .WaitFor(puppyDb);

builder.AddProject<Projects.PuppyWeightWatcher_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(identityDb)
    .WithReference(keyVault)
    .WaitFor(apiService)
    .WaitFor(identityDb);

builder.Build().Run();
