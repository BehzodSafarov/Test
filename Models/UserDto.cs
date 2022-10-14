using Avtotest.Web.Validations;

namespace Avtotest.Web.Models;

public class UserDto
{
    [Phone]
    public string? Phone { get; set; }

    [Password(8)]
    public string? Password { get; set; }
}