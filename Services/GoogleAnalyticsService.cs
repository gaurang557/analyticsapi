using Google.Analytics.Data.V1Beta;
using Google.Apis.Auth.OAuth2;
using System.Text;

namespace PortfolioAnalyticsApi.Services
{
    public class GoogleAnalyticsService
    {
        private readonly BetaAnalyticsDataClient _client;
        private readonly string _propertyId;
        public GoogleAnalyticsService(IConfiguration configuration)
        {
            Console.WriteLine("GoogleAnalyticsService initialized.");
            _propertyId = configuration["GoogleAnalytics:PropertyId"]
                ?? throw new ArgumentNullException("PropertyId not configured");
            // var credentialPath = configuration["GoogleAnalytics:CredentialPath"];
            // var credentialJson = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");
            var b64 = Environment.GetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS_JSON_B64");

            if (string.IsNullOrWhiteSpace(b64))
            {
                throw new Exception("Missing Google credentials");
            }

            var credentialJson = Encoding.UTF8.GetString(
                Convert.FromBase64String(b64));
            if (!string.IsNullOrEmpty(credentialJson))
            {
                Console.WriteLine("Using credential from environment variable.");
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialJson));

                var credential = ServiceAccountCredential.FromServiceAccountData(stream);

                _client = new BetaAnalyticsDataClientBuilder
                {
                    Credential = credential
                }.Build();
            }
            else
            {
                // credentialJson = configuration["GoogleAnalytics:CredentialJson"];
                // if (!string.IsNullOrEmpty(credentialJson))
                // {
                //     var credentialStream = new MemoryStream(Encoding.UTF8.GetBytes(credentialJson));
                //     var credential = ServiceAccountCredential.FromServiceAccountData(credentialStream);
                    
                //     _client = new BetaAnalyticsDataClientBuilder
                //     {
                //         Credential = credential
                //     }.Build();
                // }
                // else
                // {
                    _client = BetaAnalyticsDataClient.Create();
                // }
            }
        }

        public async Task<AnalyticsData> GetAnalyticsDataAsync()
        {
            var request = new RunReportRequest
            {
                Property = $"properties/{_propertyId}",
                DateRanges = { new DateRange { StartDate = "30daysAgo", EndDate = "today" } },
                Metrics =
                {
                    new Metric { Name = "screenPageViews" },
                    new Metric { Name = "activeUsers" },
                    new Metric { Name = "sessions" },
                    new Metric { Name = "averageSessionDuration" }
                }
            };

            var response = await _client.RunReportAsync(request);

            var totalViews = 0L;
            var totalUsers = 0L;
            var totalSessions = 0L;
            var totalDuration = 0.0;

            foreach (var row in response.Rows)
            {
                totalViews += long.Parse(row.MetricValues[0].Value);
                totalUsers += long.Parse(row.MetricValues[1].Value);
                totalSessions += long.Parse(row.MetricValues[2].Value);
                totalDuration += double.Parse(row.MetricValues[3].Value);
            }

            var avgDuration = response.Rows.Count > 0 
                ? totalDuration / response.Rows.Count 
                : 0;

            return new AnalyticsData
            {
                TotalViews = totalViews,
                TotalUsers = totalUsers,
                Sessions = totalSessions,
                AvgDuration = (int)avgDuration
            };
        }

        public async Task<List<PageViewData>> GetWeeklyPageViewsAsync()
        {
            var request = new RunReportRequest
            {
                Property = $"properties/{_propertyId}",
                DateRanges = { new DateRange { StartDate = "7daysAgo", EndDate = "today" } },
                Dimensions = { new Dimension { Name = "date" } },
                Metrics = { new Metric { Name = "screenPageViews" } },
                OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "date" } } }
            };

            var response = await _client.RunReportAsync(request);

            var pageViews = new List<PageViewData>();
            
            foreach (var row in response.Rows)
            {
                var date = row.DimensionValues[0].Value;
                var views = long.Parse(row.MetricValues[0].Value);
                
                var dateTime = DateTime.ParseExact(date, "yyyyMMdd", null);
                
                pageViews.Add(new PageViewData
                {
                    Day = dateTime.ToString("ddd"),
                    Views = views,
                    Date = dateTime.ToString("yyyy-MM-dd")
                });
            }

            return pageViews;
        }
    }

    public class AnalyticsData
    {
        public long TotalViews { get; set; }
        public long TotalUsers { get; set; }
        public long Sessions { get; set; }
        public int AvgDuration { get; set; }
    }

    public class PageViewData
    {
        public string Day { get; set; } = string.Empty;
        public long Views { get; set; }
        public string Date { get; set; } = string.Empty;
    }
}