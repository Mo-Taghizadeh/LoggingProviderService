using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LogActionAttribute : Attribute
    {
        public int ServiceId { get; }
        public int ServiceMethodId { get; }
        public string? Summary { get; set; }

        public long? PointerId { get; set; }
        public string? PointerKey { get; set; }
        public Guid? PointerGuid { get; set; }

        public LogActionAttribute(int serviceId, int serviceMethodId)
        { ServiceId = serviceId; ServiceMethodId = serviceMethodId; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class SkipLogActionAttribute : Attribute { }
}
