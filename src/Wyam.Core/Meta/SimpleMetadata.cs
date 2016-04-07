using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Meta;

namespace Wyam.Core.Meta
{
    public class SimpleMetadata : Dictionary<string, object>, ISimpleMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMetadata"/> class.
        /// </summary>
        public SimpleMetadata()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMetadata"/> class with objects
        /// copied from another dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to copy from.</param>
        public SimpleMetadata(IDictionary<string, object> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Creates clone of this instance.
        /// </summary>
        public ISimpleMetadata Clone()
        {
            return new SimpleMetadata(this);
        }
    }
}