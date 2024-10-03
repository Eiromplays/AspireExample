var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", builder.CreateStableUsername("postgres-username"), builder.CreateStablePassword("postgres-password"));
var apiDb = postgres.AddDatabase("apidb");

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.AspireExample_ApiService>("apiservice")
    .WithReference(apiDb)
    .WithReference(cache);

builder.AddProject<Projects.AspireExample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(cache);

builder.Build().Run();
