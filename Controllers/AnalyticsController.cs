using Microsoft.AspNetCore.Mvc;
using PortfolioAnalyticsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace PortfolioAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly GoogleAnalyticsService _analyticsService;
        private readonly RedisService _rs;
        private readonly ILogger<AnalyticsController> _logger;

        // public AnalyticsController(RedisService redisService, ILogger<AnalyticsController> logger)
        public AnalyticsController(GoogleAnalyticsService analyticsService, RedisService redisService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
            _rs = redisService;
        }

        [HttpGet]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> GetAnalytics()
        {
            // Console.WriteLine("Get analytics api hitted");
            // return Ok(new {status = "success"});
            _logger.LogInformation("Get analytics api hitted");
            bool setCountSuccess = await _rs.SetCount();
            _logger.LogInformation("api set count success:" + setCountSuccess);
            try
            {
                var stats = await _analyticsService.GetAnalyticsDataAsync();
                var pageViews = await _analyticsService.GetWeeklyPageViewsAsync();
                // var apihitcount = await _analyticsService.GetApiHitsAsync();
                var apihitcount = await _rs.GetApiHits();

                return Ok(new
                {
                    totalViews = stats.TotalViews,
                    totalUsers = stats.TotalUsers,
                    sessions = stats.Sessions,
                    avgDuration = stats.AvgDuration,
                    pageViews = pageViews,
                    apihitcount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("realtime")]
        public async Task<IActionResult> GetRealtimeData()
        {
            _logger.LogInformation("realtime api hitted");
            Tuple<DateOnly, string> dateTuple = RedisService.GetDate();
            return Ok(new { message = "Real-time data endpoint" + dateTuple.Item2 });
        }

        [HttpGet("get_number_of_api_hits")]
        public async Task<IActionResult> GetApiHits()
        {
            var data = await _rs.GetApiHits();
            // Implement real-time data if needed
            return Ok(data);
        }   
    }
}