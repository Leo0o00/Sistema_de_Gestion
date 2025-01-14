# Coworking Reservation System

Este proyecto es un sistema de gestión de reservas para un servicio de coworking.  
Permite listar salas disponibles, reservarlas, editar y cancelar reservas, y gestionar usuarios con roles (Usuario y Admin).

## Tecnologías Principales
- .NET 8
- ASP.NET Core
- Entity Framework Core
- MediatR (CQRS)
- JWT para autenticación
- xUnit para pruebas

## Requisitos
- SDK de .NET 8
- Postgres (ajustar la cadena de conexión)

## Configuración del Entorno
1. Clonar el repositorio.
2. Ubicarse en la carpeta `Coworking.WebApi`:
   ```bash
   cd src/Coworking.WebApi

3. Renombrar el archivo example.appsettings.json a appsettings.json y ajustar la cadena de conexión y clave JWT deseada.
4. Aplicar migraciones:
    ```bash
   dotnet ef migrations add InitialCreate --project ../Coworking.Infrastructure --context CoworkingDbContext
   dotnet ef database update --project ../Coworking.Infrastructure --context CoworkingDbContext

5. Ejecutar el proyecto:
    ```bash
   dotnet run

6. Revisar la documentación de la API en Swagger:
    ```bash
   http://localhost:5221/swagger
   

## Para correr los test unitarios
1. Ubicarse en la carpeta principal de la solucion
2. Ejecutar los tests
   ```bash
   dotnet test .\Coworking.UnitTest\



## Diagrama de la base de datos simplificado

        +-------------+
        |    User     |
        |-------------|
        | Id (PK)     |
        | Username    |
        | Password    |
        | Role        |
        | CreatedAt   |
        +------+------+
               |
               | 1 - * 
               |
        +------v--------+
        | Reservation   |
        |-------------- |
        | Id (PK)       |
        | RoomId (FK)   |
        | UserId (FK)   |
        | StartTime     |
        | EndTime       |
        | CreatedAt     |
        | ModifiedAt    |
        | IsCancelled   |
        +------+-------+
               |
               | 1 - *
               |
        +------v--------+
        | AuditLog      |
        |-------------- |
        | Id (PK)       |
        | ReservationId |
        | Action        |
        | Timestamp     |
        | Details       |
        +--------------+
               ^
               |
        +------v------+
        |    Room     |
        |-------------|
        | Id (PK)     |
        | Name        |
        | Location    |
        | Capacity    |
        | IsActive    |
        +-------------+

