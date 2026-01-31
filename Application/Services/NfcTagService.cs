using System.Net;

public class NfcTagService : INfcTagService
{
    private readonly INfcTagRepository _repo;

    public NfcTagService(INfcTagRepository nfcTagRepository)
    {
        _repo = nfcTagRepository;
    }

    public async Task<string> GetNfcTagIdByUriTokenAsync(string uriToken)
    {
        return await _repo.GetNfcTagIdByUriTokenAsync(uriToken);
    }

    public async Task<HttpStatusCode> ReplaceAssetNfcTagAsync(ReplaceNfcTagRequest payload)
    {
        var rowsAffected = await _repo.ReplaceAllRecordsForTagAsync(
            payload.OldTagId,
            payload.NewTagId
        );

        // If rowsAffected is returned, operation succeeded
        return rowsAffected > 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
    }

    public async Task<bool> UpdateAssetNfcTagAsync(UpdateAssetNfcTagRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TagId))
            return false;

        try
        {
            // Resolve real NFC TagId from URI token
            var actualTagId = await _repo.GetNfcTagIdByUriTokenAsync(request.TagId);
            if (string.IsNullOrWhiteSpace(actualTagId))
                return false;

            var tag = new Tag
            {
                TagId = actualTagId,
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Vehicle = request.Vehicle,
                Style = request.Style,
                Engine = request.Engine,
                Fuel = request.Fuel,
                Transmission = request.Transmission,
                Color = request.Color,
                VinNumber = request.VinNumber,
                LicencePlate = request.LicencePlate,
                IsConfigured = true,
            };

            return await _repo.UpdateNfcTagAsync(tag);
        }
        catch (Exception ex)
        {
            // Prefer ILogger in real usage
            Console.WriteLine($"UpdateAssetNfcTagAsync failed: {ex}");
            return false;
        }
    }
}
