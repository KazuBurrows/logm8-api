using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Application.Services;
using LogMate.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using ServiceRecord = LogMate.Domain.Models.Record;

namespace LogMate.Tests;

public class ServiceRecordServiceTests
{
    private static FormCollection Form(Dictionary<string, string> fields) =>
        new(fields.ToDictionary(kv => kv.Key, kv => new StringValues(kv.Value)));

    [Fact]
    public async Task UpdateServiceRecordAsync_MissingToken_ThrowsBadRequest()
    {
        var nfcTagService = new Mock<INfcTagService>();
        var repo = new Mock<IServiceRecordRepository>();
        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["Id"] = "record-1" });

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.UpdateServiceRecordAsync(form));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.StatusCode);
        nfcTagService.Verify(s => s.GetNfcTagIdByUriTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServiceRecordAsync_InvalidToken_ThrowsBadRequest_AndDoesNotTouchRepo()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetNfcTagIdByUriTokenAsync("bad-token")).ReturnsAsync(string.Empty);
        var repo = new Mock<IServiceRecordRepository>();
        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["Id"] = "record-1", ["Token"] = "bad-token", ["TagId"] = "attacker-supplied-tag" });

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.UpdateServiceRecordAsync(form));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.StatusCode);
        repo.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServiceRecordAsync_ValidToken_IgnoresClientSuppliedTagId_UsesResolvedTagId()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetNfcTagIdByUriTokenAsync("good-token")).ReturnsAsync("real-tag-id");

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.GetByIdAsync("record-1", "real-tag-id"))
            .ReturnsAsync(new ServiceRecord { id = "record-1", TagId = "real-tag-id", FileUrls = new List<string>() });
        repo.Setup(r => r.Update("record-1", It.IsAny<ServiceRecord>()))
            .ReturnsAsync((string id, ServiceRecord r) => r);

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        // Client tries to supply a different TagId than the one the token actually resolves to.
        var form = Form(new()
        {
            ["Id"] = "record-1",
            ["Token"] = "good-token",
            ["TagId"] = "attacker-supplied-tag",
            ["MechanicName"] = "Jane Doe",
        });

        var result = await service.UpdateServiceRecordAsync(form);

        Assert.Equal("Jane Doe", result.MechanicName);
        repo.Verify(r => r.GetByIdAsync("record-1", "real-tag-id"), Times.Once);
        repo.Verify(r => r.GetByIdAsync(It.IsAny<string>(), "attacker-supplied-tag"), Times.Never);
    }

    [Fact]
    public async Task AddServiceRecordAsync_InvalidToken_ThrowsBadRequest()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetNfcTagIdByUriTokenAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
        var repo = new Mock<IServiceRecordRepository>();
        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var request = new LogMate.Domain.Models.ServiceRecordRequest
        {
            Token = "bad-token",
            EnteredDate = "2026-08-16",
            ServicedDate = "2026-08-16",
            MechanicName = "Jane Doe",
            Odometer = "1000",
            ServiceCategory = "General",
            ServiceType = "Oil Change",
            ServiceOption = "Full Synthetic",
        };

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.AddServiceRecordAsync(request));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.StatusCode);
    }
}
