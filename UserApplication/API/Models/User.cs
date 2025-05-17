using System.ComponentModel.DataAnnotations;

namespace UserApplication.API.Models
{
    public enum Gender
    {
        Female = 0,
        Male = 1,
        Unknown = 2
    }

    public class User
    {
        public Guid Guid { get; set; }
        [Required(ErrorMessage = "Login is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Login can only contain Latin letters and digits.")]
        public required string Login { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Password can only contain Latin letters and digits.")]
        public required string Password { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression("^[a-zA-Zа-€ј-яЄ®]+$", ErrorMessage = "Name can only contain Latin and Cyrillic letters.")]
        public required string Name { get; set; } // возможна ошибка, если им€ состоит из 2 частей, разделенных "-"
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
        public DateTime CreatedOn { get; set; }
        [Required(ErrorMessage = "CreatedBy is required.")]
        public required string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? RevokedOn { get; set; }
        public string? RevokedBy { get; set; }

    }
}
