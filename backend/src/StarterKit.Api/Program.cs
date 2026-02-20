using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using StarterKit.Infrastructure.Data;
using StarterKit.Infrastructure.Settings;
using StarterKit.Infrastructure.Extensions;
using StarterKit.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StarterKit API", Version = "v1" });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Paste ONLY the JWT token here (no 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(doc =>
    {
        var requirement = new OpenApiSecurityRequirement();

        // IMPORTANT: pass the host document so it serializes as { "bearer": [] }
        var bearerRef = new OpenApiSecuritySchemeReference("bearer", doc);

        requirement[bearerRef] = [];
        return requirement;
    });
});

var allowedCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

if (allowedCorsOrigins.Length == 0)
    throw new InvalidOperationException("Cors:AllowedOrigins must specify at least one origin.");

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p =>
        p.WithOrigins(allowedCorsOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});

builder.Services.AddStarterKitApplication();
builder.Services.AddStarterKitInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
          ?? throw new InvalidOperationException("Missing Jwt config.");

if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

if (jwt.RefreshTokenDays <= 0)
    throw new InvalidOperationException("Jwt:RefreshTokenDays must be greater than zero.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StarterKit API v1");
        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

// Ensure DB migrations applied on startup
if (!app.Environment.IsEnvironment("Testing"))
{
    await DbInitializer.EnsureMigratedAsync(app.Services);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
