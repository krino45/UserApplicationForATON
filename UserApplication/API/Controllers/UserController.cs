using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserApplication.API.Models.Dto;
using UserApplication.Services.UserService;

namespace UserApplication.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _service;
        private readonly string _secretKey;

        private string CurrentUser => User.Identity?.Name ?? "idk";
        private bool IsAdmin => User.IsInRole("Admin");


        public UserController(ILogger<UserController> logger, IUserService service, IConfiguration config)
        {
            _logger = logger;
            _service = service;
            _secretKey = config["SecretKey"]!;
        }

        // Login
        [HttpPost("login")]
        [SwaggerOperation("Вход в приложение", "Возвращает JWT токен")]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation($"Login attempt by {dto.Login}");

            UserResponseDto user;
            try
            {
                user = await _service.ValidateCredentialsAsync(dto.Login, dto.Password);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Login failed for {dto.Login}: {ex.Message}");
                return Unauthorized(ex.Message);
            }
            if (user.Active == false)
            {
                return Problem("Can't log in : user was revoked.", statusCode: 403);
            }
            var claims = new List<Claim>
                {   new Claim(ClaimTypes.Name, dto.Login),
                    new Claim(ClaimTypes.Role, (user.Admin == true) ? "Admin" : "User")
                };
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(15)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(_secretKey), SecurityAlgorithms.HmacSha256)
                );

            return new LoginResultDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                UserResponse = user
            };
        }

        // CREATE SECTION

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation("Создать пользователя", "Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли пользователь админом (Доступно Админам)")]
        public async Task<IActionResult> Create([FromBody] UserCreateRequestDto dto)
        {
            _logger.LogInformation($"{CurrentUser} is creating a new user: {dto.Login}");
            await _service.CreateAsync(dto, CurrentUser);
            return Ok("User created");
        }

        // UPDATE SECTION 
        [HttpPut("{id:guid}")]
        [Authorize]
        [SwaggerOperation("Изменить пользователя", "Изменение имени, пола или даты рождения пользователя (Может менять Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn))")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequestDto dto)
        {
            var targetUser = await _service.GetById(id);
            if (!IsAdmin && targetUser?.RevokedOn.HasValue == true)
            {
                _logger.Log(LogLevel.Warning, $"User {targetUser.Login} tried updating while not being active");
                return Problem("User is not active", statusCode: 403);
            }
            if (!IsAdmin && targetUser?.Login != CurrentUser)
            {
                _logger.Log(LogLevel.Warning, $"User {CurrentUser} tried updating user {targetUser?.Login}");
                return Problem("User login mismatch", statusCode: 403);
            }
            bool result = await _service.UpdateAsync(id, dto, CurrentUser);
            _logger.LogInformation($"{CurrentUser} updated {targetUser?.Login}");
            return result ? Ok("User updated") : NotFound("User not found");
        }

        [HttpPut("{id:guid}/password")]
        [Authorize]
        [SwaggerOperation("Изменить пароль", "Изменение пароля (Пароль может менять либо Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn))")]

        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
        {
            _logger.LogInformation($"{CurrentUser} attempts to change password");

            bool result = await _service.ChangePasswordAsync(id, dto, CurrentUser);
            if (result)
            {
                _logger.LogInformation($"Password changed by {CurrentUser}");
                return Ok("Password updated");
            }

            _logger.LogWarning($"Password change failed for {CurrentUser}");
            return BadRequest("New password is the same as old one");
        }

        [HttpPut("{id:guid}/login")]
        [Authorize]
        [SwaggerOperation("Изменить логин", "Изменение логина (Логин может менять либо Администратор, либо лично пользователь, если он активен (отсутствует RevokedOn), логин должен оставаться уникальным)")]

        public async Task<IActionResult> ChangeLogin(Guid id, [FromBody] ChangeLoginDto dto)
        {
            _logger.LogInformation($"{CurrentUser} attempts to change login");

            bool result = await _service.ChangeLoginAsync(id, dto, CurrentUser);
            if (result)
            {
                _logger.LogInformation($"Login changed by {CurrentUser}");
                return Ok("Login updated");
            }

            _logger.LogWarning($"Login change failed for {CurrentUser}");
            return BadRequest("Login unchanged");
        }


        // READ SECTION

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation("Получить всех активных пользователей", "Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по CreatedOn (Доступно Админам)")]

        public async Task<IActionResult> GetAllActive()
        {
            _logger.LogInformation($"{CurrentUser} got active user list");
            var result = await _service.GetAllActiveAsync();
            return Ok(result);
        }

        [HttpGet("{login}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation("Получить пользователя по login", "Запрос пользователя по логину, в списке долны быть имя, пол и дата рождения статус активный или нет (Доступно Админам)")]

        public async Task<IActionResult> GetByLogin(string login)
        {
            _logger.LogInformation($"{CurrentUser} got user data for {login}");

            var result = await _service.GetByLogin(login);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("credentials")]
        [Authorize]
        [SwaggerOperation("Получить пользователя по логину и паролю", "Запрос пользователя по логину и паролю (Доступно только самому пользователю, если он активен (отсутствует RevokedOn))")]

        public async Task<IActionResult> GetByLoginAndPassword([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation($"{CurrentUser} is validating credentials");
            if (dto.Login != CurrentUser)
            {
                _logger.LogWarning($"{CurrentUser} tried validating credentials for another user: {dto.Login}");
                return Problem("You can only request your own data", statusCode: 403);
            }
            var result = await _service.ValidateCredentialsAsync(dto.Login, dto.Password);
            return Ok(result);
        }

        [HttpGet("older-than/{years}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation("Получить пользователей старше years лет.", "Запрос всех пользователей старше определённого возраста (Доступно Админам")]

        public async Task<IActionResult> GetOlderThan(int years)
        {
            _logger.LogInformation($"{CurrentUser} requested users older than {years} years");
            var date = DateTime.Today.AddYears(-years);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var result = await _service.GetOlderThanAsync(date);
            return Ok(result);
        }

        // DELETE SECTION

        [HttpDelete("{login}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation("Удалить пользователя", " Удаление пользователя по логину полное или мягкое (При мягком удалении должна происходить простановка RevokedOn и RevokedBy) (Доступно Админам)")]

        public async Task<IActionResult> Delete(string login, [FromQuery] bool softDelete = false)
        {
            _logger.LogInformation($"{CurrentUser} is deleting {login} (soft: {softDelete})");

            var result = await _service.DeleteByLoginAsync(login, softDelete, CurrentUser);
            if (result)
            {
                _logger.LogInformation($"User {login} deleted by {CurrentUser}");
                return Ok("User deleted");
            }

            _logger.LogWarning($"Delete failed: {login} not found");
            return NotFound("User not found");
        }

        [HttpPut("{login}/restore")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation("Восстановить пользователя", "Восстановление пользователя - Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)")]

        public async Task<IActionResult> Restore(string login)
        {
            _logger.LogInformation($"{CurrentUser} is restoring user {login}");

            var result = await _service.RestoreByLoginAsync(login);
            if (result)
            {
                _logger.LogInformation($"User {login} restored by {CurrentUser}");
                return Ok("User restored");
            }

            _logger.LogWarning($"Restore failed: {login} not found");
            return NotFound("User not found");
        }

    }
}
