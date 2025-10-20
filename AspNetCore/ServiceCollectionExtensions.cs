using AspNetCore.Filters;
using AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// global=true یعنی فیلتر روی همه اکشن‌ها اعمال می‌شود.
        /// اگر global=false باشد، باید روی اکشن/کنترلر از [ServiceFilter(typeof(LogActionFilter))] استفاده کنی.
        /// </summary>
        public static IServiceCollection AddLoggingProvider(
            this IServiceCollection services, IConfiguration cfg, bool global = false)
        {
            // Bind کردن تنظیمات
            services.Configure<LogOptions>(cfg.GetSection("LoggingProvider"));

            // فیلتر قابل تزریق
            services.AddScoped<LogActionFilter>();

            // اضافه کردن فیلتر به MvcOptions بعد از اینکه مصرف‌کننده AddControllers را صدا زد
            if (global)
                services.PostConfigure<MvcOptions>(o => o.Filters.AddService<LogActionFilter>());

            return services;
        }
    }
}
