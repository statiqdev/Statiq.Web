using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Core;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    // Executes the specified delegate for each input
    public class Delegate : IModule
    {
        private readonly Func<IDocument, IEnumerable<IDocument>> _execute;

        public Delegate(Func<IDocument, IEnumerable<IDocument>> execute)
        {
            _execute = execute;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.SelectMany(x => _execute(x));
        }
    }
}