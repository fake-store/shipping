using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Shipments.Repositories;
using Shipments.Services;

var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "shipping";
var hostName    = Environment.MachineName;
var logRoot     = Environment.GetEnvironmentVariable("LOG_PATH") ?? "/logs";
var logBasePath = $"{logRoot}/{serviceName}/{hostName}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u5} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: $"{logBasePath}/{{Date}}/{serviceName}.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u5} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// JWT auth
var jwtSecret = builder.Configuration["Jwt:Secret"] is { Length: > 0 } s
    ? s
    : Environment.GetEnvironmentVariable("JWT_SECRET")
      ?? throw new InvalidOperationException("JWT_SECRET is required");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Services
builder.Services.AddSingleton<IAddressRepository, InMemoryAddressRepository>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
