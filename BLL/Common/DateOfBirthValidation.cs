using System.ComponentModel.DataAnnotations;

namespace BLL.Common;

public static class DateOfBirthValidation
{
    public static readonly DateOnly Earliest = new(1900, 1, 1);

    public static IEnumerable<ValidationResult> Validate(DateOnly? dateOfBirth, string memberName)
    {
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult("Date of birth cannot be in the future.", [memberName]);
        }
        else if (dateOfBirth < Earliest)
        {
            yield return new ValidationResult("Enter a date of birth after 1900.", [memberName]);
        }
    }
}
