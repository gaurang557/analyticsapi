using PortfolioAnalyticsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("dev/auth")]
public class DevAuthController : ControllerBase
{
    [HttpGet("token")]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        return Ok(new
        {
            access_token = DevJwtGenerator.GenerateToken()
        });
    }
}
