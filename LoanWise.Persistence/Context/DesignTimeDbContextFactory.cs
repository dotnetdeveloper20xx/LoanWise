using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LoanWise.Persistence.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LoanWiseDbContext>
    {
        public LoanWiseDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../LoanWise.Api"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true) // 👈 add this line
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("DefaultConnection string not found in configuration.");

            var optionsBuilder = new DbContextOptionsBuilder<LoanWiseDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new LoanWiseDbContext(optionsBuilder.Options);
        }
    }
}
