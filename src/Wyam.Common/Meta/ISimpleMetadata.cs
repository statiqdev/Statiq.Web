using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Meta
{
    public interface ISimpleMetadata : IDictionary<string, object>
    {
        /// <summary>
        /// Creates clone of this instance.
        /// </summary>
        ISimpleMetadata Clone();
    }
}