using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BusinessLogic.Attributes
{
    public class WeekDayAttribute : ValidationAttribute
    {
        private static readonly string[] AllowedDays =
        {
            "Понеділок", "Вівторок", "Середа", "Четвер", "П’ятниця", "Субота", "Неділя"
        };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue && AllowedDays.Contains(strValue, StringComparer.OrdinalIgnoreCase))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"День тижня повинен бути одним із: {string.Join(", ", AllowedDays)}");
        }
    }
}
