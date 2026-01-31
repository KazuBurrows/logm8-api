namespace Company.Function
{
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
}