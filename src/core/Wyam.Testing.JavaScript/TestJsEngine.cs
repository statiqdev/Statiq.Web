using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using IJsEngine = Wyam.Common.JavaScript.IJsEngine;

namespace Wyam.Testing.JavaScript
{
    public class TestJsEngine : IJsEngine
    {
        private readonly JavaScriptEngineSwitcher.Core.IJsEngine _engine;

        static TestJsEngine()
        {
            JsEngineSwitcher.Instance.EngineFactories.Add(new JintJsEngineFactory());
            JsEngineSwitcher.Instance.DefaultEngineName = JintJsEngine.EngineName;
        }

        public TestJsEngine()
        {
            _engine = JsEngineSwitcher.Instance.CreateDefaultEngine();
        }

        public TestJsEngine(string engineName)
        {
            _engine = JsEngineSwitcher.Instance.CreateEngine(engineName);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }

        public string Name => _engine.Name;

        public string Version => _engine.Version;

        public object Evaluate(string expression) => _engine.Evaluate(expression);

        public T Evaluate<T>(string expression) => _engine.Evaluate<T>(expression);

        public void Execute(string code) => _engine.Execute(code);

        public void ExecuteFile(string path, Encoding encoding = null) => _engine.ExecuteFile(path, encoding);

        public void ExecuteResource(string resourceName, Type type) => _engine.ExecuteResource(resourceName, type);

        public void ExecuteResource(string resourceName, Assembly assembly) => _engine.ExecuteResource(resourceName, assembly);

        public object CallFunction(string functionName, params object[] args) => _engine.CallFunction(functionName, args);

        public T CallFunction<T>(string functionName, params object[] args) => _engine.CallFunction<T>(functionName, args);

        public bool HasVariable(string variableName) => _engine.HasVariable(variableName);

        public object GetVariableValue(string variableName) => _engine.GetVariableValue(variableName);

        public T GetVariableValue<T>(string variableName) => _engine.GetVariableValue<T>(variableName);

        public void SetVariableValue(string variableName, object value) => _engine.SetVariableValue(variableName, value);

        public void RemoveVariable(string variableName) => _engine.RemoveVariable(variableName);

        public void EmbedHostObject(string itemName, object value) => _engine.EmbedHostObject(itemName, value);

        public void EmbedHostType(string itemName, Type type) => _engine.EmbedHostType(itemName, type);
    }
}
