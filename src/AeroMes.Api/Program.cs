using AeroMes.Api.Auth;
using AeroMes.Api.Hubs;
using AeroMes.Api.Identity;
using AeroMes.Api.Middleware;
using AeroMes.Api.OpenApi;
using AeroMes.Api.Services;
using Scalar.AspNetCore;
using AeroMes.Application;
using AeroMes.Application.Interfaces;
using AeroMes.Infrastructure;
using AeroMes.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Constants;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Give in-flight requests (including SignalR) 30 s to drain on SIGTERM.
builder.Services.Configure<HostOptions>(opts =>
    opts.ShutdownTimeout = TimeSpan.FromSeconds(30));

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new ApiJsonContext());
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, new ApiJsonContext());
});
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opts.SerializerOptions.TypeInfoResolverChain.Insert(0, new ApiJsonContext());
});
builder.Services.AddOpenApi(opts =>
{
    opts.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    opts.AddOperationTransformer<AuthOperationTransformer>();
});
builder.Services.AddProblemDetails();

builder.Services.AddApplication();        // MediatR + FluentValidation + ValidationBehavior
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddScoped<IModuleStatusNotifier, ModuleStatusNotifier>();
builder.Services.AddSingleton<IotHubNotifier>();
builder.Services.AddSingleton<IIotHubNotifier>(sp => sp.GetRequiredService<IotHubNotifier>());
builder.Services.AddSingleton<AeroMes.Application.Interfaces.IIotSignalNotifier>(sp => sp.GetRequiredService<IotHubNotifier>());

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();

// Permission-based authorization
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var allPermissions = AeroMes.Api.Auth.Permissions.All;
builder.Services.AddAuthorizationBuilder()
    .AddDefaultPolicy("default", p => p.RequireAuthenticatedUser());

// Register one policy per permission code
builder.Services.AddAuthorization(opts =>
{
    foreach (var code in allPermissions)
        opts.AddPolicy($"permission:{code}",
            p => p.AddRequirements(new PermissionRequirement(code)));
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

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
            if (auth?.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase) == true)
                return ApiKeyAuthenticationHandler.SchemeName;
            if (auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
                return JwtBearerDefaults.AuthenticationScheme;
            return IdentityConstants.ApplicationScheme;
        };
    })
    .AddCookie(IdentityConstants.ApplicationScheme, opts =>
    {
        opts.Cookie.Name = "AeroMes.Auth";
        opts.Cookie.HttpOnly = true;
        opts.Cookie.SameSite = SameSiteMode.Lax;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opts.SlidingExpiration = true;
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
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
    })
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, null);

builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

var app = builder.Build();

// Only the primary instance runs migrations.
// Secondary replicas set SKIP_MIGRATIONS=true to avoid startup race conditions.
if (!app.Configuration.GetValue<bool>("SKIP_MIGRATIONS"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AeroMes.Infrastructure.Data.AppDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.Use(ExceptionMiddleware.InvokeAsync);   // must be first — catches all downstream exceptions
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.Use(ForcePasswordChangeMiddleware.InvokeAsync);
app.Use(MfaEnforcementMiddleware.InvokeAsync);
app.MapControllers();
app.MapHub<ModuleStatusHub>("/hubs/module-status");
app.MapHub<IotHub>("/hubs/iot");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
