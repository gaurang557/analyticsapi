using Google.Protobuf.WellKnownTypes;
using PortfolioAnalyticsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<GoogleAnalyticsService>();
builder.Services.AddScoped<RedisService>();
builder.Services.AddHttpClient();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Trust all proxy networks (required for Render / cloud platforms)
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("authority");
        options.Audience = Environment.GetEnvironmentVariable("audience");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,// false is easier for dev
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window limiter - allows X requests per time window
    options.AddPolicy("fixed", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            // partitionKey: context.Request.Headers.Host.ToString(),
            // partitionKey: context.User.Identity?.Name ?? "anonymous",
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = 3,
        Window = TimeSpan.FromMinutes(1),
        // QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0
    }));
});


builder.Services.AddAuthorization();

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
}

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();


app.MapGet("/", async (RedisService rc) => {
    await rc.SetCount();
    logger.LogInformation("Incremented API hit count");
    var hits = await rc.GetCount();
    return "Portfolio Analytics API is running";
});
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "My API – Demo";
    c.RoutePrefix = "swagger";
});
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var user = context.User;
    logger.LogInformation(
    "RemoteIp: {RemoteIp} | XFF: {XFF}",
    context.Connection.RemoteIpAddress,
    context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
);
    await next();
});
app.UseRateLimiter();
app.MapControllers();

app.Run();