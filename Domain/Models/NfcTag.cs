public class ReplaceNfcTagRequest
{
    public string OldTagId { get; set; } = default!;
    public string NewTagId { get; set; } = default!;
}

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
    public string? VinNumber { get; set; } // Nullable property
    public string? LicencePlate { get; set; } // Nullable property
    public bool IsConfigured { get; set; }
}


public class UpdateAssetNfcTagRequest
{
    public string TagId { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public string Vehicle { get; set; } = default!;
    public string Style { get; set; } = default!;
    public int Engine { get; set; }
    public List<string> Fuel { get; set; } = new();
    public string Transmission { get; set; } = default!;
    public string Color { get; set; } = default!;
    public string? VinNumber { get; set; }
    public string? LicencePlate { get; set; }
}
