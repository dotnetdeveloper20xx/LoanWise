using LoanWise.Api.Auth;                       // SignalRUserIdProvider
using LoanWise.API.Middlewares;                // ExceptionHandlingMiddleware (ProblemDetails)
using LoanWise.Application.DependencyInjection; // AddApplication()
using LoanWise.Infrastructure.DependencyInjection; // AddInfrastructure(), AddPersistence()
using LoanWise.Infrastructure.Notifications;   // EmailNotificationService
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

// Optional but recommended (add packages: OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.*)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Optional but recommended (add package Serilog.AspNetCore)
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// Logging (Serilog) — JSON logs, enrich with request info
// ─────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

// ─────────────────────────────────────────────
/* Layers (Clean Architecture) */
// ─────────────────────────────────────────────
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

// ─────────────────────────────────────────────
// MVC + JSON
// ─────────────────────────────────────────────
builder.Services.AddRouting(o => o.LowercaseUrls = true);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        o.JsonSerializerOptions.PropertyNamingPolicy = null; // keep DTO casing stable if desired
    });

// ─────────────────────────────────────────────
// API Versioning (v1 default) + Explorer (groups)
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

    // Include XML comments (enable <GenerateDocumentationFile/> on API csproj)
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // JWT bearer
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
builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>(); // map JWT sub -> SignalR user id

// ─────────────────────────────────────────────
// Authentication / Authorization (JWT)
// ─────────────────────────────────────────────
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"] ?? issuer;
var key = builder.Configuration["Jwt:Key"] ?? "super-secret-key"; // TODO: move to Key Vault for prod
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

        // Allow JWT via query string for SignalR hub connections
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                {
                    ctx.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Policy examples — wire these to your controllers as needed
    options.AddPolicy("IsBorrower", p => p.RequireRole("Borrower"));
    options.AddPolicy("CanDisburseLoan", p => p.RequireRole("Admin", "Lender"));
});

// ─────────────────────────────────────────────
// Rate Limiting (baseline: 60 req/min per client)
// ─────────────────────────────────────────────
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("default", opt =>
{
    opt.PermitLimit = 60;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.QueueLimit = 0;
}));

// ─────────────────────────────────────────────
// Health Checks (DB ready/live) — add more as needed
// ─────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LoanWiseDbContext>("database", HealthStatus.Unhealthy, new[] { "ready" });

// ─────────────────────────────────────────────
// OpenTelemetry (minimal) — traces for API/HTTP/EF Core
// ─────────────────────────────────────────────

builder.Services.AddOpenTelemetry()
  .ConfigureResource(r => r.AddService("LoanWise.API"))
  .WithTracing(t =>
  {
      t.AddAspNetCoreInstrumentation();
      t.AddHttpClientInstrumentation();
      //t.AddEntityFrameworkCoreInstrumentation(o =>
      //{
      //    o.SetDbStatementForText = true;
      //    o.SetDbStatementForStoredProcedure = true;
      //});
  });


// ─────────────────────────────────────────────
// Composite notifier: SignalR + Email
// ─────────────────────────────────────────────
builder.Services.AddSingleton<SignalRNotificationService>(); // stateless adapter for hub
builder.Services.AddScoped<INotificationService>(sp =>
{
    var signalr = sp.GetRequiredService<SignalRNotificationService>();
    var email = sp.GetRequiredService<EmailNotificationService>(); // from Infrastructure DI
    return new CompositeNotificationService(signalr, email);
});

// ─────────────────────────────────────────────
// Build
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// DEV‑ONLY: warn on pending EF migrations (don’t auto‑migrate in prod)
// ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LoanWiseDbContext>();
    var pending = await db.Database.GetPendingMigrationsAsync();
    if (pending.Any())
        app.Logger.LogWarning("EF Core pending migrations: {Count}. Create/apply migrations before prod.", pending.Count());
}

// --- SEED DATABASE (runs at startup) ---
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
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ─────────────────────────────────────────────
// Swagger
// ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at /
    });
}
// For production, consider enabling Swagger behind auth/proxy:
// else { app.UseSwagger(); app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1")); }

app.UseHttpsRedirection();

// ─────────────────────────────────────────────
// Security headers (baseline hardening)
// ─────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// ─────────────────────────────────────────────
// ProblemDetails middleware first, then auth
// ─────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// app.UseCors("Client"); // enable if you add a policy above

// ─────────────────────────────────────────────
// Endpoints
// ─────────────────────────────────────────────
app.MapControllers();

// Health — /health/live and /health/ready
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

// SignalR hub
app.MapHub<NotificationsHub>("/hubs/notifications");

// Tiny debug endpoint: see what routes are actually mapped
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
})
.WithName("RouteList")
.WithOpenApi();

// Serilog request logging
app.UseSerilogRequestLogging();

app.Run();
