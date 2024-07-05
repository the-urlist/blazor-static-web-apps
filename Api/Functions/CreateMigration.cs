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

        var hashedUserName = hasher.HashString(clientPrincipal.UserDetails);

        // check to see if a migration already exists for this user with this identity provider that has not been completed
        var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @username AND c.identityProvider = @identityProvider AND c.completed = false")
          .WithParameter("@username", hashedUserName)
          .WithParameter("@identityProvider", clientPrincipal.IdentityProvider);

        var iterator = container.GetItemQueryIterator<Migration>(query);
        var migrations = await iterator.ReadNextAsync();

        var migrationResponse = new MigrationWrapper();
        migrationResponse.MigrationSiteURL = Environment.GetEnvironmentVariable("MIGRATION_SITE_URL");

        if (migrations.Any<Migration>())
        {
          migrationResponse.Migration = migrations.First<Migration>();
        }
        else
        {
          var newMigration = new Migration
          {
            Id = Guid.NewGuid().ToString(),
            Username = hashedUserName,
            IdentityProvider = clientPrincipal.IdentityProvider,
            Completed = false
          };

          var response = await container.CreateItemAsync(newMigration);
          newMigration = response.Resource;

          migrationResponse.Migration = newMigration;
        }

        var responseMessage = req.CreateResponse(HttpStatusCode.Created);
        await responseMessage.WriteAsJsonAsync(migrationResponse);

        return responseMessage;
      }
      catch (Exception ex)
      {
        return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      }
    }
  }
}
