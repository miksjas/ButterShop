var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var shopDb = postgres.AddDatabase("shopdb");

builder.AddProject<Projects.ButterShop_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithReference(shopDb)
    .WaitFor(shopDb)
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Scalar (HTTPS)";
        url.Url = "/scalar";
    });

builder.Build().Run();
