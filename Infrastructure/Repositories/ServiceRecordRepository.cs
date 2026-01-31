using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;

public class ServiceRecordRepository : IServiceRecordRepository
{
    private readonly CosmosConnectionFactory _factory;
    private readonly BlobConnectionFactory _blobFactory;

    public ServiceRecordRepository(
        CosmosConnectionFactory factory,
        BlobConnectionFactory blobFactory
    )
    {
        _factory = factory;
        _blobFactory = blobFactory;
    }

    public async Task<Record> GetByIdAsync(string id)
    {
        try
        {
            // Create a new Cosmos DB client
            var container = _factory.GetContainer(CosmosContainer.Records.GetName());
            // Query by id
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter(
                "@id",
                id
            );

            using FeedIterator<Record> iterator = container.GetItemQueryIterator<Record>(query);

            if (iterator.HasMoreResults)
            {
                FeedResponse<Record> response = await iterator.ReadNextAsync();
                return response.Resource.FirstOrDefault();
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Graceful "not found"
            return null;
        }
    }

    public async Task<string> FileUploadAsync(IFormFile file)
    {
        try
        {
            var container = _blobFactory.GetContainer(BlobContainer.Receipts.GetName());

            // Generate a unique file name to prevent overwriting
            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

            // Get a reference to the blob
            var blobClient = container.GetBlobClient(uniqueFileName);

            // Determine content type based on file extension
            string contentType = GetContentType(file.FileName);

            // Upload the file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(
                    stream,
                    new BlobHttpHeaders { ContentType = contentType }
                );
            }

            // Return the public URL of the uploaded file
            return blobClient.Uri.ToString();
            // var url = _blobFactory.GetBlobUrl(BlobContainer.Receipts.GetName(), uniqueFileName);
            // return url;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file to Blob Storage: {ex.Message}");
            throw;
        }
    }

    public async Task<Record> Update(string id, Record updates)
    {
        var container = _factory.GetContainer(CosmosContainer.Records.GetName());

        var operations = new List<PatchOperation>();

        if (updates.ServicedDate != null)
            operations.Add(PatchOperation.Set("/ServicedDate", updates.ServicedDate));

        if (updates.MechanicName != null)
            operations.Add(PatchOperation.Set("/MechanicName", updates.MechanicName));

        if (updates.Odometer != null)
            operations.Add(PatchOperation.Set("/Odometer", updates.Odometer));

        if (updates.ServiceCategory != null)
            operations.Add(PatchOperation.Set("/ServiceCategory", updates.ServiceCategory));

        if (updates.ServiceType != null)
            operations.Add(PatchOperation.Set("/ServiceType", updates.ServiceType));

        if (updates.ServiceOption != null)
            operations.Add(PatchOperation.Set("/ServiceOption", updates.ServiceOption));

        if (updates.Comment != null)
            operations.Add(PatchOperation.Set("/Comment", updates.Comment));

        if (updates.FileUrls != null)
            operations.Add(PatchOperation.Set("/FileUrls", updates.FileUrls));

        var response = await container.PatchItemAsync<Record>(
            id,
            new PartitionKey(updates.TagId),
            operations
        );

        return response.Resource;
    }

    // Helper method to get the correct content type based on file extension
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            _ => "application/octet-stream", // Default for unknown file types
        };
    }

    public async Task<bool> Add(Record record)
    {
        try
        {
            var _container = _factory.GetContainer(CosmosContainer.Records.GetName());
            ItemResponse<Record> response = await _container.CreateItemAsync(
                record,
                new PartitionKey(record.TagId)
            );
            return true; // Returning the inserted record
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            throw;
        }
    }
}
