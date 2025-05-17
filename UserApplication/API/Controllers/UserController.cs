using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserApplication.API.Models.Dto;
using UserApplication.Services.UserService;
using static UserApplication.Program;

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
        [HttpGet("/login")]
        public async Task<ActionResult<LoginResultDto>> Login(string login, string password)
        {
            _logger.LogInformation($"Login attempt by {login}");

            UserResponseDto user;
            try
            {
                user = await _service.ValidateCredentialsAsync(login, password);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Login failed for {login}: {ex.Message}");
                return Unauthorized(ex.Message);
            }
            var claims = new List<Claim>
                {   new Claim(ClaimTypes.Name, login),
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
        public async Task<IActionResult> Create([FromBody] UserCreateRequestDto dto)
        {
            _logger.LogInformation($"{CurrentUser} is creating a new user: {dto.Login}");
            await _service.CreateAsync(dto, CurrentUser);
            return Ok("User created");
        }

        // UPDATE SECTION 
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequestDto dto)
        {
            var targetUser = await _service.GetById(id);
            if (!IsAdmin && targetUser?.RevokedOn.HasValue == true)
            {
                _logger.Log(LogLevel.Warning, $"User {targetUser.Login} tried updating while not being active");
                return Forbid("User is not active");
            }
            if (!IsAdmin && targetUser?.Login != CurrentUser)
            {
                _logger.Log(LogLevel.Warning, $"User {CurrentUser} tried updating user {targetUser?.Login}");
                return Forbid("User login mismatch");
            }
            bool result = await _service.UpdateAsync(id, dto, CurrentUser);
            _logger.LogInformation($"{CurrentUser} updated {targetUser?.Login}");
            return result ? Ok("User updated") : NotFound("User not found");
        }

        [HttpPut("{id:guid}/password")]
        [Authorize]
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
        public async Task<IActionResult> GetAllActive()
        {
            _logger.LogInformation($"{CurrentUser} got active user list");
            var result = await _service.GetAllActiveAsync();
            return Ok(result);
        }

        [HttpGet("{login}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByLogin(string login)
        {
            _logger.LogInformation($"{CurrentUser} got user data for {login}");

            var result = await _service.GetByLogin(login);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("credentials")]
        [Authorize]
        public async Task<IActionResult> GetByLoginAndPassword(string login, string password)
        {
            _logger.LogInformation($"{CurrentUser} is validating credentials");
            if (login != CurrentUser)
            {
                _logger.LogWarning($"{CurrentUser} tried validating credentials for another user: {login}");
                return Forbid("You can only request your own data");
            }
            var result = await _service.ValidateCredentialsAsync(login, password);
            return Ok(result);
        }

        [HttpGet("older-than/{years}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOlderThan(int years)
        {
            _logger.LogInformation($"{CurrentUser} requested users older than {years} years");
            var date = DateTime.Today.AddYears(-years);
            var result = await _service.GetOlderThanAsync(date);
            return Ok(result);
        }

        // DELETE SECTION

        [HttpDelete("{login}")]
        [Authorize(Roles = "Admin")]
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
