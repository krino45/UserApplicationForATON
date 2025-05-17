using System.Security.Authentication;
using UserApplication.API.Models;
using UserApplication.API.Models.Dto;
using UserApplication.Persistence.Repositories.UserRepository;
using UserApplication.Utility;

namespace UserApplication.Services.UserService
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task CreateAsync(UserCreateRequestDto dto, string createdBy)
        {
            var user = new User
            {
                Login = dto.Login,
                Password = CustomPasswordHasher.Hash(dto.Password),
                Name = dto.Name,
                Gender = dto.Gender,
                Birthday = dto.Birthday,
                Admin = dto.Admin,
                CreatedOn = DateTime.Now,
                CreatedBy = createdBy,
            };
            await _userRepository.AddAsync(user);
        }
        public async Task<bool> UpdateAsync(Guid id, UserUpdateRequestDto dto, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            user.Name = dto.Name ?? user.Name;
            user.Gender = dto.Gender ?? user.Gender;
            user.Birthday = dto.Birthday ?? user.Birthday;

            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
            return true;
        }
        public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto password, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new AuthenticationException($"User with given id ({id}) does not exist");
            }
            if (password == null)
            {
                throw new ArgumentNullException("Password DTO is null");
            }
            if (CustomPasswordHasher.Verify(password.Password, user.Password))
            {
                return false;
            }

            user.Password = CustomPasswordHasher.Hash(password.Password);

            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
            return true;
        }
        public async Task<bool> ChangeLoginAsync(Guid id, ChangeLoginDto login, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new AuthenticationException($"User with given id ({id}) does not exist");
            }
            if (login == null)
            {
                throw new ArgumentNullException("Login DTO is null");
            }
            if (login.Login.Equals(user.Password))
            {
                return false;
            }

            user.Login = login.Login;

            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
            return true;
        }
        public async Task<List<User>> GetAllActiveAsync()
        {
            return (await _userRepository.GetAllByAsync(user => user.RevokedOn == null))
                .OrderBy(u => u.CreatedOn).ToList();
        }
        public async Task<UserResponseDto?> GetByLogin(string login)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                return null;
            }
            return new UserResponseDto
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                Active = user.RevokedBy == null,
            };
        }
        public async Task<UserResponseDto?> ValidateCredentialsAsync(string login, string password)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                throw new AuthenticationException($"Invalid credentials: {login}:{password}");
            }
            if (!CustomPasswordHasher.Verify(password, user.Password))
            {
                throw new AuthenticationException($"Invalid credentials: {login}:{password}");
            }
            return new UserResponseDto
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                Active = user.RevokedBy == null,
            };
        }
        public async Task<List<User>> GetOlderThanAsync(DateTime? time)
        {
            return (await _userRepository.GetAllByAsync(user => user.Birthday >= time))
                           .ToList();
        }
        public async Task<bool> DeleteByLoginAsync(string login, bool softDelete = false, string? revokedBy = null)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                return false;
            }

            if (softDelete)
            {
                user.RevokedOn = DateTime.Now;
                user.RevokedBy = revokedBy;
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                await _userRepository.DeleteAsync(user);
            }
            return true;

        }
        public async Task<bool> RestoreByLoginAsync(string login)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                return false;
            }

            user.RevokedOn = null;
            user.RevokedBy = null;
            return true;
        }
    }

}