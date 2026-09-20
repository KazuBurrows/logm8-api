using LogMate.Domain.Models;

namespace LogMate.Infrastructure.Data.Interfaces;

public interface INfcTagRepository
{
    Task<bool> UpdateNfcTagAsync(Tag tag);
    Task<int> ReplaceAllRecordsForTagAsync(string oldTagId, string newTagId);
    Task<string> GetNfcTagIdByUriTokenAsync(string uriToken);
    Task<OneLifeTokenInfo?> GetOneLifeTokenAsync(string token);
}
