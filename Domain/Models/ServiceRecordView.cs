namespace LogMate.Domain.Models;

public class ServiceRecordView
{
    public string Token { get; set; }
    public string id { get; set; }
    public string TagId { get; set; }
    public string EnteredDate { get; set; }
    public string ServicedDate { get; set; }
    public string MechanicName { get; set; }
    public string Odometer { get; set; }
    public string? Certified { get; set; }
    public string ServiceCategory { get; set; }
    public string ServiceType { get; set; }
    public string ServiceOption { get; set; }
    public string Comment { get; set; }
    public List<string> FileUrls { get; set; }
    public bool CanEdit { get; set; }

    public static ServiceRecordView FromRecord(Record record, string? callerUserId, int? callerMode) =>
        new()
        {
            Token = record.Token,
            id = record.id,
            TagId = record.TagId,
            EnteredDate = record.EnteredDate,
            ServicedDate = record.ServicedDate,
            MechanicName = record.MechanicName,
            Odometer = record.Odometer,
            Certified = record.Certified,
            ServiceCategory = record.ServiceCategory,
            ServiceType = record.ServiceType,
            ServiceOption = record.ServiceOption,
            Comment = record.Comment,
            FileUrls = record.FileUrls,
            CanEdit = callerMode != (int)UserMode.Guest
                && record.UserId != null
                && record.UserId == callerUserId,
        };
}
