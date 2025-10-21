using LoggingProviderService.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.EFCore.Persistence.Dynamic
{
    public class LogDbContextDynamic : DbContext
    {
        private readonly LoggingSchema _schema;

        public LogDbContextDynamic(DbContextOptions<LogDbContextDynamic> options, IOptions<LoggingSchema> schema)
            : base(options) => _schema = schema.Value;

        public DbSet<RequestDynamic> Requests => Set<RequestDynamic>();
        public DbSet<ResponseDynamic> Responses => Set<ResponseDynamic>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            var schemaName = string.IsNullOrWhiteSpace(_schema.Schema) ? "dbo" : _schema.Schema;

            ConfigureEntity(b, _schema.GetEntity("Request"), typeof(RequestDynamic), "RequestId", schemaName);
            ConfigureEntity(b, _schema.GetEntity("Response"), typeof(ResponseDynamic), "ResponseId", schemaName);

            // FK پویا بین Response -> Request اگر در اسکیما وجود داشته باشد
            var resp = _schema.GetEntity("Response");
            var fkCol = resp.Columns.FirstOrDefault(c => (c.ForeignKey ?? "").StartsWith("Request("))?.Name;
            if (!string.IsNullOrEmpty(fkCol))
            {
                b.Entity<ResponseDynamic>()
                 .HasOne<RequestDynamic>()
                 .WithMany()
                 .HasForeignKey(fkCol!)
                 .OnDelete(DeleteBehavior.SetNull);
            }
        }

        private static void ConfigureEntity(ModelBuilder b, EntityDef def, Type clrType, string keyName, string schema)
        {
            var e = b.Entity(clrType);
            e.ToTable(def.Name, schema);

            // کلید
            e.HasKey(keyName);
            var keyType = MapType(def.Key.Type);
            var keyProp = e.Property(keyType, keyName);
            if (def.Key.Identity && keyType == typeof(long))
                keyProp.ValueGeneratedOnAdd();

            // ستون‌ها
            foreach (var c in def.Columns)
            {
                // اگر پراپرتی CLR با همین نام وجود دارد، همان را پیکربندی کن
                var clrProp = clrType.GetProperty(c.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                var p = clrProp is not null
                    ? e.Property(clrProp.PropertyType, clrProp.Name)  // configure existing CLR property
                    : e.Property(MapType(c.Type), c.Name);            // create/configure shadow property

                // Nullability:
                // اگر CLR nullable است، EF خودش متوجه می‌شود؛ ولی برای Shadow بهتر است IsRequired را ست کنیم.
                if (!c.Nullable && clrProp is null) p.IsRequired();

                if (!string.IsNullOrWhiteSpace(c.ColumnType))
                    p.HasColumnType(c.ColumnType);

                if (c.MaxLength.HasValue)
                    p.HasMaxLength(c.MaxLength.Value);

                if (!string.IsNullOrWhiteSpace(c.DefaultValueSql))
                    p.HasDefaultValueSql(c.DefaultValueSql);

                if (c.Index)
                    e.HasIndex(c.Name);
            }

            // ایندکس‌های ترکیبی
            foreach (var idx in def.CompositeIndexes ?? Enumerable.Empty<string[]>())
                e.HasIndex(idx);
        }

        private static Type MapType(string t) => t.ToLowerInvariant() switch
        {
            "int" => typeof(int),
            "long" => typeof(long),
            "string" => typeof(string),
            "bool" => typeof(bool),
            "guid" => typeof(Guid),
            "datetime2" => typeof(DateTime),
            "datetime" => typeof(DateTime),
            "decimal" => typeof(decimal),
            _ => typeof(string)
        };
    }
}
