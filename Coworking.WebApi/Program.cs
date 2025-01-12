using System.Text;
using MediatR;
using Coworking.Application.Reservations.Commands;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//1. Configuracion de la cadena de conexion a la BD
builder.Services.AddDbContext<CoworkingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//2. MediatR
builder.Services.AddMediatR(typeof(CreateReservationCommand));

// 3. Autenticación JWT
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 4. Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// 5. Caching
builder.Services.AddMemoryCache();

// 6. Registra los servicios de correo, repositorios, etc.
builder.Services.AddScoped<IEmailService, EmailService>();

// Add services to the container.
// 7. Controladores + JSON
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// 8. Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoworkingReservationSystem", Version = "v1"});
});


// Se construye la app
var app = builder.Build();

// 9. Middleware de desarrollo (Swagger)
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoworkingReservationSystem_v1"));
}

app.UseHttpsRedirection();

// Middleware de autenticacion y autorizacion
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de controladores
app.MapControllers();

app.Run();
