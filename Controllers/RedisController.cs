using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Mvc;
using PortfolioAnalyticsApi.Services;
using StackExchange.Redis;

namespace PortfolioAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        RedisService rc;
        public RedisController(RedisService rc)
        {
            this.rc = rc;
        }

        [HttpPut("putkeyval")]
        public void PutData(string key, string value)
        {
            Console.WriteLine("In put key val");
            rc.setkeyval( key, value);
        }
        [HttpGet]
        public async Task<string> Get()
        {
            string g = await rc.GetCount();
            return g;
        }

        [HttpGet("getkey")]
        public async Task<string> GetData(string key)
        {
            return await rc.GetCountbyKey(key);
        }

        [HttpGet("time")]
        public string GetDate()
        {
            DateTimeOffset istTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTimeOffset.Now,
                    "India Standard Time");
            DateOnly dateOnly = DateOnly.FromDateTime(istTime.DateTime);
            return dateOnly.ToString();
        }
    }
        
}