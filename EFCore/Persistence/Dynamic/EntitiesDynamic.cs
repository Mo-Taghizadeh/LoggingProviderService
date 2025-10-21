using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.EFCore.Persistence.Dynamic
{
    // موجودیت‌های مینیمال (فقط کلید) — بقیه ستون‌ها Shadow هستند
    public class RequestDynamic { public long RequestId { get; set; } }
    public class ResponseDynamic { public long ResponseId { get; set; } public long? RequestId { get; set; } }
}
