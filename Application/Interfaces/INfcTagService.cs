using System.Net;
using LogMate.Domain.Models;

namespace LogMate.Application.Interfaces;

public interface INfcTagService
{
    Task<bool> UpdateAssetNfcTagAsync(UpdateAssetNfcTagRequest request);
    Task<HttpStatusCode> ReplaceAssetNfcTagAsync(ReplaceNfcTagRequest payload);
    Task<string> GetNfcTagIdByUriTokenAsync(string uriToken);
}
