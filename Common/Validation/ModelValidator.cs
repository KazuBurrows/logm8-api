using System.ComponentModel.DataAnnotations;
using LogMate.Application.Exceptions;

namespace LogMate.Common.Validation;

public static class ModelValidator
{
    public static T Validate<T>(T? model) where T : class
    {
        if (model == null)
            throw new BadRequestException("Request body is missing or invalid.");

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            var message = string.Join(" ", results.Select(r => r.ErrorMessage));
            throw new BadRequestException(message);
        }

        return model;
    }
}
