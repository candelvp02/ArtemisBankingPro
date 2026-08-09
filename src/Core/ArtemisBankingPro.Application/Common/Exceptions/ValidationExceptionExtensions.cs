using FluentValidation.Results;

namespace ArtemisBankingPro.Application.Common.Exceptions;

public static class ValidationExceptionExtensions
{
    public static IDictionary<string, string[]> ToErrorDictionary(this IEnumerable<ValidationFailure> failures)
    {
        return failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(f => f.ErrorMessage).ToArray());
    }
}