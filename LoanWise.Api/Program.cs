using LoanWise.Api.Auth;                              // SignalRUserIdProvider
using LoanWise.Application.DependencyInjection;       // AddApplication()
using LoanWise.Infrastructure.DependencyInjection;    // AddInfrastructure(), AddPersistence()
using LoanWise.Infrastructure.Notifications;          // EmailNotificationService (registered in Infrastructure)
using LoanWise.Persistence.Context;
using LoanWise.Persistence.Setup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "LoanWise API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}'",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// ─────────────────────────────────────────────
// SignalR (in-app notifications)
// ─────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>(); // maps JWT user id to SignalR User()

// ─────────────────────────────────────────────
// Authentication / Authorization
// ─────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
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
// CORS (if a SPA runs on a different origin)
// ─────────────────────────────────────────────
// builder.Services.AddCors(o => o.AddPolicy("Client", p =>
//     p.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
//      .WithOrigins("http://localhost:3000", "http://localhost:5173")));

// ─────────────────────────────────────────────
// Composite notifier: SignalR + Email
// ─────────────────────────────────────────────
// EmailNotificationService is registered in AddInfrastructure(); SignalR adapter lives in API.
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


// --- SEED DATABASE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<LoanWiseDbContext>();
        var logger = services.GetService<ILogger<Program>>();

        await DbInitializer.InitializeAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// app.UseCors("Client"); // uncomment if you enabled the CORS policy above

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map the notifications hub
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
