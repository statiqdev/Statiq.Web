using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Core;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // Executes the specified delegate for each input
    public class Execute : IModule
    {
        private readonly Func<IDocument, IEnumerable<IDocument>> _execute;

        public Execute(Func<IDocument, IEnumerable<IDocument>> execute)
        {
            _execute = execute;
        }

        IEnumerable<IDocument> IModule.Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.SelectMany(x => _execute(x));
        }
    }
}