using System;

namespace Wyam.Common.Util
{
    /// <summary>
    /// A disposable class that does nothing.
    /// </summary>
    public class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
            // Do nothing
        }
    }
}