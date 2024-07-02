using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
  public partial class CreateMigration(CosmosClient cosmosClient, Hasher hasher)
  {

    [Function(nameof(CreateMigration))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "migration")] HttpRequestData req,
        FunctionContext executionContext)
    {
      var logger = executionContext.GetLogger("CreateMigration");
      logger.LogInformation("C# HTTP trigger function processed a request.");

      ClientPrincipal clientPrincipal = ClientPrincipalUtility.GetClientPrincipal(req);

      if (clientPrincipal == null)
      {
        return await req.CreateJsonResponse(HttpStatusCode.Unauthorized, "Unauthorized");
      }

      try
      {
        var databaseName = Environment.GetEnvironmentVariable("CosmosDb__Database");

        var container = cosmosClient.GetContainer(databaseName, "migrations");

        // check to see if a migration already exists for this user with this identity provider that has not been completed
        var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @username AND c.identityProvider = @identityProvider AND c.completed = false")
          .WithParameter("@username", clientPrincipal.UserDetails)
          .WithParameter("@identityProvider", clientPrincipal.IdentityProvider);

        var iterator = container.GetItemQueryIterator<Migration>(query);
        var migrations = await iterator.ReadNextAsync();

        var migrationDocument = new Migration();

        if (migrations.Any<Migration>())
        {
          migrationDocument = migrations.First<Migration>();
        }
        else
        {
          migrationDocument = new Migration
          {
            Id = Guid.NewGuid().ToString(),
            Username = clientPrincipal.UserDetails,
            IdentityProvider = clientPrincipal.IdentityProvider,
            Completed = false
          };

          var response = await container.CreateItemAsync(migrationDocument);
          migrationDocument = response.Resource;
        }

        var responseMessage = req.CreateResponse(HttpStatusCode.Created);
        await responseMessage.WriteAsJsonAsync(migrationDocument);

        return responseMessage;
      }
      catch (Exception ex)
      {
        return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }
  }
}
