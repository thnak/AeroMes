using AeroMes.Api.Identity;
using AeroMes.Application.Interfaces;
using AeroMes.Application.WorkOrders.Commands.StartWorkOrder;
using AeroMes.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(StartWorkOrderHandler).Assembly));

builder.Services.AddScoped<ITokenService, TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

// Support both Cookie (browser) and JWT Bearer (tablet/mobile/API)
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "MultiAuth";
        options.DefaultChallengeScheme = "MultiAuth";
    })
    .AddPolicyScheme("MultiAuth", "Cookie or JWT Bearer", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var auth = context.Request.Headers.Authorization.FirstOrDefault();
            return auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? JwtBearerDefaults.AuthenticationScheme
                : IdentityConstants.ApplicationScheme;
        };
    })
    .AddCookie(IdentityConstants.ApplicationScheme, opts =>
    {
        opts.Cookie.Name = "AeroMes.Auth";
        opts.Cookie.HttpOnly = true;
        opts.Cookie.SameSite = SameSiteMode.Lax;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opts.SlidingExpiration = true;
        opts.ExpireTimeSpan = TimeSpan.FromHours(8); // one full shift
        // Return 401/403 instead of redirecting for API clients
        opts.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        opts.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
