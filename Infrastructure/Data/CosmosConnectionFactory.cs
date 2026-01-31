using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

public class CosmosConnectionFactory
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;

    public CosmosConnectionFactory(IConfiguration configuration)
    {
        var endpoint =
            configuration["CosmosDb_Endpoint"]
            ?? throw new InvalidOperationException("Cosmos endpoint not configured");

        var key =
            configuration["CosmosDb_PrimaryKey"]
            ?? throw new InvalidOperationException("Cosmos primary key not configured");

        _databaseName =
            configuration["CosmosDb_DatabaseName"]
            ?? throw new InvalidOperationException("Cosmos database name not configured");

        _client = new CosmosClient(endpoint, key);
    }

    public Container GetContainer(string containerName) =>
        _client.GetContainer(_databaseName, containerName);
}
