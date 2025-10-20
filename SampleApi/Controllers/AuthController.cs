using AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [LogAction(serviceId: 1, serviceMethodId: 1, Summary = "Login")]
        public IActionResult Login([FromBody] object body) => Ok(new { token = "ok" });
    }

}
