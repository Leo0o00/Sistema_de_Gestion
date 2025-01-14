using Coworking.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Donde está CoworkingDbContext

namespace IntegrationTest
{
    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            // Reconfiguramos servicios:
            builder.ConfigureServices(services =>
            {
                // Remover la configuración de la BD real
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CoworkingDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Usar InMemory en su lugar
                services.AddDbContext<CoworkingDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDB");
                });
            });
        }
    }
}