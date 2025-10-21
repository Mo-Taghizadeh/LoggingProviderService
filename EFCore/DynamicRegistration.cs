using LoggingProviderService.Abstractions;
using LoggingProviderService.Abstractions.Models;
using LoggingProviderService.EFCore.Persistence.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.EFCore
{
    public static class DynamicRegistration
    {
        /// ثبت DbContext داینامیک + ILogWriterDynamic
        public static IServiceCollection AddLoggingProviderEfCoreDynamic(
            this IServiceCollection services, IConfiguration cfg, string connectionName = "LoggingDb")
        {
            services.Configure<LoggingSchema>(cfg.GetSection("LoggingSchema"));

            var cs = cfg.GetConnectionString(connectionName)
                     ?? throw new InvalidOperationException($"ConnectionStrings:{connectionName} not found.");

            services.AddDbContext<LogDbContextDynamic>(opt =>
                opt.UseSqlServer(cs, sql => sql.EnableRetryOnFailure().CommandTimeout(30)));

            services.AddScoped<ILogWriterDynamic, DynamicLogWriter>();
            return services;
        }
    }
}
