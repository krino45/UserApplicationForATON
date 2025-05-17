namespace UserApplication.API.Models.Dto
{
    public class LoginResultDto
    {
        public string Token { get; set; }
        public UserResponseDto UserResponse { get; set; }
    }
}