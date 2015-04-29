using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Extensibility
{
    public interface ITrace
    {
        void SetLevel(SourceLevels level);
        void AddListener(TraceListener listener);
        void RemoveListener(TraceListener listener);
        void Critical(string format, params object[] args);
        void Error(string format, params object[] args);
        void Warning(string format, params object[] args);
        void Information(string format, params object[] args);
        void Verbose(string format, params object[] args);
        void TraceEvent(TraceEventType eventType, string format, params object[] args);
    }
}
