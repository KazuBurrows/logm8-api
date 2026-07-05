using Azure.Storage.Blobs.Models;
using LogMate.Domain.Enums;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Data.Interfaces;
using LogMate.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;

namespace LogMate.Infrastructure.Data.Repositories;

public class ServiceRecordRepository : IServiceRecordRepository
{
    private readonly CosmosConnectionFactory _factory;
    private readonly BlobConnectionFactory _blobFactory;

    public ServiceRecordRepository(CosmosConnectionFactory factory, BlobConnectionFactory blobFactory)
    {
        _factory = factory;
        _blobFactory = blobFactory;
    }

    public async Task<Record> GetByIdAsync(string id, string tagId)
    {
        try
        {
            var container = _factory.GetContainer(CosmosContainer.Records.GetName());
            ItemResponse<Record> response = await container.ReadItemAsync<Record>(
                id,
                new PartitionKey(tagId)
            );
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<string> FileUploadAsync(IFormFile file)
    {
        var container = _blobFactory.GetContainer(BlobContainer.Receipts.GetName());

        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var blobClient = container.GetBlobClient(uniqueFileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = GetContentType(file.FileName) }
        );

        return blobClient.Uri.ToString();
    }

    public async Task<Record> Add(Record record)
    {
        var container = _factory.GetContainer(CosmosContainer.Records.GetName());
        ItemResponse<Record> response = await container.CreateItemAsync(
            record,
            new PartitionKey(record.TagId)
        );
        return response.Resource;
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

    private static string GetContentType(string fileName) =>
        Path.GetExtension(fileName).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            _ => "application/octet-stream",
        };
}
