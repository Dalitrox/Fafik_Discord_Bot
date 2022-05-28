using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fafik.Data.Context
{
    public class FafikDatabaseContextFactory : IDesignTimeDbContextFactory<FafikDatabaseContext>
    {
        public FafikDatabaseContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseMySql(configuration.GetConnectionString("Default"),
                new MySqlServerVersion(new Version(8, 0, 27)));

            return new FafikDatabaseContext(optionsBuilder.Options);
        }
    }
}
