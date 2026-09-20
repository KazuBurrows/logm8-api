using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LogMate.Domain.Models;

public class Record
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
    public string? UserId { get; set; }
}

public class ServiceRecordRequest
{
    public string Token { get; init; }
    public string EnteredDate { get; init; }
    public string ServicedDate { get; init; }
    public string MechanicName { get; init; }
    public string Odometer { get; init; }
    public string ServiceCategory { get; init; }
    public string ServiceType { get; init; }
    public string ServiceOption { get; init; }
    public string? Comment { get; init; }

    public static ServiceRecordRequest FromForm(IFormCollection form) =>
        new()
        {
            Token = form["Token"],
            EnteredDate = form["EnteredDate"],
            ServicedDate = form["ServicedDate"],
            MechanicName = form["MechanicName"],
            Odometer = form["Odometer"],
            ServiceCategory = form["serviceCategory"],
            ServiceType = form["ServiceType"],
            ServiceOption = form["serviceOption"],
            Comment = form["Comment"],
        };
}

public class AddServiceRecordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    public string? Id { get; set; }
    public string TagId { get; set; } = string.Empty;

    [Required]
    public string EnteredDate { get; set; } = string.Empty;

    [Required]
    public string ServicedDate { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string MechanicName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d+$", ErrorMessage = "Odometer must be numeric.")]
    public string Odometer { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string ServiceCategory { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string ServiceOption { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Comment { get; set; }
}
