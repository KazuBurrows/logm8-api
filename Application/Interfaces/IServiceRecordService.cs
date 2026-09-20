using LogMate.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace LogMate.Application.Interfaces;

public interface IServiceRecordService
{
    Task<Record> AddServiceRecordAsync(
        ServiceRecordRequest request,
        IReadOnlyList<string>? fileUrls = null
    );
    Task<Record> AddServiceRecordAsync(IFormCollection form);
    Task<Record> UpdateServiceRecordAsync(IFormCollection record);
    Task<Record> UpdateServiceRecordAsync(string id, IFormCollection record);

    Task<Record?> GetById(string id);
    Task<IEnumerable<Record>> GetAll();
    Task<Record> Create(Record r);
    Task<bool> Delete(int id);
    Task<Record> Update(Record r);
}
