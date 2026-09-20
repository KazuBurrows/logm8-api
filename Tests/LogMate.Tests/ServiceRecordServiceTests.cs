using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Application.Services;
using LogMate.Domain.Models;
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

    private static OneLifeTokenInfo TokenInfo(string logId, string? userId, int mode = (int)UserMode.Service) =>
        new(logId, userId, mode, DateTime.UtcNow.AddMinutes(30));

    [Fact]
    public async Task UpdateServiceRecordAsync_MissingToken_ThrowsBadRequest()
    {
        var nfcTagService = new Mock<INfcTagService>();
        var repo = new Mock<IServiceRecordRepository>();
        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["Id"] = "record-1" });

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.UpdateServiceRecordAsync(form));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.StatusCode);
        nfcTagService.Verify(s => s.GetOneLifeTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServiceRecordAsync_InvalidToken_ThrowsBadRequest_AndDoesNotTouchRepo()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("bad-token")).ReturnsAsync((OneLifeTokenInfo?)null);
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
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("good-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", "user-123"));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.GetByIdAsync("record-1", "real-tag-id"))
            .ReturnsAsync(new ServiceRecord { id = "record-1", TagId = "real-tag-id", UserId = "user-123", FileUrls = new List<string>() });
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
    public async Task UpdateServiceRecordAsync_CallerIsNotOwner_ThrowsForbidden_AndDoesNotUpdate()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("good-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", "user-456"));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.GetByIdAsync("record-1", "real-tag-id"))
            .ReturnsAsync(new ServiceRecord { id = "record-1", TagId = "real-tag-id", UserId = "user-123", FileUrls = new List<string>() });

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["Id"] = "record-1", ["Token"] = "good-token" });

        await Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateServiceRecordAsync(form));
        repo.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<ServiceRecord>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServiceRecordAsync_ExistingRecordHasNoOwner_ThrowsForbidden_ForAnyCaller()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("good-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", "user-123"));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.GetByIdAsync("record-1", "real-tag-id"))
            .ReturnsAsync(new ServiceRecord { id = "record-1", TagId = "real-tag-id", UserId = null, FileUrls = new List<string>() });

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["Id"] = "record-1", ["Token"] = "good-token" });

        await Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateServiceRecordAsync(form));
        repo.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<ServiceRecord>()), Times.Never);
    }

    [Fact]
    public async Task UpdateServiceRecordAsync_IdOverload_UsesExplicitId_NotFormId()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("good-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", "user-123"));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.GetByIdAsync("route-id", "real-tag-id"))
            .ReturnsAsync(new ServiceRecord { id = "route-id", TagId = "real-tag-id", UserId = "user-123", FileUrls = new List<string>() });
        repo.Setup(r => r.Update("route-id", It.IsAny<ServiceRecord>()))
            .ReturnsAsync((string id, ServiceRecord r) => r);

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["Id"] = "form-id", ["Token"] = "good-token" });

        await service.UpdateServiceRecordAsync("route-id", form);

        repo.Verify(r => r.GetByIdAsync("route-id", "real-tag-id"), Times.Once);
        repo.Verify(r => r.GetByIdAsync("form-id", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddServiceRecordAsync_InvalidToken_ThrowsBadRequest()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync(It.IsAny<string>())).ReturnsAsync((OneLifeTokenInfo?)null);
        var repo = new Mock<IServiceRecordRepository>();
        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var request = new ServiceRecordRequest
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

    [Fact]
    public async Task AddServiceRecordAsync_ValidToken_StampsUserIdFromToken_IgnoringClientInput()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("good-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", "user-123"));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.Add(It.IsAny<ServiceRecord>()))
            .ReturnsAsync((ServiceRecord r) => r);

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var request = new ServiceRecordRequest
        {
            Token = "good-token",
            EnteredDate = "2026-08-16",
            ServicedDate = "2026-08-16",
            MechanicName = "Jane Doe",
            Odometer = "1000",
            ServiceCategory = "General",
            ServiceType = "Oil Change",
            ServiceOption = "Full Synthetic",
        };

        var result = await service.AddServiceRecordAsync(request);

        Assert.Equal("user-123", result.UserId);
        repo.Verify(r => r.Add(It.Is<ServiceRecord>(rec => rec.UserId == "user-123")), Times.Once);
    }

    [Fact]
    public async Task AddServiceRecordAsync_TokenHasNoUserId_StampsNullUserId()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("guest-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", null, (int)UserMode.Guest));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.Add(It.IsAny<ServiceRecord>()))
            .ReturnsAsync((ServiceRecord r) => r);

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var request = new ServiceRecordRequest
        {
            Token = "guest-token",
            EnteredDate = "2026-08-16",
            ServicedDate = "2026-08-16",
            MechanicName = "Jane Doe",
            Odometer = "1000",
            ServiceCategory = "General",
            ServiceType = "Oil Change",
            ServiceOption = "Full Synthetic",
        };

        var result = await service.AddServiceRecordAsync(request);

        Assert.Null(result.UserId);
    }

    [Fact]
    public async Task AddServiceRecordAsync_FormOverload_MissingToken_ThrowsBadRequest()
    {
        var nfcTagService = new Mock<INfcTagService>();
        var repo = new Mock<IServiceRecordRepository>();
        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new() { ["MechanicName"] = "Jane Doe" });

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.AddServiceRecordAsync(form));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.StatusCode);
        nfcTagService.Verify(s => s.GetOneLifeTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddServiceRecordAsync_FormOverload_ValidToken_StampsUserId()
    {
        var nfcTagService = new Mock<INfcTagService>();
        nfcTagService.Setup(s => s.GetOneLifeTokenAsync("good-token"))
            .ReturnsAsync(TokenInfo("real-tag-id", "user-123"));

        var repo = new Mock<IServiceRecordRepository>();
        repo.Setup(r => r.Add(It.IsAny<ServiceRecord>()))
            .ReturnsAsync((ServiceRecord r) => r);

        var service = new ServiceRecordService(repo.Object, nfcTagService.Object);

        var form = Form(new()
        {
            ["Token"] = "good-token",
            ["MechanicName"] = "Jane Doe",
            ["EnteredDate"] = "2026-08-16",
            ["ServicedDate"] = "2026-08-16",
            ["Odometer"] = "1000",
            ["ServiceCategory"] = "General",
            ["ServiceType"] = "Oil Change",
            ["ServiceOption"] = "Full Synthetic",
            ["Comment"] = "",
        });

        var result = await service.AddServiceRecordAsync(form);

        Assert.Equal("user-123", result.UserId);
        Assert.Equal("real-tag-id", result.TagId);
    }
}
