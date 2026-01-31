using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public interface IServiceRecordService
{
    Task<bool> AddServiceRecordAsync(
        ServiceRecordRequest request,
        IReadOnlyList<string>? fileUrls = null
    );

    Task<bool?> UpdateServiceRecordAsync(IFormCollection record);
}
