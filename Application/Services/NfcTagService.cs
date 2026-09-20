using System.Net;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Data.Interfaces;

namespace LogMate.Application.Services;

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

    public async Task<OneLifeTokenInfo?> GetOneLifeTokenAsync(string token)
    {
        return await _repo.GetOneLifeTokenAsync(token);
    }

    public async Task<(HttpStatusCode Status, int MigratedCount)> ReplaceAssetNfcTagAsync(ReplaceNfcTagRequest payload)
    {
        var rowsAffected = await _repo.ReplaceAllRecordsForTagAsync(
            payload.OldTagId,
            payload.NewTagId
        );

        var status = rowsAffected > 0 ? HttpStatusCode.OK : HttpStatusCode.NotFound;
        return (status, rowsAffected);
    }

    public async Task<bool> UpdateAssetNfcTagAsync(UpdateAssetNfcTagRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TagId))
            return false;

        try
        {
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
            Console.WriteLine($"UpdateAssetNfcTagAsync failed: {ex}");
            return false;
        }
    }
}
