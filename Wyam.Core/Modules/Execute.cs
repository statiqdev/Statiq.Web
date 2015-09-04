using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Core;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    // Executes the specified delegate for each input
    public class Execute : IModule
    {
        private readonly DocumentConfig _execute;

        // The delegate should return a IEnumerable<IDocument>
        public Execute(DocumentConfig execute)
        {
            _execute = execute;
        }

        IEnumerable<IDocument> IModule.Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.SelectMany(x => _execute.Invoke<IEnumerable<IDocument>>(x, context) ?? Array.Empty<IDocument>());
        }
    }
}