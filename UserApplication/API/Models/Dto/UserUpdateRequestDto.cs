using System.ComponentModel.DataAnnotations;

namespace UserApplication.API.Models.Dto
{
    public class UserUpdateRequestDto
    {   
        [RegularExpression("^[a-zA-Zа-яА-ЯёЁ]*$", ErrorMessage = "Name can only contain Latin and Cyrillic letters.")]
        public string? Name { get; set; } // возможна ошибка, если имя состоит из 2 частей, разделенных "-"
        public Gender? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public UserUpdateRequestDto() { }
        public UserUpdateRequestDto(string name, Gender gender, DateTime? birthday)
        {
            Name = name;
            Gender = gender;
            Birthday = birthday;
        }
    }
}
