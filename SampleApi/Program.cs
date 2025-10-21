using Abstractions;
using AspNetCore;
using AspNetCore.Filters;
using AspNetCore.Options;
using EFCore.Persistence;
using LoggingProviderService.AspNetCore;
using LoggingProviderService.EFCore;
using LoggingProviderService.EFCore.Persistence.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace SampleApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SampleApi (LoggingProvider)",
                    Version = "v1"
                });
            });


            // رجیستر EF Core داینامیک
            builder.Services.AddLoggingProviderEfCoreDynamic(builder.Configuration, "LoggingDb");

            // (اختیاری) لاگ فیلتر ASP.NET Core — اگر خواستی
            //builder.Services.AddLoggingProvider(builder.Configuration, global: false);
            builder.Services.AddScoped<DynamicRequestLoggingMiddleware>(); // مهم

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleApi v1");
                c.RoutePrefix = "swagger"; // یعنی آدرس: /swagger
            });

            app.UseHttpsRedirection();  // اگر HTTPS نداری می‌تونی فعلاً برداری

            app.UseMiddleware<DynamicRequestLoggingMiddleware>();

            // ساخت دیتابیس/تیبل‌ها بار اول
            await app.Services.EnsureCreatedAsync<LogDbContextDynamic>();

            app.UseDynamicRequestLogging();

            app.MapControllers();
            await app.RunAsync();
        }
    }
}
