namespace UserApplication.API.Models.Dto
{
    public class LoginResultDto
    {
        public required string Token { get; set; }
        public required UserResponseDto UserResponse { get; set; }
    }
}