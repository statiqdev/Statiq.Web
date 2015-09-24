using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    // Implement this interface to provide lazy metadata values or values based on other metadata
    // The Get(...) method will be called for each request of this value and then processed like any other value
    public interface IMetadataValue
    {
        // The implementation of this method must be thread-safe
        object Get(string key, IMetadata metadata);
    }
}
