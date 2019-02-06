using System;
using System.Reflection;
using System.Text;
using JSPool;
using Wyam.Common.JavaScript;

namespace Wyam.Core.JavaScript
{
    /// <summary>
    /// Wraps a <see cref="JavaScriptEngine"/> but overrides the
    /// dispose behavior so that instead of disposing the
    /// underlying engine, it returns the engine to the pool.
    /// </summary>
    internal class PooledJavaScriptEngine : IJavaScriptEngine
    {
        private bool _disposed = false;

        public PooledJavaScriptEngine(JavaScriptEngine engine, JsPool<JavaScriptEngine> pool)
        {
            Engine = engine;
            Pool = pool;
        }

        internal JavaScriptEngine Engine { get; }

        internal JsPool<JavaScriptEngine> Pool { get; }

        public void Dispose()
        {
            CheckDisposed();
            Pool.ReturnEngineToPool(Engine);
            _disposed = true;
        }

        public string Name
        {
            get
            {
                CheckDisposed();
                return Engine.Name;
            }
        }

        public string Version
        {
            get
            {
                CheckDisposed();
                return Engine.Version;
            }
        }

        public object Evaluate(string expression)
        {
            CheckDisposed();
            return Engine.Evaluate(expression);
        }

        public T Evaluate<T>(string expression)
        {
            CheckDisposed();
            return Engine.Evaluate<T>(expression);
        }

        public void Execute(string code)
        {
            CheckDisposed();
            Engine.Execute(code);
        }

        public void ExecuteFile(string path, Encoding encoding = null)
        {
            CheckDisposed();
            Engine.ExecuteFile(path, encoding);
        }

        public void ExecuteResource(string resourceName, Type type)
        {
            CheckDisposed();
            Engine.ExecuteResource(resourceName, type);
        }

        public void ExecuteResource(string resourceName, Assembly assembly)
        {
            CheckDisposed();
            Engine.ExecuteResource(resourceName, assembly);
        }

        public object CallFunction(string functionName, params object[] args)
        {
            CheckDisposed();
            return Engine.CallFunction(functionName, args);
        }

        public T CallFunction<T>(string functionName, params object[] args)
        {
            CheckDisposed();
            return Engine.CallFunction<T>(functionName, args);
        }

        public bool HasVariable(string variableName)
        {
            CheckDisposed();
            return Engine.HasVariable(variableName);
        }

        public object GetVariableValue(string variableName)
        {
            CheckDisposed();
            return Engine.GetVariableValue(variableName);
        }

        public T GetVariableValue<T>(string variableName)
        {
            CheckDisposed();
            return Engine.GetVariableValue<T>(variableName);
        }

        public void SetVariableValue(string variableName, object value)
        {
            CheckDisposed();
            Engine.SetVariableValue(variableName, value);
        }

        public void RemoveVariable(string variableName)
        {
            CheckDisposed();
            Engine.RemoveVariable(variableName);
        }

        public void EmbedHostObject(string itemName, object value)
        {
            CheckDisposed();
            Engine.EmbedHostObject(itemName, value);
        }

        public void EmbedHostType(string itemName, Type type)
        {
            CheckDisposed();
            Engine.EmbedHostType(itemName, type);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PooledJavaScriptEngine));
            }
        }
    }
}