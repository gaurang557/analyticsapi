using System.Text.Json;
using StackExchange.Redis;

namespace PortfolioAnalyticsApi.Services{
    public class RedisService
    {
        private IDatabase db;
        public RedisService()
        {
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
        public async Task<string> SetCount()
        {
            string current_date = GetDate().ToString();
            string currentCount = db.StringGet(current_date);
            if(currentCount != null)
            {
                int count = Int32.Parse(currentCount);
                count += 1;
                db.StringSet(current_date, count.ToString());
                return db.StringGet(current_date);
            }
            db.StringSet(current_date, "0");
            return "0";
        }
        // public async Task<List<(string Date, int Count)>> GetApiHits()
        public async Task<string> GetApiHits()

        {
            var result = new List<Tuple<string, int>>();
            var keys = new RedisKey[7];
            DateOnly indiaToday = GetDate();
            for (int i = 0; i < 7; i++)
            {
                DateOnly date = indiaToday.AddDays(-i);
                string dateKeyPart = date.ToString("M/dd/yyyy");
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
        private DateOnly GetDate()
        {
            // return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;
            DateTimeOffset istTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTimeOffset.Now,
                    "India Standard Time");
            DateOnly dateOnly = DateOnly.FromDateTime(istTime.DateTime);
            return dateOnly;
        }
    }
}
