using LoanWise.Api.Auth;                        // SignalRUserIdProvider
using LoanWise.API.Middlewares;                 // ExceptionHandlingMiddleware (ProblemDetails)
using LoanWise.Application.DependencyInjection; // AddApplication()
using LoanWise.Infrastructure.DependencyInjection; // AddInfrastructure(), AddPersistence()
using LoanWise.Infrastructure.Notifications;    // EmailNotificationService
using LoanWise.Persistence.Context;
using LoanWise.Persistence.Setup;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;

using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// Logging (Serilog)
// ─────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// ─────────────────────────────────────────────
// Register application layers (Clean Architecture)
// ─────────────────────────────────────────────
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

// ─────────────────────────────────────────────
// MVC + JSON settings
// ─────────────────────────────────────────────
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

// ─────────────────────────────────────────────
// API Versioning + Explorer
// ─────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ─────────────────────────────────────────────
// Swagger + JWT auth
// ─────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LoanWise API", Version = "v1" });

    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

// ─────────────────────────────────────────────
// SignalR (in-app notifications)
// ─────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();

// ─────────────────────────────────────────────
// Authentication / Authorization (JWT)
// ─────────────────────────────────────────────
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"] ?? issuer;
var key = builder.Configuration["Jwt:Key"] ?? "super-secret-key"; // TODO: Key Vault for prod
var keyBytes = Encoding.UTF8.GetBytes(key);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
            ValidateAudience = !string.IsNullOrWhiteSpace(audience),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Allow JWT via query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsBorrower", p => p.RequireRole("Borrower"));
    options.AddPolicy("CanDisburseLoan", p => p.RequireRole("Admin", "Lender"));
});

// ─────────────────────────────────────────────
// Rate Limiting
// ─────────────────────────────────────────────
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("default", opt =>
{
    opt.PermitLimit = 60;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.QueueLimit = 0;
}));

// ─────────────────────────────────────────────
// Health Checks (DB ready/live)
// ─────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LoanWiseDbContext>("database", HealthStatus.Unhealthy, new[] { "ready" });

// ─────────────────────────────────────────────
// OpenTelemetry (ASP.NET + HttpClient + optional EFCore)
// ─────────────────────────────────────────────
builder.Services.AddOpenTelemetry()
  .ConfigureResource(r => r.AddService("LoanWise.API"))
  .WithTracing(t =>
  {
      t.AddAspNetCoreInstrumentation();
      t.AddHttpClientInstrumentation();
      // Requires OpenTelemetry.Instrumentation.EntityFrameworkCore package
      //t.AddEntityFrameworkCoreInstrumentation(o =>
      //{
      //    o.SetDbStatementForText = true;
      //    o.SetDbStatementForStoredProcedure = true;
      //});

      // Optional: export to Azure Monitor / App Insights
      // t.AddAzureMonitorTraceExporter(o =>
      //     o.ConnectionString = builder.Configuration["OpenTelemetry:AzureMonitorConnectionString"]);
  });

// ─────────────────────────────────────────────
// Composite notifier: SignalR + Email
// ─────────────────────────────────────────────
builder.Services.AddSingleton<SignalRNotificationService>();
builder.Services.AddScoped<INotificationService>(sp =>
{
    var signalr = sp.GetRequiredService<SignalRNotificationService>();
    var email = sp.GetRequiredService<EmailNotificationService>();
    return new CompositeNotificationService(signalr, email);
});

// ─────────────────────────────────────────────
// CORS (configurable; defaults for Vite 5173 http/https)
// ─────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173", "https://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition")
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

// ─────────────────────────────────────────────
// Build app
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// Dev-only migration warning
// ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LoanWiseDbContext>();
    var pending = await db.Database.GetPendingMigrationsAsync();
    if (pending.Any())
        app.Logger.LogWarning("EF Core pending migrations: {Count}.", pending.Count());
}

// ─────────────────────────────────────────────
// Seed database at startup
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<LoanWiseDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(db, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during DB seeding.");
    }
}

// ─────────────────────────────────────────────
// Swagger (dev)
// ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ─────────────────────────────────────────────
// Pipeline order (Routing → CORS → security → endpoints)
// ─────────────────────────────────────────────
app.UseRouting();

// CORS must be after routing and before auth
app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}



// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// Global exception handler (ProblemDetails)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// ─────────────────────────────────────────────
// Endpoints
// ─────────────────────────────────────────────
app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});
app.MapHub<NotificationsHub>("/hubs/notifications");

// Debug route list
app.MapGet("/_debug/routes", (IEnumerable<EndpointDataSource> sources) =>
{
    var routes = sources.SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            Route = e.RoutePattern.RawText,
            Methods = string.Join(",", e.Metadata
                .OfType<HttpMethodMetadata>()
                .FirstOrDefault()?.HttpMethods ?? Array.Empty<string>())
        })
        .OrderBy(x => x.Route);
    return Results.Ok(routes);
}).WithName("RouteList").WithOpenApi();

app.UseSerilogRequestLogging();
app.Run();
