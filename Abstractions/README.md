# LoggingProviderService.Abstractions

این پکیج شامل **مدل‌ها (DTOs)** و **اینترفیس‌های قراردادی** برای لاگ‌گیری در سیستم EduFlux است.  
هدفش جداسازی کامل قراردادهای لاگ از پیاده‌سازی (EFCore یا ASP.NET Core) است تا سرویس‌های دیگر بدون وابستگی مستقیم بتوانند با سیستم لاگ تعامل داشته باشند.

## 📦 نصب
```bash
dotnet add package LoggingProviderService.Abstractions
```

## 📘 محتوای پکیج
- `ILogWriter` → رابط اصلی برای ثبت لاگ‌ها  
- `RequestLogDto` → مدل داده‌ی درخواست  
- `ResponseLogDto` → مدل داده‌ی پاسخ  

## 🚀 نمونه استفاده
```csharp
using Abstractions;
using Abstractions.Models;

public class MyService
{
    private readonly ILogWriter _writer;
    public MyService(ILogWriter writer) => _writer = writer;

    public async Task DoAsync()
    {
        var reqId = await _writer.LogRequestAsync(new RequestLogDto(
            ServiceId: 10,
            ServiceMethodId: 1001,
            MethodInput: "{\"UserId\":42}",
            Exception: null,
            CallTime: DateTime.UtcNow,
            SummaryData: "UI:GetUser",
            PointerId: null,
            PointerKey: null,
            PointerGuid: Guid.NewGuid(),
            UserId: "u-123"
        ), CancellationToken.None);
    }
}
```

## ⚙️ مشخصات فنی
| ویژگی | توضیح |
|--------|--------|
| Namespace اصلی | `Abstractions` |
| Target Framework | .NET 8.0 |
| وابستگی‌ها | ندارد |
| هدف | اشتراک قرارداد بین پروژه‌ها بدون نیاز به EFCore |

## 📄 لایسنس
MIT