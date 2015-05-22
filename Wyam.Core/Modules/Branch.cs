using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // This executes the specified modules and then outputs the input documents without modification
    // In other words, the branch does affect the primary pipeline module flow
    public class Branch : IModule
    {
        private readonly Func<IDocument, bool> _predicate;
        private readonly IModule[] _modules;

        public Branch(params IModule[] modules)
        {
            _modules = modules;
        }

        public Branch(Func<IDocument, bool> predicate, params IModule[] modules)
        {
            _predicate = predicate;
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            IEnumerable<IDocument> documents = _predicate == null ? inputs : inputs.Where(_predicate);
            pipeline.Execute(_modules, documents);
            return inputs;
        }
    }
}
