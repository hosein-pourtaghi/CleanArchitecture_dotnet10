using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.LoggingCore.Services;
 
public interface ITraceIdAccessor
{
    string TraceId { get; }
    string CorrelationId { get; }
}
