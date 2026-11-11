using Laqeeta.Api.Auth;
using Laqeeta.Api.Middleware;
using Laqeeta.Api.Web;
using Laqeeta.Application;
using Laqeeta.Core.Interfaces;
using Laqeeta.Infrastructure;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection"));

// ---------------------------------------------------------------- JWT auth
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 characters. " +
        "Set it with: dotnet user-secrets set \"Jwt:Key\" \"<random 32+ chars>\" (or an environment variable Jwt__Key).");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Section));
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = WebAuth.Smart;
        o.DefaultChallengeScheme = WebAuth.Smart;
    })
    .AddPolicyScheme(WebAuth.Smart, "JWT for /api, cookie for pages", o =>
    {
        o.ForwardDefaultSelector = ctx =>
            ctx.Request.Path.StartsWithSegments("/api") ? JwtBearerDefaults.AuthenticationScheme : WebAuth.Scheme;
    })
    .AddCookie(WebAuth.Scheme, o =>
    {
        o.LoginPath = "/account/login";
        o.AccessDeniedPath = "/account/denied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.Name = "laqeeta.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        // Re-check account status on every cookie-authenticated request so a disabled
        // employee/admin cannot keep using an already-issued browser session.
        o.Events.OnValidatePrincipal = async context =>
        {
            var userIdValue = context.Principal?.FindFirst(ClaimNames.UserId)?.Value;
            var role = context.Principal?.FindFirst(ClaimNames.Role)?.Value;
            if (!long.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(role))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(WebAuth.Scheme);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var isValid = await db.Users.AsNoTracking().AnyAsync(
                user => user.UserId == userId && user.IsActive && user.Role.ToString() == role);
            if (!isValid)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(WebAuth.Scheme);
            }
        };
    })
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        // Reject already-issued API tokens when an account is disabled or its role changes.
        o.Events.OnTokenValidated = async context =>
        {
            var userIdValue = context.Principal?.FindFirst(ClaimNames.UserId)?.Value;
            var role = context.Principal?.FindFirst(ClaimNames.Role)?.Value;
            if (!long.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(role))
            {
                context.Fail("Invalid account claims.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var isValid = await db.Users.AsNoTracking().AnyAsync(
                user => user.UserId == userId && user.IsActive && user.Role.ToString() == role);
            if (!isValid) context.Fail("Account is inactive or its role has changed.");
        };
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimNames.Name,
            RoleClaimType = ClaimNames.Role
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("EmployeeOnly", p => p.RequireRole("Employee"));
    o.AddPolicy("PassengerOnly", p => p.RequireRole("User"));
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// brute-force protection for login / register: 10 requests per minute per IP
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

// ---------------------------------------------------------------- MVC + Swagger
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILoc, Loc>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token returned by /api/auth/login (no 'Bearer ' prefix)."
    });
    o.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/error");

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
    await DbSeeder.SeedAdminAsync(db, builder.Configuration);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();