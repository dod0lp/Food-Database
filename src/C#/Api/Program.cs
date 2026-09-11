using Food_Database.Database.Repositories.Foods;
using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using AuthRepository = Food_Database.Database.Repositories.Auth.Repository;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("FoodDatabase");

// The existing SQL Docker setup exposes these variables from web/docker/.env.
// A direct ConnectionStrings__FoodDatabase value still takes precedence for
// production deployments or user secrets.
if (string.IsNullOrWhiteSpace(connectionString)) {
    string? address = builder.Configuration["DB_ADDRESS"];
    string? port = builder.Configuration["DB_PORT"];
    string? database = builder.Configuration["DB_NAME"];
    string? user = builder.Configuration["DB_USER"];
    string? password = builder.Configuration["DB_PASSWORD"];

    if (new[] { address, port, database, user, password }
        .Any(string.IsNullOrWhiteSpace)) {
        throw new InvalidOperationException(
            "Set ConnectionStrings:FoodDatabase or DB_ADDRESS, DB_PORT, DB_NAME, DB_USER, and DB_PASSWORD.");
    }

    connectionString = DB_Descriptors.MakeConnectionString(
        $"{address},{port}", database!, user!, password!, trustedServerCertificate: true);
}

string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddDbContext<DB_FoodContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services
    .AddIdentity<Users_DBEntity, IdentityRole<int>>(options => {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<DB_FoodContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = "food-auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<Repository>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddCors(options => options.AddPolicy("angular", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.AddRateLimiter(options => options.AddPolicy("auth", context =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        })));

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("angular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
