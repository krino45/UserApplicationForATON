using System.ComponentModel.DataAnnotations;

namespace UserApplication.API.Models.Dto
{
    public class ChangeLoginDto
    {
        [Required(ErrorMessage = "Login is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Login can only contain Latin letters and digits.")]
        public required string Login { get; set; }

    }
}
