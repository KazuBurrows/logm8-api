public interface INfcTagRepository
{
    Task<bool> UpdateNfcTagAsync(Tag tag);
    Task<int> ReplaceAllRecordsForTagAsync(string oldTagId, string newTagId);
    Task<string> GetNfcTagIdByUriTokenAsync(string uriToken);
}