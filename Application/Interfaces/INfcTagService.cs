using System.Net;

public interface INfcTagService
{
    Task<bool> UpdateAssetNfcTagAsync(UpdateAssetNfcTagRequest request);
    Task<HttpStatusCode> ReplaceAssetNfcTagAsync(ReplaceNfcTagRequest payload);
    Task<string> GetNfcTagIdByUriTokenAsync(string uriToken);
}