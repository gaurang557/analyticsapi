using Google.Protobuf.WellKnownTypes;
using PortfolioAnalyticsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<GoogleAnalyticsService>();
builder.Services.AddScoped<RedisService>();
builder.Services.AddHttpClient();

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
            ValidateLifetime = true // false is easier for dev
        };
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
    // app.UseSwaggerUI(options =>
    // {
    //     options.SwaggerEndpoint("/openapi/v1.json", "Portfolio Analytics API");
    // });
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
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();