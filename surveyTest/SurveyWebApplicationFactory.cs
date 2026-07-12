using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Repository.Data;

namespace surveyTest
{
    internal class SurveyWebApplicationFactory : WebApplicationFactory<Program>
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the production/development DB configuration
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                // Add the Test SQL Server DB configuration
                var connectionString = GetConnectionString();
                services.AddSqlServer<AppDbContext>(connectionString);
            });
        }

        private static string? GetConnectionString()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsetting.Test.json", optional: false, reloadOnChange: true)
            .Build();

            
            var connectionString = config.GetConnectionString("TestConnection");
            return connectionString;
        }
    }
}
