using System.Text;
using System.Text.Json;
using Abstractions;
using Abstractions.Models;
using AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;   // برای RouteData/RouteValueDictionary

namespace AspNetCore.Filters
{
    public sealed class LogActionFilter : IAsyncActionFilter
    {
        private readonly ILogWriter _writer;
        private readonly LogOptions _opt;
        public LogActionFilter(ILogWriter writer, IOptions<LogOptions> opt)
        { _writer = writer; _opt = opt.Value; }

        public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            if (!_opt.Enabled) { await next(); return; }

            var cad = ctx.ActionDescriptor as ControllerActionDescriptor;

            if (cad?.MethodInfo.GetCustomAttributes(typeof(SkipLogActionAttribute), true).Any() == true ||
                cad?.ControllerTypeInfo.GetCustomAttributes(typeof(SkipLogActionAttribute), true).Any() == true)
            { await next(); return; }

            var attr = cad?.MethodInfo.GetCustomAttributes(typeof(LogActionAttribute), true)
                          .Cast<LogActionAttribute>().FirstOrDefault()
                      ?? cad?.ControllerTypeInfo.GetCustomAttributes(typeof(LogActionAttribute), true)
                          .Cast<LogActionAttribute>().FirstOrDefault();

            if (attr is null) { await next(); return; }

            var http = ctx.HttpContext;
            var req = http.Request;
            var userId = http.User?.Identity?.IsAuthenticated == true
                ? http.User.Identity!.Name
                : null;

            string? bodyText = await ReadBodyPreviewAsync(req, _opt.MaxBodyBytes);
            string? reqHeaders = _opt.LogHeaders ? SerializeHeaders(req.Headers) : null;

            // جلوگیری از null در RouteValues
            var routeDict = ctx.RouteData?.Values?
    .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "")
    ?? new Dictionary<string, string>();

            var inputJson = JsonSerializer.Serialize(new
            {
                path = req.Path.ToString(),
                method = req.Method,
                query = req.Query.ToDictionary(k => k.Key, v => v.Value.ToString()),
                route = routeDict,
                body = bodyText,
                headers = reqHeaders
            });

            var callTime = DateTime.UtcNow;
            long requestId = 0;

            try
            {
                requestId = await _writer.LogRequestAsync(new RequestLogDto(
                    attr.ServiceId, attr.ServiceMethodId,
                    inputJson, null, callTime, attr.Summary,
                    attr.PointerId, attr.PointerKey, attr.PointerGuid ?? Guid.NewGuid(), userId
                ), http.RequestAborted);
            }
            catch { /* swallow logging errors */ }

            var original = http.Response.Body;
            using var mem = new MemoryStream();
            http.Response.Body = mem;

            string? respBody = null; string? respHeaders = null; string? exception = null;

            try
            {
                var executed = await next();
                if (executed.Exception is { } ex && !executed.ExceptionHandled)
                    exception = Flatten(ex);
            }
            finally
            {
                try
                {
                    mem.Position = 0;
                    using var r = new StreamReader(mem, Encoding.UTF8, leaveOpen: true);
                    var buf = new char[_opt.MaxBodyBytes];
                    var read = await r.ReadBlockAsync(buf, 0, buf.Length);
                    respBody = new string(buf, 0, read);
                    if (r.Peek() >= 0) respBody += "...(truncated)";
                    mem.Position = 0;

                    if (_opt.LogHeaders) respHeaders = SerializeHeaders(http.Response.Headers);
                    await mem.CopyToAsync(original, http.RequestAborted);
                }
                catch { }
                finally { http.Response.Body = original; }

                try
                {
                    var outputJson = JsonSerializer.Serialize(new
                    {
                        statusCode = http.Response.StatusCode,
                        body = respBody,
                        headers = respHeaders
                    });

                    await _writer.LogResponseAsync(new ResponseLogDto(
                        attr.ServiceId, attr.ServiceMethodId,
                        null, outputJson, exception,
                        callTime, DateTime.UtcNow, attr.Summary,
                        attr.PointerId, attr.PointerKey, attr.PointerGuid, userId,
                        requestId == 0 ? (long?)null : requestId
                    ), http.RequestAborted);
                }
                catch { }
            }
        }

        private static async Task<string?> ReadBodyPreviewAsync(HttpRequest req, int maxBytes)
        {
            try
            {
                if (!req.Body.CanSeek) req.EnableBuffering();
                req.Body.Position = 0;
                using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                var buf = new char[maxBytes];
                var read = await reader.ReadBlockAsync(buf, 0, buf.Length);
                var text = new string(buf, 0, read);
                if (reader.Peek() >= 0) text += "...(truncated)";
                req.Body.Position = 0;
                return text;
            }
            catch { return null; }
        }

        private static string SerializeHeaders(IHeaderDictionary headers)
            => JsonSerializer.Serialize(headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

        private static string Flatten(Exception ex)
        {
            var sb = new StringBuilder();
            for (var e = ex; e != null; e = e.InnerException)
                sb.AppendLine($"{e.GetType().Name}: {e.Message}")
                  .AppendLine(e.StackTrace);
            return sb.ToString();
        }
    }
}
