using LogMate.Domain.Models;
using Xunit;
using ServiceRecord = LogMate.Domain.Models.Record;

namespace LogMate.Tests;

public class ServiceRecordViewTests
{
    private static ServiceRecord MakeRecord(string? userId) => new()
    {
        id = "record-1",
        TagId = "tag-1",
        Token = "token-1",
        EnteredDate = "2026-08-16",
        ServicedDate = "2026-08-16",
        MechanicName = "Jane Doe",
        Odometer = "1000",
        ServiceCategory = "General",
        ServiceType = "Oil Change",
        ServiceOption = "Full Synthetic",
        Comment = "",
        FileUrls = new List<string>(),
        UserId = userId,
    };

    [Fact]
    public void FromRecord_MatchingUserId_ServiceMode_CanEditTrue()
    {
        var view = ServiceRecordView.FromRecord(MakeRecord("user-123"), "user-123", (int)UserMode.Service);
        Assert.True(view.CanEdit);
    }

    [Fact]
    public void FromRecord_MismatchedUserId_CanEditFalse()
    {
        var view = ServiceRecordView.FromRecord(MakeRecord("user-123"), "user-456", (int)UserMode.Service);
        Assert.False(view.CanEdit);
    }

    [Fact]
    public void FromRecord_NullRecordUserId_LegacyRecord_CanEditFalse()
    {
        var view = ServiceRecordView.FromRecord(MakeRecord(null), "user-123", (int)UserMode.Service);
        Assert.False(view.CanEdit);
    }

    [Fact]
    public void FromRecord_NullCallerUserId_CanEditFalse()
    {
        var view = ServiceRecordView.FromRecord(MakeRecord("user-123"), null, (int)UserMode.Service);
        Assert.False(view.CanEdit);
    }

    [Fact]
    public void FromRecord_MatchingUserId_GuestMode_CanEditFalse()
    {
        var view = ServiceRecordView.FromRecord(MakeRecord("user-123"), "user-123", (int)UserMode.Guest);
        Assert.False(view.CanEdit);
    }
}
