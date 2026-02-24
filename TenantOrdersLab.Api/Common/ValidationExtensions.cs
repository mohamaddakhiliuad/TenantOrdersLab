using System.Linq;
using FluentValidation.Results;

namespace TenantOrdersLab.Api.Common;

public static class ValidationExtensions
{
    public static Dictionary<string, string[]> ToProblemDetails(this ValidationResult result)
        => result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
}