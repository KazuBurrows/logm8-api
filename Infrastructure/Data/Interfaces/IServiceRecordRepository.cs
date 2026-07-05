using LogMate.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace LogMate.Infrastructure.Data.Interfaces;

public interface IServiceRecordRepository
{
    Task<Record> GetByIdAsync(string id, string tagId);
    Task<string> FileUploadAsync(IFormFile file);
    Task<Record> Add(Record record);
    Task<Record> Update(string id, Record updates);
}
