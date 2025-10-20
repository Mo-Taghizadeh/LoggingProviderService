using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Persistence
{
    public class RequestLog
    {
        public long RequestId { get; set; }
        public int ServiceId { get; set; }
        public int ServiceMethodId { get; set; }
        public string? MethodInput { get; set; }
        public string? Exception { get; set; }
        public DateTime CallTime { get; set; }
        public DateTime InsertTime { get; set; }
        public string? SummaryData { get; set; }
        public long? PointerId { get; set; }
        public string? PointerKey { get; set; }
        public Guid? PointerGuid { get; set; }
        public string? UserId { get; set; }
        public ICollection<ResponseLog> Responses { get; set; } = new List<ResponseLog>();
    }

    public class ResponseLog
    {
        public long ResponseId { get; set; }
        public int ServiceId { get; set; }
        public int ServiceMethodId { get; set; }
        public string? MethodInput { get; set; }
        public string? MethodOutput { get; set; }
        public string? Exception { get; set; }
        public DateTime CallTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public DateTime InsertTime { get; set; }
        public string? SummaryData { get; set; }
        public long? PointerId { get; set; }
        public string? PointerKey { get; set; }
        public Guid? PointerGuid { get; set; }
        public string? UserId { get; set; }
        public long? RequestId { get; set; }
        public RequestLog? Request { get; set; }
    }
}
