using Microsoft.AspNetCore.Mvc;
using UserApplication.API.Models;

namespace UserApplication.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetUsers")]
        public IActionResult Get()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => index)
            .ToArray());
        }
    }
}
