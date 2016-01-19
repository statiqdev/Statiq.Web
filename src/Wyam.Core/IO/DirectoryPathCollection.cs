using System.Collections;
using System.Collections.Generic;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    public class DirectoryPathCollection : IDirectoryPathCollection
    {
        private readonly List<DirectoryPath> _paths = new List<DirectoryPath>();

        public IEnumerator<DirectoryPath> GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _paths.Count;

        public DirectoryPath this[int index] => _paths[index];

        public bool Add(DirectoryPath path)
        {
            if (_paths.Contains(path))
            {
                return false;
            }
            _paths.Add(path);
            return true;
        }

        public void Clear()
        {
            _paths.Clear();
        }

        public bool Contains(DirectoryPath path)
        {
            return _paths.Contains(path);
        }

        public bool Remove(DirectoryPath path)
        {
            return _paths.Remove(path);
        }

        public int IndexOf(DirectoryPath path)
        {
            return _paths.IndexOf(path);
        }

        public bool Insert(int index, DirectoryPath path)
        {
            if (_paths.Contains(path))
            {
                return false;
            }
            _paths.Insert(index, path);
            return true;
        }

        public void RemoveAt(int index)
        {
            _paths.RemoveAt(index);
        }
    }
}
