
using LMS.Application.Common;
using LMS.Application.Features.Assignment.Services;
using LMS.Application.Features.Auth.Services.Admin;
using LMS.Application.Features.Auth.Services.Student;
using LMS.Application.Features.Auth.Services.Trainer;
using LMS.Application.Features.Course.Services;
using LMS.Infrastructure.Authentication;
using LMS.Infrastructure.Configurations;
using LMS.Infrastructure.Email;
using LMS.Infrastructure.Repositories.Admin;
using LMS.Infrastructure.Repositories.Assignment;
using LMS.Infrastructure.Repositories.Auth;
using LMS.Infrastructure.Repositories.Course;
using LMS.Infrastructure.Repositories.Student;
using LMS.Infrastructure.Repositories.Trainer;
using LMS.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000;
});

#region JWT Configuration

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()!;

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });

#endregion

#region Authorization

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

#endregion

#region Controllers

builder.Services.AddControllers();

#endregion

#region Dependency Injection

builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<StudentRepository>();
builder.Services.AddScoped<TrainerRepository>();
builder.Services.AddScoped<AdminRepository>();

builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();

builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();

builder.Services.Configure<AzureStorageSettings>(
    builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

builder.Services.Configure<BrevoSettings>(
    builder.Configuration.GetSection("BrevoSettings"));

builder.Services.AddHttpClient<EmailService>();

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS Portal API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});
#endregion

var app = builder.Build();

#region Middleware

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();

internal class OpenApiReference
{
    public ReferenceType Type { get; set; }
    public string Id { get; set; }
}