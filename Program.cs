using Google.Protobuf.WellKnownTypes;
using PortfolioAnalyticsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<GoogleAnalyticsService>();
builder.Services.AddScoped<RedisService>();
builder.Services.AddHttpClient();


// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
           policy.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod();
              
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // app.UseSwaggerUI(options =>
    // {
    //     options.SwaggerEndpoint("/openapi/v1.json", "Portfolio Analytics API");
    // });
}

app.MapGet("/", async (RedisService rc) => {
    rc.SetCount();
    var hits = await rc.GetCount();
    return "Portfolio Analytics API is running. Number of hits to api" + hits.ToString();
});
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "My API – Demo";
    c.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();