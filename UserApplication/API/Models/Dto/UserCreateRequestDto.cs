using System.ComponentModel.DataAnnotations;

namespace UserApplication.API.Models.Dto
{
    public class UserCreateRequestDto
    {
        [Required(ErrorMessage = "Login is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Login can only contain Latin letters and digits.")]
        public required string Login { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Password can only contain Latin letters and digits.")]
        public required string Password { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression("^[a-zA-Zа-яА-ЯёЁ]+$", ErrorMessage = "Name can only contain Latin and Cyrillic letters.")]
        public required string Name { get; set; } // возможна ошибка, если имя состоит из 2 частей, разделенных "-"
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
        public UserCreateRequestDto() { }
        public UserCreateRequestDto(string login, string password, string name, Gender gender, DateTime? birthday, bool? admin)
        {
            Login = login;
            Password = password;
            Name = name;
            Gender = gender;
            Birthday = birthday;
            Admin = admin ?? false;
        }
    }
}
