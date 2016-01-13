using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules.Extensibility
{
    /// <summary>
    /// Executes custom code for each input document.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// </remarks>
    /// <category>Extensibility</category>
    public class Execute : IModule
    {
        private readonly DocumentConfig _executeDocuments;
        private readonly ContextConfig _executeContext;

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a IEnumerable%lt;IDocument&gt;.</param>
        public Execute(DocumentConfig execute)
        {
            _executeDocuments = execute;
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a IEnumerable%lt;IDocument&gt;.</param>
        public Execute(ContextConfig execute)
        {
            _executeContext = execute;
        }

        IEnumerable<IDocument> IModule.Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_executeDocuments != null)
                return inputs.SelectMany(x => _executeDocuments.Invoke<IEnumerable<IDocument>>(x, context) ?? Array.Empty<IDocument>());
            else
                return _executeContext.Invoke<IEnumerable<IDocument>>(context) ?? Array.Empty<IDocument>();
        }
    }
}