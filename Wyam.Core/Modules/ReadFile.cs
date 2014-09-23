using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    public class ReadFile : IModule
    {
        private readonly Func<dynamic, string> _fileFunc;

        public ReadFile(Func<dynamic, string> fileFunc)
        {
            _fileFunc = fileFunc;
        }

        public void SetMetadata(dynamic meta)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IEnumerable<string> Execute(dynamic meta, string content)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}
