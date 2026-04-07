using EDMS.Core.Queue;
using System.Text;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Auth;
using EDMS.Infrastructure.Data;
using EDMS.Infrastructure.Repositories;
using EDMS.API.Hubs;
using EDMS.API.Middleware;
using EDMS.API.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("SupabaseConnection");
    if (string.IsNullOrWhiteSpace(connection))
    {
        options.UseInMemoryDatabase("edms-dev");
    }
    else
    {
        options.UseNpgsql(connection);
    }
});

builder.Services.AddSingleton<IQueueEngine, MM1Engine>();
builder.Services.AddSingleton<IQueueEngine, MG1Engine>();
builder.Services.AddSingleton<IQueueEngine, GG1Engine>();
builder.Services.AddSingleton<ModelSelectorService>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IStaffUserRepository, StaffUserRepository>();
builder.Services.AddScoped<IAllergyRegistryRepository, AllergyRegistryRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ClinicalService>();
builder.Services.AddScoped<NotificationService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-key-change-this-in-production-32chars";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EDMS.API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EDMS.Client",
            IssuerSigningKey = signingKey
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/queue"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StaffPolicy", p => p.RequireRole("Staff", "Nurse", "Doctor", "Admin"));
    options.AddPolicy("ClinicalPolicy", p => p.RequireRole("Nurse", "Doctor", "Admin"));
    options.AddPolicy("DoctorPolicy", p => p.RequireRole("Doctor", "Admin"));
    options.AddPolicy("AdminPolicy", p => p.RequireRole("Admin"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<QueueHub>("/hubs/queue");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();
    await DbInitializer.SeedAsync(db, passwordService);
}

app.Run();
