using LoggingProviderService.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoggingProviderService.EFCore.Persistence.Dynamic
{
    public class DynamicLogWriter : ILogWriterDynamic
    {
        private readonly LogDbContextDynamic _db;
        public DynamicLogWriter(LogDbContextDynamic db) => _db = db;

        public async Task<long> InsertAsync(string entityName, IDictionary<string, object?> values, CancellationToken ct)
        {
            if (entityName.Equals("Request", StringComparison.OrdinalIgnoreCase))
            {
                var e = new RequestDynamic();
                await FillAndSaveAsync(e, values, ct);
                return e.RequestId;
            }
            if (entityName.Equals("Response", StringComparison.OrdinalIgnoreCase))
            {
                var e = new ResponseDynamic();
                await FillAndSaveAsync(e, values, ct);
                return e.ResponseId;
            }

            throw new ArgumentException($"Unknown entity: {entityName}");
        }

        private async Task FillAndSaveAsync(object entity, IDictionary<string, object?> values, CancellationToken ct)
        {
            var entry = _db.Entry(entity);
            var et = entry.Metadata; // IEntityType

            foreach (var (key, raw) in values)
            {
                // فقط ستون‌هایی که واقعاً در مدل وجود دارند را ست کن
                var prop = et.FindProperty(key);
                if (prop is null) continue; // ← کلیدهای ناشناخته (مثل additionalProp1) را نادیده بگیر

                var coerced = Coerce(raw, prop.ClrType);
                entry.Property(prop.Name).CurrentValue = coerced;
            }

            _db.Add(entity);
            await _db.SaveChangesAsync(ct);
        }

        // تبدیل امن به نوع ستون (handle JsonElement هم)
        private static object? Coerce(object? value, Type targetType)
        {
            if (value is null) return null;

            // وقتی بدنه به Dictionary بایند می‌شود، مقدارها اغلب JsonElement هستند
            if (value is JsonElement je)
            {
                if (targetType == typeof(string)) return je.ValueKind == JsonValueKind.String ? je.GetString() : je.ToString();
                if (targetType == typeof(int)) return je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var i) ? i : int.Parse(je.ToString());
                if (targetType == typeof(long)) return je.ValueKind == JsonValueKind.Number && je.TryGetInt64(out var l) ? l : long.Parse(je.ToString());
                if (targetType == typeof(bool)) return je.ValueKind == JsonValueKind.True || je.ValueKind == JsonValueKind.False ? je.GetBoolean() : bool.Parse(je.ToString());
                if (targetType == typeof(Guid)) return Guid.Parse(je.GetString() ?? je.ToString());
                if (targetType == typeof(DateTime)) return DateTime.Parse(je.GetString() ?? je.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                if (targetType == typeof(decimal)) return decimal.Parse(je.ToString());
                // سایر انواع
                return je.ToString();
            }

            // اگر نوع همین است
            if (targetType.IsInstanceOfType(value)) return value;

            // تبدیل‌های رایج از string
            if (value is string s)
            {
                if (targetType == typeof(Guid)) return Guid.Parse(s);
                if (targetType == typeof(DateTime)) return DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind);
                if (targetType == typeof(int)) return int.Parse(s);
                if (targetType == typeof(long)) return long.Parse(s);
                if (targetType == typeof(bool)) return bool.Parse(s);
                if (targetType == typeof(decimal)) return decimal.Parse(s);
            }

            // تلاش عمومی
            try { return Convert.ChangeType(value, targetType); }
            catch { return value; }
        }
    }
}
