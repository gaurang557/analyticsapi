using Microsoft.AspNetCore.Mvc;
using PortfolioAnalyticsApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string healthcheck()
        {
            Console.WriteLine("Health check endpoint called");
            return "Health check successful";
        }   
    }
}