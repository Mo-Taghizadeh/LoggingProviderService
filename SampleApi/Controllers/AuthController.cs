using AspNetCore.Filters;
using LoggingProviderService.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogWriterDynamic _writer;
        public AuthController(ILogWriterDynamic writer) => _writer = writer;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Dictionary<string, object?>body, CancellationToken ct)
        {
            var id = await _writer.InsertAsync("Request", body, ct);
            return Ok(new { requestId = id });
        }
    }

}
