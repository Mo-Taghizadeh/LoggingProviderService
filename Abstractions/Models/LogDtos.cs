using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstractions.Models
{
    public record RequestLogDto(
    int ServiceId,
    int ServiceMethodId,
    string? MethodInput,
    string? Exception,
    DateTime CallTime,
    string? SummaryData,
    long? PointerId,
    string? PointerKey,
    Guid? PointerGuid,
    string? UserId);

    public record ResponseLogDto(
        int ServiceId,
        int ServiceMethodId,
        string? MethodInput,
        string? MethodOutput,
        string? Exception,
        DateTime CallTime,
        DateTime ResponseTime,
        string? SummaryData,
        long? PointerId,
        string? PointerKey,
        Guid? PointerGuid,
        string? UserId,
        long? RequestId);
}
