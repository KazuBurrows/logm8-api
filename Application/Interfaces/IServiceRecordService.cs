using LogMate.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace LogMate.Application.Interfaces;

public interface IServiceRecordService
{
    Task<Record> AddServiceRecordAsync(
        ServiceRecordRequest request,
        IReadOnlyList<string>? fileUrls = null
    );
    Task<Record> UpdateServiceRecordAsync(IFormCollection record);



    Task<Record> GetById(int id);
    Task<IEnumerable<Record>> GetAll();
    Task<Record> Create(Record r);
    Task<bool> Delete(int id);
    Task<Record> Update(Record r);
}
