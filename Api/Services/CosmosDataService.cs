using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IDataService
    {
        Task<LinkBundle> GetLinkBundle(string vanityUrl);
        Task SaveLinkBundle(LinkBundle linkBundle);
        Task UpdateLinkBundle(LinkBundle linkBundle);
        Task DeleteLinkBundle(LinkBundle linkBundle);
        string GetLinkBundleId(string vanityUrl);
    }

    public class CosmosDataService : IDataService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Microsoft.Azure.Cosmos.Container _container;

        public CosmosDataService(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
            _container = _cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<LinkBundle> GetLinkBundle(string vanityUrl)
        {
            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                            .WithParameter("@vanityUrl", vanityUrl);

            var queryResultSetIterator = _container.GetItemQueryIterator<LinkBundle>(query);

            if (queryResultSetIterator.HasMoreResults)
            {
                var linkBundles = await queryResultSetIterator.ReadNextAsync();
                return linkBundles.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task SaveLinkBundle(LinkBundle linkBundle)
        {
            try
            {
                await _container.CreateItemAsync(linkBundle);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new Exception("Vanity URL already exists");
            }
        }

        public async Task UpdateLinkBundle(LinkBundle linkBundle)
        {
            await _container.ReplaceItemAsync(linkBundle, linkBundle.Id);
        }

        public async Task DeleteLinkBundle(LinkBundle linkBundle)
        {
            var partitionKey = new PartitionKey(linkBundle.VanityUrl);
            await _container.DeleteItemAsync<LinkBundle>(linkBundle.Id, partitionKey);
        }

        public string GetLinkBundleId(string vanityUrl)
        {
            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                            .WithParameter("@vanityUrl", vanityUrl);

            var iterator = _container.GetItemQueryIterator<LinkBundle>(query);

            if(iterator.HasMoreResults)
            {
                var result = iterator.ReadNextAsync().Result;
                return result.FirstOrDefault().Id;
            }
            else
            {
                return null;
            }
        }
    }
}