using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Modules;

namespace Wyam.Modules.Razor
{
    public class Razor : IModule
    {
        private readonly ReadFile _readFile;

        public Razor()
        {
        }

        // Use this constructor to read file(s) for input
        // It has the effect of inserting a ReadFile module into the pipeline just before this module
        public Razor(Func<dynamic, string> fileFunc)
        {
            _readFile = new ReadFile(fileFunc);
        }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public string Execute(PipelineContext context, string content)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}
