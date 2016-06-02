using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Configuration
{
    public interface IRawAssemblyCollection : IEnumerable<byte[]>
    {
        void Add(byte[] rawAssembly);
    }
}
