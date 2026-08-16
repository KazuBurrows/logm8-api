using System.ComponentModel.DataAnnotations;

namespace LogMate.Domain.Models;

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
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer.")]
    public int CategoryId { get; set; }
}

public class AddParentOptionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "OptionId must be a positive integer.")]
    public int OptionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ParentId must be a positive integer.")]
    public int ParentId { get; set; }
}

public class AddServiceTypeRequest
{
    [Required, RegularExpression(@"^\d+$", ErrorMessage = "OptionId must be numeric.")]
    public string OptionId { get; set; }

    [Required, RegularExpression(@"^\d+$", ErrorMessage = "ServiceTypeId must be numeric.")]
    public string ServiceTypeId { get; set; }
}
