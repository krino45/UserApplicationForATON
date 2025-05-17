using System.ComponentModel.DataAnnotations;

namespace UserApplication.API.Models.Dto
{
    public class UserResponseDto
    {
        public string? Name { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool? Admin { get; set; }
        public bool? Active { get; set; }

        public UserResponseDto() {  }
        public UserResponseDto(string? name, Gender? gender, DateTime? birthday, bool? admin, bool? active)
        {
            Name = name;
            Gender = gender;
            Birthday = birthday;
            Admin = admin;
            Active = active;
        }
   }
}