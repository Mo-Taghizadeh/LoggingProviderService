# LoggingProviderService.AspNetCore

پکیج **ASP.NET Core Middleware / ActionFilter** برای لاگ‌گیری خودکار Request و Response در کنترلرها.  
به‌طور مستقیم با `ILogWriter` (از Abstractions) کار می‌کند و قابل استفاده در پروژه‌های WebAPI و MVC است.

## 📦 نصب
```bash
dotnet add package LoggingProviderService.AspNetCore
```

## ⚙️ پیکربندی در Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// فعال‌سازی فیلتر لاگ
builder.Services.AddLoggingProvider(builder.Configuration, global: true);

var app = builder.Build();
app.MapControllers();

await app.RunAsync();
```

## ⚙️ تنظیمات appsettings.json
```json
{
  "LoggingProvider": {
    "Enabled": true,
    "MaxBodyBytes": 65536,
    "LogHeaders": true
  }
}
```

- `Enabled`: فعال یا غیرفعال کردن کامل لاگ‌گیری  
- `MaxBodyBytes`: حداکثر بایت بدنه‌ی درخواست/پاسخ که در لاگ ذخیره می‌شود  
- `LogHeaders`: ثبت هدرها  

## 🧠 استفاده در کنترلر
### حالت ۱: سراسری (Global)
```csharp
builder.Services.AddLoggingProvider(builder.Configuration, global: true);
```

### حالت ۲: فقط برای اکشن‌های خاص
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [LogAction(serviceId: 1, serviceMethodId: 101, Summary = "Login")]
    public IActionResult Login([FromBody] object input) => Ok(new { token = "ok" });

    [HttpGet("health")]
    [SkipLogAction]
    public IActionResult Health() => Ok("Healthy");
}
```

## 📋 نکات فنی
| ویژگی | توضیح |
|--------|--------|
| سیستم لاگ | ActionFilter مبتنی بر Dependency Injection |
| حداکثر طول Body | از طریق LogOptions قابل تنظیم |
| پشتیبانی از Headers | بله |
| Correlation Keys | PointerId, PointerKey, PointerGuid |
| Minimal APIs | پشتیبانی مستقیم ندارد (فقط روی Controllerها) |

## 🧩 سازگاری
| Framework | وضعیت |
|------------|--------|
| .NET 6 | ✅ |
| .NET 7 | ✅ |
| .NET 8 | ✅ |

## 📄 لایسنس
MIT