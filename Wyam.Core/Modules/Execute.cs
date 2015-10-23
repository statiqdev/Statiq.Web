using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Core;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    // Executes the specified delegate for each input
    public class Execute : IModule
    {
        private readonly DocumentConfig _executeDocuments;
        private readonly ContextConfig _executeContext;

        // The delegate should return a IEnumerable<IDocument>
        public Execute(DocumentConfig execute)
        {
            _executeDocuments = execute;
        }

        // The delegate should return a IEnumerable<IDocument>
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