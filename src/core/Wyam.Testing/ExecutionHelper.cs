using System.Collections.Generic;

namespace Wyam.Testing
{
    /// <summary>
    /// Execution Helpers
    /// </summary>
    public static class ExecutionHelper
    {
        /// <summary>
        /// Enumerate a enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable to enumerate.</param>
        /// <typeparam name="T">Generic type of the enumerable.</typeparam>
        public static void Enumerate<T>(this IEnumerable<T> enumerable)
        {
            // ReSharper disable once UnusedVariable
            foreach (T nothing in enumerable)
            {
            }
        }
    }
}