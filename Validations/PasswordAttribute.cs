using System.ComponentModel.DataAnnotations;

namespace Avtotest.Web.Validations
{
    public class PasswordAttribute : ValidationAttribute
    {
        public int MaxLength = 6;

        public PasswordAttribute(int maxLength)
        {
            MaxLength = maxLength;
        }

        public override bool IsValid(object? value)
        {
            var _value = (string?)value;

            if (string.IsNullOrEmpty(_value))
            {
                ErrorMessage = "Password is null";
            }
            else if(_value.Length < MaxLength)
            {
                ErrorMessage = $"Password must be minimum length of {MaxLength}";
            }

            return !string.IsNullOrEmpty(_value) && _value.Length >= MaxLength;
        }
    }
}
