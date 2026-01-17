// using Microsoft.AspNetCore.Components.Endpoints;
// using Microsoft.AspNetCore.Mvc;
// using PortfolioAnalyticsApi.Services;
// using StackExchange.Redis;

// namespace PortfolioAnalyticsApi.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class RedisController : ControllerBase
//     {
//         RedisService rc;
//         public RedisController(RedisService rc)
//         {
//             this.rc = rc;
//         }

//         [HttpPut("putkeyval")]
//         public void PutData(string key, string value="0")
//         {
//             rc.setkeyval( key, value);
//             // rc.setExpiration( key);
//         }

//         [HttpGet("getkey")]
//         public async Task<string> GetData(string key)
//         {
//             return await rc.GetCountbyKey(key);
//         }
//         [HttpGet("delkey")]
//         public void DelData(RedisKey[] keys)
//         {
//             rc.DeleteKey(keys);
//         }
//     }
        
// }