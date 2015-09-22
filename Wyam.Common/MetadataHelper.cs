using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public static class MetadataHelper
    {
        public static KeyValuePair<string, object> New(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        } 
    }
}
