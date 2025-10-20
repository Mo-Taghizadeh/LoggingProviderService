using Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstractions
{
    public interface ILogWriter
    {
        Task<long> LogRequestAsync(RequestLogDto log, CancellationToken ct);
        Task<long> LogResponseAsync(ResponseLogDto log, CancellationToken ct);
    }
}
