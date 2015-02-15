using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    public class WriteFile : IModule
    {
        private readonly Func<dynamic, string> _path;

        public WriteFile(Func<dynamic, string> path)
        {
            _path = path;
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
