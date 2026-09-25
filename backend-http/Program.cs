using Food_Database.Database.Repositories.Foods;
using Food_Database.Database.Descriptors;
using Food_Database.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using AuthRepository = Food_Database.Database.Repositories.Auth.Repository;

var builder = WebApplication.CreateBuilder(args);

string connectionString = DB_Food_Descriptors.GetConnectionString(
    builder.Configuration.GetConnectionString("FoodDatabase"),
    builder.Configuration["DB_ADDRESS"],
    builder.Configuration["DB_PORT"],
    builder.Configuration["DB_NAME"],
    builder.Configuration["DB_USER"],
    builder.Configuration["DB_PASSWORD"]);

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
builder.Services.AddProblemDetails();
builder.Services.AddScoped<Repository>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddCors(options => options.AddPolicy("angular", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// podla tutorialu
app.UseExceptionHandler(errorApp => errorApp.Run(async context => {
    Exception? exception = context.Features
        .Get<IExceptionHandlerFeature>()?.Error;

    if (exception is null) {
        return;
    }

    ILogger<Program> logger = context.RequestServices
        .GetRequiredService<ILogger<Program>>();
    logger.LogError(exception,
        "Unhandled error while processing {Method} {Path}",
        context.Request.Method, context.Request.Path);

    int statusCode;
    string title;
    string detail;

    if (exception is DbUpdateException) {
        statusCode = StatusCodes.Status409Conflict;
        title = "Database update could not be completed";
        detail = "Error with database operations. Refresh and try again.";
    } else if (IsDatabaseUnavailable(exception)) {
        statusCode = StatusCodes.Status503ServiceUnavailable;
        title = "Database temporarily unavailable";
        detail = "Try again shortly.";
    } else {
        statusCode = StatusCodes.Status500InternalServerError;
        title = "Unexpected server error";
        detail = "Try again later.";
    }

    await Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail)
        .ExecuteAsync(context);
}));
app.UseForwardedHeaders();
app.UseCors("angular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static bool IsDatabaseUnavailable(Exception exception) {
    return 
        (exception is SqlException or TimeoutException)
            || exception.InnerException is not null
            && IsDatabaseUnavailable(exception.InnerException);
}
