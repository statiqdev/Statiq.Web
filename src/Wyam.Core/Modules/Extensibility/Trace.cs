using System.Collections.Generic;
using System.Diagnostics;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules.Extensibility
{
    /// <summary>
    /// Outputs trace messages during execution.
    /// </summary>
    /// <remarks>
    /// This module has no effect on documents and the input documents are passed through to output documents.
    /// </remarks>
    /// <category>Extensibility</category>
    public class Trace : ContentModule
    {
        private TraceEventType _traceEventType = TraceEventType.Information;

        /// <summary>
        /// Outputs the string value of the specified object to trace.
        /// </summary>
        /// <param name="content">The content to trace.</param>
        public Trace(object content)
            : base(content)
        {
        }

        /// <summary>
        /// Outputs the string value of the returned object to trace. This allows 
        /// you to trace different content depending on the execution context.
        /// </summary>
        /// <param name="content">A delegate that returns the content to trace.</param>
        public Trace(ContextConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// Outputs the string value of the returned object to trace. This allows 
        /// you to trace different content for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to trace.</param>
        public Trace(DocumentConfig content) 
            : base(content)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the 
        /// resulting document content is output to trace.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Trace(params IModule[] modules)
            : base(modules)
        {
        }

        public Trace EventType(TraceEventType traceEventType)
        {
            _traceEventType = traceEventType;
            return this;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            Wyam.Common.Tracing.Trace.TraceEvent(_traceEventType, content.ToString());
            return new [] { input };
        }
    }
}
