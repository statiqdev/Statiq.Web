using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // Outputs the supplied content to the trace without manipulating the inputs
    // This is useful for debugging or reporting custom status during build
    public class Trace : ContentModule
    {
        private readonly TraceEventType _traceEventType;

        public Trace(object content, TraceEventType traceEventType = TraceEventType.Information)
            : base(content)
        {
            _traceEventType = traceEventType;
        }

        public Trace(Func<IDocument, object> content, TraceEventType traceEventType = TraceEventType.Information)
            : base(content)
        {
            _traceEventType = traceEventType;
        }

        public Trace(params IModule[] modules)
            : this(TraceEventType.Information, modules)
        {
        }

        public Trace(TraceEventType traceEventType, params IModule[] modules)
            : base(modules)
        {
            _traceEventType = traceEventType;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IPipelineContext pipeline)
        {
            pipeline.Trace.TraceEvent(_traceEventType, content.ToString());
            return new [] { input };
        }
    }
}
