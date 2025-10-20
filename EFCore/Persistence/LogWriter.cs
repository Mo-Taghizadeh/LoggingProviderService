using Abstractions;
using Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Persistence
{
    public class LogWriter : ILogWriter
    {
        private readonly LogDbContext _db;
        public LogWriter(LogDbContext db) => _db = db;

        public async Task<long> LogRequestAsync(RequestLogDto dto, CancellationToken ct)
        {
            var e = new RequestLog
            {
                ServiceId = dto.ServiceId,
                ServiceMethodId = dto.ServiceMethodId,
                MethodInput = dto.MethodInput,
                Exception = dto.Exception,
                CallTime = dto.CallTime,
                InsertTime = DateTime.UtcNow,
                SummaryData = dto.SummaryData,
                PointerId = dto.PointerId,
                PointerKey = dto.PointerKey,
                PointerGuid = dto.PointerGuid ?? Guid.NewGuid(),
                UserId = dto.UserId
            };
            _db.Requests.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.RequestId;
        }

        public async Task<long> LogResponseAsync(ResponseLogDto dto, CancellationToken ct)
        {
            var e = new ResponseLog
            {
                ServiceId = dto.ServiceId,
                ServiceMethodId = dto.ServiceMethodId,
                MethodInput = dto.MethodInput,
                MethodOutput = dto.MethodOutput,
                Exception = dto.Exception,
                CallTime = dto.CallTime,
                ResponseTime = dto.ResponseTime,
                InsertTime = DateTime.UtcNow,
                SummaryData = dto.SummaryData,
                PointerId = dto.PointerId,
                PointerKey = dto.PointerKey,
                PointerGuid = dto.PointerGuid ?? Guid.NewGuid(),
                UserId = dto.UserId,
                RequestId = dto.RequestId
            };
            _db.Responses.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.ResponseId;
        }
    }
}
