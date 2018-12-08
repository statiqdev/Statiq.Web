using System;

namespace Wyam.Common.Util
{
    /// <summary>
    /// A disposable class that does nothing.
    /// </summary>
    public class EmptyDisposable : IDisposable
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// A singleton instance of the <see cref="EmptyDisposable"/>.
        /// </summary>
        public static EmptyDisposable Instance = new EmptyDisposable();
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
            // Do nothing
        }
    }
}