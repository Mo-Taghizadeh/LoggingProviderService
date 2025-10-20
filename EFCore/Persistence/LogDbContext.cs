using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Persistence
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

        public DbSet<RequestLog> Requests => Set<RequestLog>();
        public DbSet<ResponseLog> Responses => Set<ResponseLog>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<RequestLog>(e =>
            {
                e.ToTable("Request", "dbo");
                e.HasKey(x => x.RequestId);
                e.Property(x => x.RequestId).ValueGeneratedOnAdd();
                e.Property(x => x.Exception).HasMaxLength(4000);
                e.Property(x => x.SummaryData).HasMaxLength(500);
                e.Property(x => x.UserId).HasMaxLength(100);
                e.Property(x => x.PointerKey).HasMaxLength(100);
                e.Property(x => x.CallTime).HasColumnType("datetime2(3)");
                e.Property(x => x.InsertTime).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");
                e.HasIndex(x => x.InsertTime);
                e.HasIndex(x => new { x.ServiceId, x.ServiceMethodId, x.InsertTime });
                e.HasIndex(x => x.PointerId);
                e.HasIndex(x => x.PointerKey);
                e.HasIndex(x => x.PointerGuid);
            });

            b.Entity<ResponseLog>(e =>
            {
                e.ToTable("Response", "dbo");
                e.HasKey(x => x.ResponseId);
                e.Property(x => x.ResponseId).ValueGeneratedOnAdd();
                e.Property(x => x.Exception).HasMaxLength(4000);
                e.Property(x => x.SummaryData).HasMaxLength(500);
                e.Property(x => x.UserId).HasMaxLength(100);
                e.Property(x => x.PointerKey).HasMaxLength(100);
                e.Property(x => x.CallTime).HasColumnType("datetime2(3)");
                e.Property(x => x.ResponseTime).HasColumnType("datetime2(3)").IsRequired();
                e.Property(x => x.InsertTime).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");
                e.HasOne(x => x.Request).WithMany(r => r.Responses).HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.SetNull);
                e.HasIndex(x => x.RequestId);
                e.HasIndex(x => x.InsertTime);
                e.HasIndex(x => new { x.ServiceId, x.ServiceMethodId, x.InsertTime });
                e.HasIndex(x => x.PointerId);
                e.HasIndex(x => x.PointerKey);
                e.HasIndex(x => x.PointerGuid);
            });
        }
    }
}
