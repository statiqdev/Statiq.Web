using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Helpers;

namespace Wyam.Core.Modules
{
    public class WriteFiles : IModule
    {
        private readonly Func<dynamic, string> _path;

        public WriteFiles(Func<dynamic, string> path)
        {
            if (path == null) throw new ArgumentNullException("path");

            _path = path;
        }

        public IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public string Execute(IPipelineContext context, string content)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}
