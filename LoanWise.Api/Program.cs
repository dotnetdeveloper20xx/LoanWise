using LoanWise.Api.Auth;                       // SignalRUserIdProvider         
using LoanWise.Application.DependencyInjection; // AddApplication()
using LoanWise.Infrastructure.DependencyInjection; // AddInfrastructure(), AddPersistence()
using LoanWise.Infrastructure.Notifications;    // EmailNotificationService
using LoanWise.Persistence.Context;
using LoanWise.Persistence.Setup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// Layers (Clean Architecture)
// ─────────────────────────────────────────────
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

// ─────────────────────────────────────────────
// MVC + Swagger
// ─────────────────────────────────────────────
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LoanWise API", Version = "v1" });

    // Include XML comments (if you enable <GenerateDocumentationFile/> on your API csproj)
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // JWT bearer
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",                       // must be "bearer"
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
var audience = builder.Configuration["Jwt:Audience"] ?? issuer; // often same
var key = builder.Configuration["Jwt:Key"] ?? "super-secret-key";
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

builder.Services.AddAuthorization();

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
// Build & Pipeline
// ─────────────────────────────────────────────
var app = builder.Build();

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

// Always expose Swagger in Dev; optionally enable in other envs too
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at /
    });
}
// Uncomment to always show Swagger (even in Prod behind auth/proxy)
// else
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1"));
// }

app.UseHttpsRedirection();

// app.UseCors("Client"); // enable if you add a policy above

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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

app.Run();
