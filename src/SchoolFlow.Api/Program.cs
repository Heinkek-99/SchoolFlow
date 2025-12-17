using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolFlow.Application;
using SchoolFlow.Infrastructure;
using SchoolFlow.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. LOGGING (Serilog)
// ============================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/schoolflow-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================
// 2. SERVICES
// ============================================

// Add Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
// builder.Services.AddValidatorsFromAssemblyContaining<Application.DependencyInjection>();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Application Layer (MediatR + Validators)
builder.Services.AddApplication();

// Infrastructure Layer (EF Core + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// ============================================
// 2.1 CONFIGURATION DU DbContext
// ============================================
// Chaîne de connexion définie dans votre configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Activer le Split Query pour éviter les warnings
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(3);
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
});


// ============================================
// 3. AUTHENTICATION JWT
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DirecteurOrAdmin", policy => policy.RequireRole("Admin", "Directeur"));
    options.AddPolicy("ComptableAccess", policy => policy.RequireRole("Admin", "Directeur", "Comptable"));
    options.AddPolicy("SecretaireAccess", policy => policy.RequireRole("Admin", "Directeur", "Secretaire"));
});

// ============================================
// 4. CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================
// 5. SWAGGER / OpenAPI
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SchoolFlow API",
        Version = "v1",
        Description = "API de gestion scolaire - SchoolFlow MVP",
        Contact = new OpenApiContact
        {
            Name = "EdifyTech",
            Email = "support@edifytech.cm"
        }
    });

    // JWT Authentication dans Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez 'Bearer' [espace] puis votre token JWT.\n\nExemple: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Inclure XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ============================================
// 6. HEALTHCHECKS
// ============================================
builder.Services.AddHealthChecks()
    // .AddDbContextCheck<ApplicationDbContext>("Database");
     .AddCheck("Database", () =>
    {
        using var scope = builder.Services.BuildServiceProvider().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            dbContext.Database.CanConnect();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection OK");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    });
// ============================================
// 7. BUILD APP
// ============================================
var app = builder.Build();

// ============================================
// 8. MIDDLEWARE PIPELINE
// ============================================

// Exception Handler (Development vs Production)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SchoolFlow API v1");
    options.RoutePrefix = string.Empty; // Swagger à la racine (http://localhost:5000/)
    options.DocumentTitle = "SchoolFlow API - Documentation";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ============================================
// 9. DATABASE MIGRATION AUTO (Dev uniquement)
// ============================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        await dbContext.Database.MigrateAsync();
        Log.Information("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ [ERROR] Error applying database migrations");
    }
}

// ============================================
// 10. RUN
// ============================================
Log.Information("🚀 SchoolFlow API starting...");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
