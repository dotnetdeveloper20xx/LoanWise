using LoanWise.Application.DependencyInjection;
using LoanWise.Infrastructure.DependencyInjection;
using LoanWise.Infrastructure.Notifications;
using LoanWise.Persistence.Context;
using LoanWise.Persistence.Setup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// Register Services
// ─────────────────────────────────────────────

// Clean Architecture layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSingleton<SignalRNotificationService>();   // API adapter
builder.Services.AddScoped<EmailNotificationService>();        // from Infra registration above if not already
builder.Services.AddScoped<INotificationService>(sp =>
{
    var signalr = sp.GetRequiredService<SignalRNotificationService>();
    var email = sp.GetRequiredService<EmailNotificationService>();
    return new CompositeNotificationService(signalr, email);
});



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
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            jwtScheme,
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────
// JWT Authentication
// ─────────────────────────────────────────────

var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Set to true if using issuer validation
            ValidateAudience = false, // Set to true if using audience validation
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ─────────────────────────────────────────────
// Configure HTTP Pipeline
// ─────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanWise API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationsHub>("/hubs/notifications");

//// Optional: DB seeding logic
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<LoanWiseDbContext>();
//    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
//    await DbInitializer.InitializeAsync(dbContext, logger);
//}

app.Run();
