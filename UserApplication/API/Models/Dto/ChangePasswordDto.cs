using System.ComponentModel.DataAnnotations;

namespace UserApplication.API.Models.Dto
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Password can only contain Latin letters and digits.")]
        public required string Password { get; set; }

    }
}
