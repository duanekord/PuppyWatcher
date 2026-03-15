var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sqldata");

var puppyDb = sqlServer.AddDatabase("puppydb");
var identityDb = sqlServer.AddDatabase("identitydb");

var apiService = builder.AddProject<Projects.PuppyWeightWatcher_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(puppyDb)
    .WaitFor(puppyDb);

var webfrontend = builder.AddProject<Projects.PuppyWeightWatcher_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(identityDb)
    .WaitFor(apiService)
    .WaitFor(identityDb);

// Only use Key Vault when publishing to Azure; locally, user secrets provide auth config
if (builder.ExecutionContext.IsPublishMode)
{
    var keyVault = builder.AddAzureKeyVault("keyvault");
    webfrontend.WithReference(keyVault);
}

builder.Build().Run();
