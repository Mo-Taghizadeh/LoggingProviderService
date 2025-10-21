using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.Abstractions.Models
{
    public sealed class LoggingSchema
    {
        public string? Schema { get; set; } = "dbo";
        public List<EntityDef> Entities { get; set; } = new();

        public EntityDef GetEntity(string name)
            => Entities.First(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public sealed class EntityDef
    {
        public string Name { get; set; } = default!;
        public KeyDef Key { get; set; } = new();
        public List<ColumnDef> Columns { get; set; } = new();
        public List<string[]>? CompositeIndexes { get; set; }
    }

    public sealed class KeyDef
    {
        public string Name { get; set; } = "Id";
        public string Type { get; set; } = "long";    // long|int|guid|…
        public bool Identity { get; set; } = true;    // برای SQL Server
    }

    public sealed class ColumnDef
    {
        public string Name { get; set; } = default!;
        public string Type { get; set; } = "string";  // string|int|long|bool|guid|datetime2|decimal
        public bool Nullable { get; set; } = true;
        public int? MaxLength { get; set; }
        public string? ColumnType { get; set; }       // مثلاً "nvarchar(max)" یا "datetime2(3)"
        public string? DefaultValueSql { get; set; }  // مثلاً "SYSUTCDATETIME()"
        public bool Index { get; set; } = false;
        public string? ForeignKey { get; set; }       // مثلا: "Request(RequestId)"
    }

}
