using UserApplication.API.Models;
using UserApplication.API.Models.Dto;

namespace UserApplication.Services.UserService
{
    public interface IUserService
    {
        public Task CreateAsync(UserCreateRequestDto dto, string createdBy);
        public Task<bool> UpdateAsync(Guid id, UserUpdateRequestDto dto, string modifiedBy);
        public Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto password, string modifiedBy);
        public Task<bool> ChangeLoginAsync(Guid id, ChangeLoginDto login, string modifiedBy);
        public Task<List<User>> GetAllActiveAsync();
        public Task<UserResponseDto?> GetByLogin(string login);
        public Task<User?> GetById(Guid id);
        public Task<UserResponseDto> ValidateCredentialsAsync(string login, string password);
        public Task<List<User>> GetOlderThanAsync(DateTime time);
        public Task<bool> DeleteByLoginAsync(string login, bool softDelete = false, string? revokedBy = null);
        public Task<bool> RestoreByLoginAsync(string login);
    }

}