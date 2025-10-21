using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.EFCore
{
    public static class SchemaBootstrapper
    {
        /// بار اول دیتابیس و تیبل‌ها را می‌سازد (بدون مایگریشن‌های design-time).
        public static async Task EnsureCreatedAsync<TContext>(this IServiceProvider sp) where TContext : DbContext
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            await db.Database.EnsureCreatedAsync();

            // جدول نسخه اسکیما (اختیاری)
            var raw = db.Database;
            await raw.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.__LogSchemaVersion','U') IS NULL
    CREATE TABLE dbo.__LogSchemaVersion(
      Id INT IDENTITY(1,1) PRIMARY KEY,
      CreatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
    );");
        }
    }
}
