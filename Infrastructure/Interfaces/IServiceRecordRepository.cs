using Microsoft.AspNetCore.Http;

public interface IServiceRecordRepository
{
    Task<Record> GetByIdAsync(string id);
    Task<string> FileUploadAsync(IFormFile file);
    Task<bool> Add(Record record);
    Task<Record> Update(string id, Record updates);
}