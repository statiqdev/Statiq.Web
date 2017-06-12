using System;

namespace Wyam.Common.Util
{
    /// <summary>
    /// A simple disposable that calls an action on disposal. This class
    /// will also throw an exception on subsiquent disposals.
    /// </summary>
    public class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        private bool _disposed;

        /// <summary>
        /// Create a disposable instance.
        /// </summary>
        /// <param name="action">The action to call on disposal.</param>
        public ActionDisposable(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Calls the action.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ActionDisposable));
            }
            _disposed = true;
            _action();
        }
    }
}