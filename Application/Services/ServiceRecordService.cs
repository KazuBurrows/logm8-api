using Microsoft.AspNetCore.Http;

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

    public async Task<bool> AddServiceRecordAsync(
        ServiceRecordRequest request,
        IReadOnlyList<string>? fileUrls = null
    )
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        fileUrls ??= Array.Empty<string>();

        var tagId = await _nfcTagService.GetNfcTagIdByUriTokenAsync(request.Token);

        if (string.IsNullOrEmpty(tagId))
            throw new InvalidOperationException("Invalid NFC token");

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

    public async Task<bool?> UpdateServiceRecordAsync(IFormCollection? record)
    {
        try
        {
            // Read form fields
            string id = record["Id"]; // Required for updates
            if (string.IsNullOrEmpty(id))
                return false;

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

            // --- Process Files (optional) ---
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

            // --- Fetch existing record from Cosmos ---
            var existingRecord = await _repo.GetByIdAsync(id);
            if (existingRecord is null)
                return false;

            // --- Merge updated values ---
            // existingRecord.Token = token ?? existingRecord.Token;
            // existingRecord.TagId = tagId ?? existingRecord.TagId;
            existingRecord.EnteredDate = enteredDate ?? existingRecord.EnteredDate;
            existingRecord.ServicedDate = servicedDate ?? existingRecord.ServicedDate;
            existingRecord.MechanicName = mechanicName ?? existingRecord.MechanicName;
            existingRecord.Odometer = odometer ?? existingRecord.Odometer;
            existingRecord.ServiceCategory = serviceCategory ?? existingRecord.ServiceCategory;
            existingRecord.ServiceType = serviceType ?? existingRecord.ServiceType;
            existingRecord.ServiceOption = serviceOption ?? existingRecord.ServiceOption;
            existingRecord.Comment = comment ?? existingRecord.Comment;

            // Append new files to existing list if any
            if (newFileUrls.Count > 0)
            {
                existingRecord.FileUrls ??= new List<string>();
                existingRecord.FileUrls.AddRange(newFileUrls);
            }

            // --- Write back to Cosmos ---
            var updated = await _repo.Update(id, existingRecord);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
