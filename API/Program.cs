using BLL;
using DAO;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.OpenApi.Any;
using API.Controllers.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

if (string.IsNullOrEmpty(jwtKey))
    throw new ArgumentNullException(nameof(jwtKey), "JWT Key is not configured");
if (string.IsNullOrEmpty(jwtIssuer))
    throw new ArgumentNullException(nameof(jwtIssuer), "JWT Issuer is not configured");
if (string.IsNullOrEmpty(jwtAudience))
    throw new ArgumentNullException(nameof(jwtAudience), "JWT Audience is not configured");

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
        ValidIssuer = builder.Configuration[jwtIssuer],
        ValidAudience = builder.Configuration[jwtAudience],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<UserDto>(() => new OpenApiSchema
    {
        Type = "object",
        Example = new OpenApiObject
        {
            ["nombre"] = new OpenApiString("John Doe"),
            ["edad"] = new OpenApiInteger(30),
            ["email"] = new OpenApiString("john@example.com"),
            ["dni"] = new OpenApiInteger(38112194),
            ["password"] = new OpenApiString("yourPassword123")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token in the format 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddScoped<IDataAccess>(provider =>
    new DataAccess(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        provider.GetRequiredService<ILogger<DataAccess>>()
    )
);

builder.Services.AddScoped<Login>();
builder.Services.AddScoped<IBusinessLayer, BusinessLayer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();