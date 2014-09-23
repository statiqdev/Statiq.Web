using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // Modules should be stateless - they can potentially be run more than once
    public interface IModule
    {
        // TODO: This needs to return some kind of enumerable, and possibly allow modules to clone the meta (I.e., for multiple read files)
        void SetMetadata(dynamic meta);

        IEnumerable<string> Execute(dynamic meta, string content);
    }
}
