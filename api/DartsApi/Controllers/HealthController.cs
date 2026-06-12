using Microsoft.AspNetCore.Mvc;

namespace DartsApi.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    [HttpGet("healthz")]
    public IActionResult Get() =>
        Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}
