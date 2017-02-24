using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.JavaScript;

namespace Wyam.Core.JavaScript
{
    internal class JsEngine : IJsEngine
    {
        private readonly JavaScriptEngineSwitcher.Core.IJsEngine _engine;
        private bool _disposed = false;

        public JsEngine(JavaScriptEngineSwitcher.Core.IJsEngine engine)
        {
            _engine = engine;
        }

        public void Dispose()
        {
            CheckDisposed();
            _engine.Dispose();
            _disposed = true;
        }

        public string Name
        {
            get
            {
                CheckDisposed();
                return _engine.Name;
            }
        }

        public string Version
        {
            get
            {
                CheckDisposed();
                return _engine.Version;
            }
        }

        public object Evaluate(string expression)
        {
            CheckDisposed();
            return _engine.Evaluate(expression);
        }

        public T Evaluate<T>(string expression)
        {
            CheckDisposed();
            return _engine.Evaluate<T>(expression);
        }

        public void Execute(string code)
        {
            CheckDisposed();
            _engine.Execute(code);
        }

        public void ExecuteFile(string path, Encoding encoding = null)
        {
            CheckDisposed();
            _engine.ExecuteFile(path, encoding);
        }

        public void ExecuteResource(string resourceName, Type type)
        {
            CheckDisposed();
            _engine.ExecuteResource(resourceName, type);
        }

        public void ExecuteResource(string resourceName, Assembly assembly)
        {
            CheckDisposed();
            _engine.ExecuteResource(resourceName, assembly);
        }

        public object CallFunction(string functionName, params object[] args)
        {
            CheckDisposed();
            return _engine.CallFunction(functionName, args);
        }

        public T CallFunction<T>(string functionName, params object[] args)
        {
            CheckDisposed();
            return _engine.CallFunction<T>(functionName, args);
        }

        public bool HasVariable(string variableName)
        {
            CheckDisposed();
            return _engine.HasVariable(variableName);
        }

        public object GetVariableValue(string variableName)
        {
            CheckDisposed();
            return _engine.GetVariableValue(variableName);
        }

        public T GetVariableValue<T>(string variableName)
        {
            CheckDisposed();
            return _engine.GetVariableValue<T>(variableName);
        }

        public void SetVariableValue(string variableName, object value)
        {
            CheckDisposed();
            _engine.SetVariableValue(variableName, value);
        }

        public void RemoveVariable(string variableName)
        {
            CheckDisposed();
            _engine.RemoveVariable(variableName);
        }

        public void EmbedHostObject(string itemName, object value)
        {
            CheckDisposed();
            _engine.EmbedHostObject(itemName, value);
        }

        public void EmbedHostType(string itemName, Type type)
        {
            CheckDisposed();
            _engine.EmbedHostType(itemName, type);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(JsEngine));
            }
        }
    }
}
