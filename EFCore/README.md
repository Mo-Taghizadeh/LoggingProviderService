# LoggingProviderService.EFCore

پکیج EF Core برای ذخیره‌سازی لاگ‌ها در دیتابیس SQL Server.  
این پکیج شامل DbContext، Entityها، Fluent Configurations و پیاده‌سازی ILogWriter است.

## 📦 نصب
```bash
dotnet add package LoggingProviderService.EFCore
```

## ⚙️ پیکربندی در Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ثبت DbContext و اتصال EFCore Provider
builder.Services.AddLoggingProviderEfCore(builder.Configuration, "LoggingDb");

// ثبت ActionFilter لاگ‌گیری (اختیاری)
builder.Services.AddLoggingProvider(builder.Configuration, global: true);

var app = builder.Build();

// ساخت خودکار جداول (Migration یا EnsureCreated)
await app.Services.ApplyLoggingProviderMigrationsAsync();

app.MapControllers();
await app.RunAsync();
```

## ⚙️ تنظیمات appsettings.json
```json
{
  "ConnectionStrings": {
    "LoggingDb": "Server=.;Database=LogDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

## 🧱 اسکیمای دیتابیس
دو جدول اصلی:
- `Request`
- `Response`

هر کدام شامل فیلدهایی مانند:
- ServiceId
- ServiceMethodId
- MethodInput / MethodOutput
- Exception
- CallTime / ResponseTime
- PointerGuid
- UserId  
- SummaryData

## 🧮 ایندکس‌های کلیدی
| جدول | ایندکس‌ها |
|-------|------------|
| Request | (ServiceId, ServiceMethodId, InsertTime) |
| Response | (ServiceId, ServiceMethodId, InsertTime), (RequestId) |

## 🔍 گزارش‌گیری نمونه
```sql
SELECT ServiceId, ServiceMethodId,
       COUNT(*) AS TotalCalls,
       AVG(DATEDIFF(ms, CallTime, ResponseTime)) AS AvgMs,
       SUM(CASE WHEN DATEDIFF(ms, CallTime, ResponseTime) > 5000 THEN 1 ELSE 0 END) AS SlowOver5s
FROM dbo.Response
GROUP BY ServiceId, ServiceMethodId
ORDER BY AvgMs DESC;
```

## ⚙️ اسکیمای سفارشی (اختیاری)
```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    b.HasDefaultSchema("UILog");
    b.Entity<RequestLog>().ToTable("Request");
    b.Entity<ResponseLog>().ToTable("Response");
}
```

## 📄 لایسنس
MIT