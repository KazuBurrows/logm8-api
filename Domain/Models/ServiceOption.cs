public class ServiceOption
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public List<string> ServiceTypes { get; set; } = new();

    public List<ServiceOption> Children { get; set; } = new();
}

public class FlatServiceOption
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public List<string> ServiceTypes { get; set; } = new();
}

public class ServiceHierarchy
{
    public List<ServiceOption> MotorbikeOptions { get; set; } = new();
    public List<ServiceOption> OwnershipOptions { get; set; } = new();
}

public class AddServiceOptionRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
}


public class AddParentOptionRequest
{
    public int OptionId { get; set; }
    public int ParentId { get; set; }
}

public class AddServiceTypeRequest
{
    public string OptionId { get; set; }
    public string ServiceTypeId { get; set; }
}