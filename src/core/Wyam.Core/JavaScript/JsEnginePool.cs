using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JSPool;
using Wyam.Common.JavaScript;
using IJsEngine = Wyam.Common.JavaScript.IJsEngine;

namespace Wyam.Core.JavaScript
{
    internal class JsEnginePool : IJsEnginePool
    {
        private readonly JsPool<JsEngine> _pool;
        private bool _disposed = false;

        public JsEnginePool(Action<IJsEngine> initializer,
            int startEngines,
            int maxEngines,
            int maxUsagesPerEngine,
            TimeSpan engineTimeout)
        {
            // First we need to check if the JsEngineSwitcher has been configured. We'll do this
            // by checking the DefaultEngineName being set. If that's there we can safely assume
            // its been configured somehow (maybe via a configuration file). If not we'll wire up
            // Jint as the default engine.
            if (string.IsNullOrWhiteSpace(JsEngineSwitcher.Instance.DefaultEngineName))
            {
                JsEngineSwitcher.Instance.EngineFactories.Add(new JintJsEngineFactory());
                JsEngineSwitcher.Instance.DefaultEngineName = JintJsEngine.EngineName;
            }

            _pool = new JsPool<JsEngine>(new JsPoolConfig<JsEngine>
            {
                EngineFactory = () => new JsEngine(JsEngineSwitcher.Instance.CreateDefaultEngine()),
                Initializer = x => initializer?.Invoke(x),
                StartEngines = startEngines,
                MaxEngines = maxEngines,
                MaxUsagesPerEngine = maxUsagesPerEngine,
                GetEngineTimeout = engineTimeout
            });
        }

        public void Dispose()
        {
            CheckDisposed();
            _pool.Dispose();
            _disposed = true;
        }

        public IJsEngine GetEngine(TimeSpan? timeout = null) => new PooledJsEngine(_pool.GetEngine(timeout), _pool);

        public void RecycleEngine(IJsEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }
            PooledJsEngine pooledEngine = engine as PooledJsEngine;
            if (pooledEngine == null)
            {
                throw new ArgumentException("The specified engine was not from a pool");
            }
            if (pooledEngine.Pool != _pool)
            {
                throw new ArgumentException("The specified engine is from a different pool");
            }
            _pool.DisposeEngine(pooledEngine.Engine);
        }

        public void RecycleAllEngines() => _pool.Recycle();

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(JsPool));
            }
        }
    }
}
