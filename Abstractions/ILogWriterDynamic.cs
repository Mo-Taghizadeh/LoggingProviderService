using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingProviderService.Abstractions
{
    public interface ILogWriterDynamic
    {
        Task<long> InsertAsync(string entityName, IDictionary<string, object?> values, CancellationToken ct);
    }
}
