using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

public class BlobConnectionFactory
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _baseUrl;

    public BlobConnectionFactory(IConfiguration configuration)
    {
        var connectionString =
            configuration["BlobStorage_ConnectionString"]
            ?? configuration.GetConnectionString("BlobStorage")
            ?? throw new InvalidOperationException("Blob storage connection string not configured");

        _baseUrl =
            configuration["BlobStorage_BaseUrl"]
            ?? throw new InvalidOperationException("Blob storage base URL not configured");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public BlobContainerClient GetContainer(string containerName) =>
        _blobServiceClient.GetBlobContainerClient(containerName);

    public string GetBlobUrl(string containerName, string blobName) =>
        $"{_baseUrl.TrimEnd('/')}/{containerName}/{blobName}";
}
