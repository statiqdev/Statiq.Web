using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    public class DirectoryComparer : IEqualityComparer<IDirectory>
    {
        public bool Equals(IDirectory x, IDirectory y)
        {
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(IDirectory obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}
