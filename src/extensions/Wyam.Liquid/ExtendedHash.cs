using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace Wyam.Liquid
{
    /// <summary>
    /// This is an extension of the Hash class in DotLiquid.  This adds support for importing from read-only dictionaries.
    /// </summary>
    public class ExtendedHash : Hash
    {
        /// <summary>
        /// Adds ability to create a Hash from a ReadOnlyDictionary
        /// </summary>
        /// <param name="dictionary">ReadOnly Dictionary object</param>
        /// <returns>Hash object</returns>
        public static Hash FromReadOnlyDictionary(IReadOnlyDictionary<string, object> dictionary)
        {
            Hash result = new Hash();

            foreach (var keyValue in dictionary)
            {
                if (keyValue.Value is IReadOnlyDictionary<string, object>)
                {
                    result.Add(keyValue.Key, FromReadOnlyDictionary((IReadOnlyDictionary<string, object>)keyValue.Value));
                }
                else
                {
                    result.Add(keyValue);
                }
            }

            return result;
        }
    }
}
