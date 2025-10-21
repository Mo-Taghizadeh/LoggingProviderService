using LoggingProviderService.Abstractions;
using LoggingProviderService.Abstractions.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.AspNetCore
{
    public class DynamicRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogWriterDynamic _writer;
        private readonly LoggingSchema _schema;
        private readonly int _maxBodyBytes;
        private readonly HashSet<string> _skipPaths;

        public DynamicRequestLoggingMiddleware(
            RequestDelegate next,
            ILogWriterDynamic writer,
            IOptions<LoggingSchema> schema,
            IConfiguration cfg)
        {
            _next = next;
            _writer = writer;
            _schema = schema.Value;

            _maxBodyBytes = cfg.GetSection("LoggingProvider").GetValue("MaxBodyBytes", 64 * 1024);
            _skipPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/swagger", "/swagger/index.html", "/swagger/v1/swagger.json",
            "/favicon.ico", "/health", "/healthz"
        };
        }

        public async Task Invoke(HttpContext ctx)
        {
            if (ShouldSkip(ctx.Request.Path)) { await _next(ctx); return; }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            string? reqBody = await ReadBodyAsync(ctx.Request, _maxBodyBytes);

            // Capture response
            var originalBody = ctx.Response.Body;
            await using var mem = new MemoryStream();
            ctx.Response.Body = mem;

            Exception? ex = null;
            try { await _next(ctx); }
            catch (Exception e) { ex = e; throw; }
            finally
            {
                sw.Stop();
                mem.Position = 0;
                string? respBody = await ReadStreamAsync(mem, _maxBodyBytes);
                mem.Position = 0;
                await mem.CopyToAsync(originalBody);
                ctx.Response.Body = originalBody;

                var now = DateTime.UtcNow;
                var call = now.AddMilliseconds(-sw.ElapsedMilliseconds);

                // فقط فیلدهای موجود در اسکیما
                var reqDict = ProjectToSchema("Request", new Dictionary<string, object?>
                {
                    ["ServiceId"] = 0, // اگر خواستی از RouteData پر کن
                    ["ServiceMethodId"] = 0,
                    ["MethodInput"] = reqBody,
                    ["Exception"] = null,
                    ["CallTime"] = call,
                    ["InsertTime"] = now,
                    ["SummaryData"] = $"{ctx.Request.Method} {ctx.Request.Path}",
                    ["PointerGuid"] = (Guid?)null,
                    ["UserId"] = ctx.User?.Identity?.Name
                });

                var reqId = await _writer.InsertAsync("Request", reqDict, ctx.RequestAborted);

                var respDict = ProjectToSchema("Response", new Dictionary<string, object?>
                {
                    ["ServiceId"] = 0,
                    ["ServiceMethodId"] = 0,
                    ["MethodInput"] = reqBody,
                    ["MethodOutput"] = respBody,
                    ["Exception"] = ex?.ToString(),
                    ["CallTime"] = call,
                    ["ResponseTime"] = now,
                    ["InsertTime"] = now,
                    ["SummaryData"] = ctx.Response.StatusCode.ToString(),
                    ["PointerGuid"] = (Guid?)null,
                    ["UserId"] = ctx.User?.Identity?.Name,
                    ["RequestId"] = reqId
                });

                await _writer.InsertAsync("Response", respDict, ctx.RequestAborted);
            }
        }

        private bool ShouldSkip(PathString path)
            => _skipPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

        private static async Task<string?> ReadBodyAsync(HttpRequest req, int cap)
        {
            if (req.ContentLength is null or 0) return null;
            req.EnableBuffering();
            using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
            var buf = await reader.ReadToEndAsync();
            req.Body.Position = 0;
            return Truncate(buf, cap);
        }

        private static async Task<string?> ReadStreamAsync(Stream s, int cap)
        {
            using var r = new StreamReader(s, Encoding.UTF8, leaveOpen: true);
            var text = await r.ReadToEndAsync();
            return Truncate(text, cap);
        }

        private static string Truncate(string text, int cap)
            => text.Length <= cap ? text : text[..cap];

        private Dictionary<string, object?> ProjectToSchema(string entity, IDictionary<string, object?> raw)
        {
            var e = _schema.GetEntity(entity);
            var allowed = new HashSet<string>(e.Columns.Select(c => c.Name), StringComparer.OrdinalIgnoreCase)
                      { e.Key.Name };
            var d = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in raw)
                if (allowed.Contains(kv.Key)) d[kv.Key] = kv.Value;
            return d;
        }
    }

    public static class DynamicRequestLoggingExtensions
    {
        public static IApplicationBuilder UseDynamicRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<DynamicRequestLoggingMiddleware>();
    }
}
