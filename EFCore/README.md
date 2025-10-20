# LoggingProviderService.AspNetCore

Attribute-based request & response logging for ASP.NET Core applications.

## 🚀 Features
- `[LogAction]` attribute to automatically log request/response data
- `[SkipLogAction]` to exclude actions from logging
- Configurable max body size and header logging
- Works with any logging provider implementing `ILogWriter`
- Fully async, safe, and lightweight

## ⚙️ Usage
In your `Program.cs`:
```csharp
using LoggingProviderService.AspNetCore;
using LoggingProviderService.EFCore;

builder.Services.AddLoggingProvider(builder.Configuration, global: false);
builder.Services.AddLoggingProviderEfCore(builder.Configuration, "LoggingDb");

var app = builder.Build();
await app.Services.ApplyLoggingProviderMigrationsAsync();
app.MapControllers();
app.Run();

On your controller:
[ServiceFilter(typeof(LogActionFilter))]
[LogAction(serviceId: 10, serviceMethodId: 102, Summary = "User login")]
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest req) => Ok();


📖 Configuration

Add this section in your appsettings.json:
"LoggingProvider": {
  "Enabled": true,
  "MaxBodyBytes": 65536,
  "LogHeaders": true
}

📦 About

LoggingProviderService.AspNetCore is part of the Logging Provider Service ecosystem:

LoggingProviderService.Abstractions → Contracts (ILogWriter, DTOs)

LoggingProviderService.EFCore → EF Core provider

LoggingProviderService.AspNetCore → ASP.NET integration layer