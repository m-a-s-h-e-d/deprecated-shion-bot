using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shion.Data.Context
{
    /// <summary>
    /// The <see cref="IDbContextFactory"/> that produces a preconfigured <see cref="ShionDbContext"/> for the Shion project.
    /// </summary>
    public class ShionDbContextFactory : IDesignTimeDbContextFactory<ShionDbContext>
    {
        /// <summary>
        /// Creates a <see cref="ShionDbContext"/> with the configurations passed in the arguments.
        /// </summary>
        /// <param name="args">The <see cref="string"/> arguments to be passed.</param>
        /// <returns>A <see cref="ShionDbContext"/> representing the configured custom <see cref="DbContext"/>.</returns>
        public ShionDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseMySql(
                configuration.GetConnectionString("Default"),
                new MySqlServerVersion(new Version(8, 0, 24)));

            return new ShionDbContext(optionsBuilder.Options);
        }
    }
}
