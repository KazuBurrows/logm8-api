using LogMate.Application.Exceptions;
using LogMate.Common.Validation;
using LogMate.Domain.Models;
using Xunit;

namespace LogMate.Tests;

public class ModelValidatorTests
{
    [Fact]
    public void Validate_NullModel_ThrowsBadRequest()
    {
        Assert.Throws<BadRequestException>(() => ModelValidator.Validate<AddServiceOptionRequest>(null));
    }

    [Fact]
    public void Validate_MissingRequiredField_ThrowsBadRequest()
    {
        var request = new AddServiceOptionRequest { Name = "", CategoryId = 1 };

        Assert.Throws<BadRequestException>(() => ModelValidator.Validate(request));
    }

    [Fact]
    public void Validate_NonNumericOdometer_ThrowsBadRequest()
    {
        var request = new AddServiceRecordRequest
        {
            Token = "tok",
            EnteredDate = "2026-08-16",
            ServicedDate = "2026-08-16",
            MechanicName = "Jane Doe",
            Odometer = "not-a-number",
            ServiceCategory = "General",
            ServiceType = "Oil Change",
            ServiceOption = "Full Synthetic",
        };

        Assert.Throws<BadRequestException>(() => ModelValidator.Validate(request));
    }

    [Fact]
    public void Validate_ValidModel_ReturnsSameInstance()
    {
        var request = new AddServiceOptionRequest { Name = "Brake Service", CategoryId = 1 };

        var result = ModelValidator.Validate(request);

        Assert.Same(request, result);
    }
}
