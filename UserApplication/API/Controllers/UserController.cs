using Microsoft.AspNetCore.Mvc;
using UserApplication.API.Models;
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
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        // CREATE SECTION

        [HttpPost]
        public async Task<ActionResult<User>> Create(UserCreateRequestDto userCreate)
        {
            if (userCreate == null)
            {
                return BadRequest("No user provided");
            }
            var created = await _service.CreateAsync(userCreate);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        }

        // READ SECTION 

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
        {
            var user = await _service.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        // UPDATE SECTION

        // DELETE SECTION

        [HttpGet(Name = "GetUsers")]
        public IActionResult Get()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => index)
            .ToArray());
        }
    }
}
