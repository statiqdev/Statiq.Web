using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    public class NamedModule : IModule
    {
        public string Name { get; }
        public IModule Module { get; }

        public NamedModule(string name, IModule module)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }
            Name = name;
            Module = module;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context) => Module.Execute(inputs, context);
    }
}
