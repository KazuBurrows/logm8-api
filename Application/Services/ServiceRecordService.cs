using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LogMate.Application.Services;

public class ServiceRecordService : IServiceRecordService
{
    private readonly INfcTagService _nfcTagService;
    private readonly IServiceRecordRepository _repo;

    public ServiceRecordService(
        IServiceRecordRepository serviceRecordRepository,
        INfcTagService nfcTagService
    )
    {
        _repo = serviceRecordRepository;
        _nfcTagService = nfcTagService;
    }

    public async Task<Record> AddServiceRecordAsync(
        ServiceRecordRequest request,
        IReadOnlyList<string>? fileUrls = null
    )
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        fileUrls ??= Array.Empty<string>();

        var tagId = await _nfcTagService.GetNfcTagIdByUriTokenAsync(request.Token);

        if (string.IsNullOrEmpty(tagId))
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid NFC token");

        var record = new Record
        {
            id = Guid.NewGuid().ToString(),
            Token = request.Token,
            TagId = tagId,
            EnteredDate = request.EnteredDate,
            ServicedDate = request.ServicedDate,
            MechanicName = request.MechanicName,
            Odometer = request.Odometer,
            ServiceCategory = request.ServiceCategory,
            ServiceType = request.ServiceType,
            ServiceOption = request.ServiceOption,
            Comment = request.Comment ?? string.Empty,
            FileUrls = fileUrls.ToList(),
        };

        return await _repo.Add(record);
    }

    public Task<Record> Create(Record r)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Record>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<Record> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Record> Update(Record r)
    {
        throw new NotImplementedException();
    }


    public async Task<Record> UpdateServiceRecordAsync(IFormCollection record)
    {
        string id = record["Id"];
        if (string.IsNullOrEmpty(id))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing record Id");

        string token = record["Token"];
        string tagId = record["TagId"];
        string enteredDate = record["EnteredDate"];
        string servicedDate = record["ServicedDate"];
        string mechanicName = record["MechanicName"];
        string odometer = record["Odometer"];
        string serviceCategory = record["serviceCategory"];
        string serviceOption = record["serviceOption"];
        string serviceType = record["ServiceType"];
        string comment = record["Comment"];

        List<IFormFile> newFiles = new();
        foreach (var file in record.Files)
        {
            using var stream = file.OpenReadStream();
            newFiles.Add(file);
        }

        List<string> newFileUrls = new();
        foreach (var file in newFiles)
        {
            string fileUrl = await _repo.FileUploadAsync(file);
            newFileUrls.Add(fileUrl);
        }

        var existingRecord = await _repo.GetByIdAsync(id, tagId);
        if (existingRecord is null)
            throw new ApiException(HttpStatusCode.NotFound, "Service record not found");

        existingRecord.EnteredDate = enteredDate ?? existingRecord.EnteredDate;
        existingRecord.ServicedDate = servicedDate ?? existingRecord.ServicedDate;
        existingRecord.MechanicName = mechanicName ?? existingRecord.MechanicName;
        existingRecord.Odometer = odometer ?? existingRecord.Odometer;
        existingRecord.ServiceCategory = serviceCategory ?? existingRecord.ServiceCategory;
        existingRecord.ServiceType = serviceType ?? existingRecord.ServiceType;
        existingRecord.ServiceOption = serviceOption ?? existingRecord.ServiceOption;
        existingRecord.Comment = comment ?? existingRecord.Comment;

        if (newFileUrls.Count > 0)
        {
            existingRecord.FileUrls ??= new List<string>();
            existingRecord.FileUrls.AddRange(newFileUrls);
        }

        return await _repo.Update(id, existingRecord);
    }
}
