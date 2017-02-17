using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.JavaScript
{
    /// <summary>
    /// Provides a shared pool of JavaScript engine instances. You should dispose the pool when
    /// no longer needed to properly dispose of any allocated engines.
    /// </summary>
    public interface IJsEnginePool : IDisposable
    {
        /// <summary>
        /// Gets an engine from the pool. This engine should be disposed when you are finished with it.
        /// If an engine is free, this method returns immediately with the engine.
        /// If no engines are available but we have not reached the maximum number of engines
        /// yet, creates a new engine. If the maximum number of engines has been reached, blocks until an engine is
        /// available again.
        /// </summary>
        /// <param name="timeout">
        /// Maximum time to wait for a free engine. If not specified, defaults to the timeout 
        /// specified when creating the pool.
        /// </param>
        /// <returns>A JavaScript engine.</returns>
        IJsEngine GetEngine(TimeSpan? timeout = null);

        /// <summary>
        /// Disposes the specified engine and removes it from the pool. A new engine will be created in it's place.
        /// </summary>
        /// <param name="engine"></param>
        void RecycleEngine(IJsEngine engine);

        /// <summary>
        /// Disposes all engines in this pool, and creates new engines in their place.
        /// </summary>
        void RecycleAllEngines();
    }
}
