using AspNetCore.Filters;
using AspNetCore.Options;
using Abstractions;
using EFCore.Persistence;
using Microsoft.EntityFrameworkCore;
using AspNetCore;
using EFCore;

namespace SampleApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // اضافه کردن EFCore Provider و فیلترها
            builder.Services.AddLoggingProviderEfCore(builder.Configuration, "LoggingDb");
            builder.Services.AddLoggingProvider(builder.Configuration, global: true);

            // 👇 حتما اضافه کن تا Controllerها و Swagger فعال بشن
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // SampleApi/Program.cs
            builder.Services.AddControllers(); // ← حتماً

            var app = builder.Build();

            await app.Services.ApplyLoggingProviderMigrationsAsync();

            // تزریق فیلتر از DI
            app.MapPost("/login", async (HttpContext http, LogActionFilter logFilter) =>
            {
                // چون فیلتر یک ActionFilter است، در MinimalAPI مستقیم اجرا نمی‌شود،
                // ولی می‌توانی تست کنی که DI درست کار می‌کند.
                await http.Response.WriteAsJsonAsync(new { token = "ok" });
            })
            .WithMetadata(new LogActionAttribute(serviceId: 1, serviceMethodId: 1) { Summary = "Login" });

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllers();
            await app.RunAsync();
        }
    }
}
