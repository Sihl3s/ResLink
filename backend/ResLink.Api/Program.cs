using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ResLink.Api.Auth;
using ResLink.Api.Data;
using ResLink.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers and JWT bearer auth follow the ASP.NET Core 9 pipeline (Microsoft, 2025a; Microsoft, 2025b).
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ResLink API",
        Version = "v1",
        Description = "Student residence engagement API for Dynamic Developers."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Local SQLite for development; hosted runs use a writable folder (Microsoft, 2025c; SQLite Consortium, 2025).
var hosted = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RENDER"))
    || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT"));
var dataDir = Environment.GetEnvironmentVariable("RESLINK_DATA_DIR");
if (string.IsNullOrWhiteSpace(dataDir))
{
    dataDir = Directory.Exists("/home/data") ? "/home/data" : hosted ? "/tmp/reslink" : ".";
}
Directory.CreateDirectory(dataDir);
var sqlitePath = hosted
    ? $"Data Source={Path.Combine(dataDir, "reslink.db")}"
    : builder.Configuration.GetConnectionString("Default") ?? $"Data Source={Path.Combine(dataDir, "reslink.db")}";

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqlitePath));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PointsService>();

// Issuer, audience and signing-key checks match Microsoft's JWT bearer guidance (Microsoft, 2025b; OWASP, 2023).
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// CORS is open for the prototype so the browser UI and emulator can share one API (Microsoft, 2025a).

builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

// OpenAPI/Swagger documents the REST surface for marking (Microsoft, 2025d; OpenAPI Initiative, 2024).
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ResLink API v1");
    options.RoutePrefix = "swagger";
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
