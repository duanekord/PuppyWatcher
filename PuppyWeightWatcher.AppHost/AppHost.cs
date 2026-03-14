var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sqldata");

var puppyDb = sqlServer.AddDatabase("puppydb");
var identityDb = sqlServer.AddDatabase("identitydb");

// External auth provider secrets (azd will prompt and persist these)
var msClientId = builder.AddParameter("auth-microsoft-clientid");
var msClientSecret = builder.AddParameter("auth-microsoft-clientsecret", secret: true);

var apiService = builder.AddProject<Projects.PuppyWeightWatcher_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(puppyDb)
    .WaitFor(puppyDb);

builder.AddProject<Projects.PuppyWeightWatcher_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(identityDb)
    .WithEnvironment("Authentication__Microsoft__ClientId", msClientId)
    .WithEnvironment("Authentication__Microsoft__ClientSecret", msClientSecret)
    .WaitFor(apiService)
    .WaitFor(identityDb);

builder.Build().Run();
