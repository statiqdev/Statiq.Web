using System;

namespace Wyam.Common.Util
{
    /// <summary>
    /// A disposable class that does nothing.
    /// </summary>
    public class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance = new EmptyDisposable();

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
            // Do nothing
        }
    }
}