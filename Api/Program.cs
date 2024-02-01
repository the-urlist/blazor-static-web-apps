using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

// Setup custom serializer to use System.Text.Json
JsonSerializerOptions jsonSerializerOptions = new()
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
CosmosSystemTextJsonSerializer cosmosSystemTextJsonSerializer = new(jsonSerializerOptions);
CosmosClientOptions cosmosClientOptions = new()
{
    ApplicationName = "SystemTextJson",
    Serializer = cosmosSystemTextJsonSerializer
};

var host = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<CosmosClient>(sp => new CosmosClient(
            context.Configuration["CosmosDb:Endpoint"],
            context.Configuration["CosmosDb:Key"],
            cosmosClientOptions));
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

await host.RunAsync();