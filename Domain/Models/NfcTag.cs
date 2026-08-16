using System.ComponentModel.DataAnnotations;

namespace LogMate.Domain.Models;

public class Tag
{
    public string id { get; set; }
    public string TagId { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Vehicle { get; set; }
    public string Style { get; set; }
    public int Engine { get; set; }
    public List<string> Fuel { get; set; }
    public string Transmission { get; set; }
    public string Color { get; set; }
    public string? VinNumber { get; set; }
    public string? LicencePlate { get; set; }
    public bool IsConfigured { get; set; }
}

public class ReplaceNfcTagRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string OldTagId { get; set; } = default!;

    [Required, StringLength(200, MinimumLength = 1)]
    public string NewTagId { get; set; } = default!;
}

public class UpdateAssetNfcTagRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string TagId { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Make { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Model { get; set; } = default!;

    [Range(1885, 2100, ErrorMessage = "Year must be a plausible model year.")]
    public int Year { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Vehicle { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Style { get; set; } = default!;

    [Range(0, 20000, ErrorMessage = "Engine must be a plausible displacement/value.")]
    public int Engine { get; set; }

    public List<string> Fuel { get; set; } = new();

    [Required, StringLength(100, MinimumLength = 1)]
    public string Transmission { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Color { get; set; } = default!;

    [StringLength(50)]
    public string? VinNumber { get; set; }

    [StringLength(20)]
    public string? LicencePlate { get; set; }
}
