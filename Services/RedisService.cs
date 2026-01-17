using System.Text.Json;
using StackExchange.Redis;

namespace PortfolioAnalyticsApi.Services{
    public class RedisService
    {
        private IDatabase db;
        private readonly ILogger<RedisService> _logger;
        public RedisService(ILogger<RedisService> logger)
        {
            _logger = logger;
            var muxer = ConnectionMultiplexer.Connect(
                new ConfigurationOptions{
                    EndPoints= { {"redis-15231.c56.east-us.azure.cloud.redislabs.com", 15231} },
                    User="default",
                    Password=Environment.GetEnvironmentVariable("REDIS_PASSWORD")
                }
            );
            db = muxer.GetDatabase();
        }
        public async Task<string> GetCount()
        {
            string current_date = GetDate().ToString();
            string currentCount = db.StringGet(current_date);
            if(currentCount != null)
            {
                return currentCount;
            }
            db.StringSet(current_date, "0");
            return "0";
        }
        public async Task<string> GetCountbyKey(string key)
        {
            string Count = db.StringGet(key);
            if(Count != null)
            {
                return Count;
            }
            return "0";
        }
        public async Task<bool> SetCount()
        {
            Tuple<DateOnly, string> dateTuple = GetDate();
            string current_date = dateTuple.Item1.ToString("MM/dd/yyyy");
            _logger.LogInformation("SetCount: current date in redis service:" + current_date);
            string currentCount = db.StringGet(current_date);
            if(currentCount != null)
            {
                int count = Int32.Parse(currentCount);
                _logger.LogInformation("SetCount: current count:" + count.ToString());
                count += 1;
                db.StringSet(current_date, count.ToString());
                return db.StringGet(current_date) == count.ToString() ? true : false;
            }
            db.StringSet(current_date, "0");
            return true;
        }
        // public async Task<List<(string Date, int Count)>> GetApiHits()
        public async Task<string> GetApiHits()

        {
            var result = new List<Tuple<string, int>>();
            var keys = new RedisKey[7];
            Tuple<DateOnly, string> indiaToday = GetDate();
            var todayDate = indiaToday.Item1;
            for (int i = 0; i < 7; i++)
            {
                DateOnly date = todayDate.AddDays(-i);
                string dateKeyPart = date.ToString("MM/dd/yyyy");
                keys[i] = dateKeyPart;
            }
            RedisValue[] response = await db.StringGetAsync(keys);
            for(int i = 0; i < 7; i++)
            {
                int count = int.TryParse((string)response[i], out int parsed)
                    ? parsed
                    : 0;
                result.Add(new Tuple<string, int>(keys[i], count));
            }

            result.Reverse();
            return JsonSerializer.Serialize(result);
        }
        public void setkeyval(string key, string value)
        {
            db.StringSet(key, value);
        }
        public static Tuple<DateOnly, string> GetDate()
        {
            // return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;
            DateTimeOffset istTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTimeOffset.Now,
                    "India Standard Time");
            DateOnly dateOnly = DateOnly.FromDateTime(istTime.DateTime);
            return new Tuple<DateOnly, string>(dateOnly, istTime.ToString());
        }
    }
}
