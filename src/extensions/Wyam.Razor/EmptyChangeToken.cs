using System;
using Microsoft.Extensions.Primitives;
using Wyam.Common.Util;

namespace Wyam.Razor
{
    internal class EmptyChangeToken : IChangeToken
    {
        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => EmptyDisposable.Instance;
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
    }
}