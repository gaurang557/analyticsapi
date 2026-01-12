using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortfolioAnalyticsApi.Services
{
public static class GoogleAnalyticsTracker
{
    static  string _measurementId="G-XYCHJJQEV3";
    static  string _apiSecret=Environment.GetEnvironmentVariable("apisecret") ?? "";
    static  string _clientId=Guid.NewGuid().ToString();
    static HttpClient _httpClient=new HttpClient();
    public static  async Task TrackApiHitAsync(string apiEndpoint = null)
    {
        var url = $"https://www.google-analytics.com/mp/collect?measurement_id={_measurementId}&api_secret={_apiSecret}";

        var payload = new
        {
            client_id = _clientId,
            events = new[]
            {
                new
                {
                    name = "api_hit",
                    @params = new
                    {
                        endpoint = apiEndpoint ?? "default",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Log error - don't let tracking failures break your application
            Console.WriteLine($"GA tracking failed: {ex.Message}");
        }
    }
}
}