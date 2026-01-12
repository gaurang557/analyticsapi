using Microsoft.AspNetCore.Mvc;
using PortfolioAnalyticsApi.Services;

namespace PortfolioAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly GoogleAnalyticsService _analyticsService;
        // private readonly GoogleAnalyticsService _sendanalyticsService;
        // private readonly GoogleAnalyticsTracker _sendanalyticsService;


        public AnalyticsController(GoogleAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
            // _sendanalyticsService = sendanalyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics()
        {
            Console.WriteLine("sending api count hit");
            await GoogleAnalyticsTracker.TrackApiHitAsync();
            try
            {
                var stats = await _analyticsService.GetAnalyticsDataAsync();
                var pageViews = await _analyticsService.GetWeeklyPageViewsAsync();
                var apihitcount = await _analyticsService.GetApiHitsAsync();

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
            Console.WriteLine("sending api count hit");
            await GoogleAnalyticsTracker.TrackApiHitAsync();
            // Implement real-time data if needed
            return Ok(new { message = "Real-time data endpoint" });
        }

        [HttpGet("get_number_of_api_hits")]
        public async Task<IActionResult> GetApiHits()
        {
            Console.WriteLine("sending api count hit");
            var data = await _analyticsService.GetApiHitsAsync();
            // Implement real-time data if needed
            return Ok(data);
        }   
    }
}