using System;

namespace Wyam.Razor
{
    internal class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
            // Do nothing
        }
    }
}