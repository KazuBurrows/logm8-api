using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LogMate.Application.Services;

public class ServiceRecordService : IServiceRecordService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/heic", "image/heif", "application/pdf",
    };
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".heif", ".pdf",
    };

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

        var tokenInfo = await _nfcTagService.GetOneLifeTokenAsync(request.Token);
        if (tokenInfo is null)
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid or expired token");

        var record = new Record
        {
            id = Guid.NewGuid().ToString(),
            Token = request.Token,
            TagId = tokenInfo.LogId,
            UserId = tokenInfo.UserId,
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

    public async Task<Record> AddServiceRecordAsync(IFormCollection form)
    {
        string token = form["Token"];
        if (string.IsNullOrEmpty(token))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing token");

        var tokenInfo = await _nfcTagService.GetOneLifeTokenAsync(token);
        if (tokenInfo is null)
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid or expired token");

        var fileUrls = await ValidateAndUploadFilesAsync(form.Files);

        var record = new Record
        {
            id = Guid.NewGuid().ToString(),
            Token = token,
            TagId = tokenInfo.LogId,
            UserId = tokenInfo.UserId,
            EnteredDate = form["EnteredDate"],
            ServicedDate = form["ServicedDate"],
            MechanicName = form["MechanicName"],
            Odometer = form["Odometer"],
            ServiceCategory = form["ServiceCategory"],
            ServiceType = form["ServiceType"],
            ServiceOption = form["ServiceOption"],
            Comment = form["Comment"].ToString() ?? string.Empty,
            FileUrls = fileUrls,
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

    public Task<Record?> GetById(string id)
    {
        return _repo.GetByIdAsync(id);
    }

    public Task<Record> Update(Record r)
    {
        throw new NotImplementedException();
    }

    public Task<Record> UpdateServiceRecordAsync(IFormCollection record) =>
        UpdateServiceRecordAsync(record["Id"], record);

    public async Task<Record> UpdateServiceRecordAsync(string id, IFormCollection record)
    {
        if (string.IsNullOrEmpty(id))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing record Id");

        string token = record["Token"];
        if (string.IsNullOrEmpty(token))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing token");

        var tokenInfo = await _nfcTagService.GetOneLifeTokenAsync(token);
        if (tokenInfo is null)
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid NFC token");

        string enteredDate = record["EnteredDate"];
        string servicedDate = record["ServicedDate"];
        string mechanicName = record["MechanicName"];
        string odometer = record["Odometer"];
        string serviceCategory = record["serviceCategory"];
        string serviceOption = record["serviceOption"];
        string serviceType = record["ServiceType"];
        string comment = record["Comment"];

        var newFileUrls = await ValidateAndUploadFilesAsync(record.Files);

        var existingRecord = await _repo.GetByIdAsync(id, tokenInfo.LogId);
        if (existingRecord is null)
            throw new ApiException(HttpStatusCode.NotFound, "Service record not found");

        if (string.IsNullOrEmpty(existingRecord.UserId) || existingRecord.UserId != tokenInfo.UserId)
            throw new ForbiddenException("You do not have permission to edit this service record.");

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

    private async Task<List<string>> ValidateAndUploadFilesAsync(IFormFileCollection files)
    {
        var urls = new List<string>();
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedContentTypes.Contains(file.ContentType) || !AllowedExtensions.Contains(ext))
                throw new UnsupportedMediaTypeException(
                    $"File type not allowed: {file.FileName}. Only images and PDFs are accepted."
                );

            urls.Add(await _repo.FileUploadAsync(file));
        }
        return urls;
    }
}
