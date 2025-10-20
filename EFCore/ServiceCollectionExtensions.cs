using Abstractions;
using EFCore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoggingProviderEfCore(this IServiceCollection services, IConfiguration cfg, string connectionName = "LoggingDb")
        {
            var cs = cfg.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException($"ConnectionStrings:{connectionName} not found.");

            services.AddDbContext<LogDbContext>(opt =>
                opt.UseSqlServer(cs, sql => sql.EnableRetryOnFailure().CommandTimeout(10)));

            services.AddScoped<ILogWriter, LogWriter>();
            return services;
        }

        public static async Task ApplyLoggingProviderMigrationsAsync(this IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LogDbContext>();
            if ((await db.Database.GetPendingMigrationsAsync()).Any())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync(); // ← fallback وقتی مایگریشنی نداریم
        }
    }
}
