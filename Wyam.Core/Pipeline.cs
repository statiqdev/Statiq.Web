using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class Pipeline
    {
        private readonly List<IModule> _modules = new List<IModule>();

        public Pipeline(params IModule[] modules)
        {
            foreach(IModule module in modules)
            {
                Add(module);
            }
        }

        public void Add(IModule module)
        {
            _modules.Add(module);
        }

        internal void Execute(MetadataStack meta)
        {
            // TODO
        }
    }
}
