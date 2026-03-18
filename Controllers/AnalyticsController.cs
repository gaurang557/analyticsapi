using Microsoft.AspNetCore.Mvc;
using PortfolioAnalyticsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace PortfolioAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly GoogleAnalyticsService _analyticsService;
        private readonly RedisService _rs;
        private readonly IDistributedCache _drs;
        private readonly string cachekey;

        private readonly ILogger<AnalyticsController> _logger;

        // public AnalyticsController(RedisService redisService, ILogger<AnalyticsController> logger)
        public AnalyticsController(GoogleAnalyticsService analyticsService, IConfiguration configuration,
        RedisService redisService, ILogger<AnalyticsController> logger, IDistributedCache distributedredisService)
        {
            _analyticsService = analyticsService;
            _logger = logger;
            _rs = redisService;
            _drs = distributedredisService;
            cachekey = configuration["CacheKey"]
                ?? throw new ArgumentNullException("CacheKey not configured");
        }

        [HttpGet]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> GetAnalytics()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var cachedData = await _drs.GetStringAsync(cachekey);
            if (!string.IsNullOrWhiteSpace(cachedData))
            {
                Console.WriteLine("going in cache");
                try
                {
                    using var document = JsonDocument.Parse(cachedData);
                    sw.Stop();
                    _logger.LogInformation("Time Elapsed (cached)-> " + sw.ElapsedMilliseconds + " ms");
                    var result = document.RootElement.Clone();

                    return Ok(result);
                }
                catch (JsonException)
                {
                    Console.WriteLine("Cache miss or deserialization failed, fetching from Google Analytics");
                }
            }
            bool setCountSuccess = await _rs.SetCount();
            try
            {
                var stats = await _analyticsService.GetAnalyticsDataAsync();
                var pageViews = await _analyticsService.GetWeeklyPageViewsAsync();
                // var apihitcount = await _analyticsService.GetApiHitsAsync();
                var apihitcount = await _rs.GetApiHits();
                var ObjDto = new
                {
                    totalViews = stats.TotalViews,
                    totalUsers = stats.TotalUsers,
                    sessions = stats.Sessions,
                    avgDuration = stats.AvgDuration,
                    pageViews = pageViews,
                    apihitcount
                };
                var json = JsonSerializer.Serialize(ObjDto);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _drs.SetStringAsync(
                    cachekey,
                    json,
                    options
                );
                sw.Stop();
                _logger.LogInformation("Time Elapsed (no-cache)-> " + sw.ElapsedMilliseconds + " ms");
                return Ok(ObjDto);
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