using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    public interface IExecutionCache
    {
        bool TryGetValue(IDocument document, out object value);
        void Set(IDocument document, object value);
    }
}
