using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Wyam.Common.Util;

namespace Wyam.Razor
{
    internal class ExecutionChangeToken : IChangeToken
    {
        private readonly List<(Action<object>, object)> _changeCallbacks = new List<(Action<object>, object)>();
        private bool _hasChanged;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            _changeCallbacks.Add((callback, state));
            return new ActionDisposable(() => _changeCallbacks.Clear());
        }

        public bool HasChanged => _hasChanged;
        public bool ActiveChangeCallbacks => false;

        public void Expire()
        {
            _hasChanged = true;
            foreach ((Action<object> Action, object State) callback in _changeCallbacks)
            {
                callback.Action(callback.State);
            }
        }
    }
}