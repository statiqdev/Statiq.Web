using System.Collections.Generic;

namespace Wyam.Common.IO
{
    public class FileEqualityComparer : IEqualityComparer<IFile>
    {
        public bool Equals(IFile x, IFile y)
        {
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(IFile obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}