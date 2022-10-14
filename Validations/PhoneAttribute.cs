using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Avtotest.Web.Validations
{
    public class PhoneAttribute : ValidationAttribute
    {
        public int MaxLength = 7;

        public override bool IsValid(object? value)
        {
            var _value = (string?)value;

            if (string.IsNullOrEmpty(_value))
            {
                ErrorMessage = "Phone is null";
            }
            else if (_value.Length < MaxLength)
            {
                ErrorMessage = $"Phone must be minimum length of {MaxLength}";
            }
            else
            {
                var isNumber = long.TryParse(_value, out _);
                if (!isNumber)
                {
                    ErrorMessage = "Phone is not valid";
                    return false;
                }
            }

            return !string.IsNullOrEmpty(_value) && _value.Length >= MaxLength;
        }
    }
}
