using System.Text;
using Coworking.Application.Handlers.Reservations;
using Coworking.Application.Handlers.Rooms;
using Coworking.Application.Handlers.Users;
using MediatR;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Commands.Reservations;
using Coworking.Infrastructure.Commands.Rooms;
using Coworking.Infrastructure.Commands.Users;
using Coworking.Infrastructure.Queries.Reservations;
using Coworking.Infrastructure.Queries.Rooms;
using Coworking.Infrastructure.Repositories;
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
builder.Services.AddMediatR(cfg =>
{
    //Commands
    //Reservations 
    cfg.RegisterServicesFromAssembly(typeof(CancelReservationCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CancelReservationHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateReservationCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateReservationHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(DeleteReservationCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(DeleteReservationHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(EditReservationCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(EditReservationHandler).Assembly);
    //Rooms
    cfg.RegisterServicesFromAssembly(typeof(CreateRoomCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateRoomHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(EditRoomCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(EditRoomHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RemoveRoomCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RemoveRoomHandler).Assembly);
    //Users
    cfg.RegisterServicesFromAssembly(typeof(ChangeUserRoleCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(ChangeUserRoleHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(LoginUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(LoginUserHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserHandler).Assembly);
    
    //Queries
    //Reservations
    cfg.RegisterServicesFromAssembly(typeof(GetReservationByIdQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetReservationByIdHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAllReservationsQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAllReservationsHandler).Assembly);
    //Rooms
    cfg.RegisterServicesFromAssembly(typeof(GetAllRoomsQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAllRoomsHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAvailableRoomsQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAvailableRoomsCachedHandler).Assembly); //Request cacheada en memoria
    cfg.RegisterServicesFromAssembly(typeof(GetRoomByIdQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetRoomByIdHandler).Assembly);
    
    
});

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
builder.Services.AddScoped<IReservationsRepository, ReservationsRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IUsersRepository, UserRepository>();
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
