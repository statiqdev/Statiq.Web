using System;
using System.Reflection;
using System.Text;

namespace Wyam.Common.JavaScript
{
    /// <summary>
    /// A common interface to a JavaScript engine. Every JavaScript engine is
    /// obtained from a <see cref="IJavaScriptEnginePool"/> and will be returned to the
    /// pool when it is disposed. Therefore, you must dispose the engine when
    /// you are done with it.
    /// </summary>
    public interface IJavaScriptEngine : IDisposable
    {
        /// <summary>
        /// Gets the name of JavaScript engine.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of original JavaScript engine.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <param name="expression">JavaScript expression.</param>
        /// <returns>Result of the expression.</returns>
        object Evaluate(string expression);

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <typeparam name="T">Type of result.</typeparam>
        /// <param name="expression">JavaScript expression.</param>
        /// <returns>Result of the expression.</returns>
        T Evaluate<T>(string expression);

        /// <summary>
        /// Executes JavaScript code.
        /// </summary>
        /// <param name="code">The JavaScript code to execute.</param>
        void Execute(string code);

        /// <summary>
        /// Executes code from JavaScript file.
        /// </summary>
        /// <param name="path">Path to the JavaScript file.</param>
        /// <param name="encoding">Text encoding.</param>
        void ExecuteFile(string path, Encoding encoding = null);

        /// <summary>
        /// Executes code from embedded JavaScript resource.
        /// </summary>
        /// <param name="resourceName">The case-sensitive resource name without the namespace of the specified type.</param>
        /// <param name="type">The type, that determines the assembly and whose namespace is used to scope
        /// the resource name.</param>
        void ExecuteResource(string resourceName, Type type);

        /// <summary>
        /// Executes code from embedded JavaScript resource.
        /// </summary>
        /// <param name="resourceName">The case-sensitive resource name.</param>
        /// <param name="assembly">The assembly, which contains the embedded resource.</param>
        void ExecuteResource(string resourceName, Assembly assembly);

        /// <summary>
        /// Calls a JavaScript function.
        /// </summary>
        /// <param name="functionName">Function name.</param>
        /// <param name="args">Function arguments.</param>
        /// <returns>Result of the function execution.</returns>
        object CallFunction(string functionName, params object[] args);

        /// <summary>
        /// Calls a JavaScript function.
        /// </summary>
        /// <typeparam name="T">Type of function result.</typeparam>
        /// <param name="functionName">Function name.</param>
        /// <param name="args">Function arguments.</param>
        /// <returns>Result of the function execution.</returns>
        T CallFunction<T>(string functionName, params object[] args);

        /// <summary>
        /// Сhecks for the existence of a variable.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns><c>true</c> if the variable exists, otherwise <c>false</c>.</returns>
        bool HasVariable(string variableName);

        /// <summary>
        /// Gets the value of variable.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of variable.</returns>
        object GetVariableValue(string variableName);

        /// <summary>
        /// Gets the value of variable.
        /// </summary>
        /// <typeparam name="T">Type of variable.</typeparam>
        /// <param name="variableName">Variable name.</param>
        /// <returns>Value of variable.</returns>
        T GetVariableValue<T>(string variableName);

        /// <summary>
        /// Sets the value of variable.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        /// <param name="value">Value of variable.</param>
        void SetVariableValue(string variableName, object value);

        /// <summary>
        /// Removes a variable.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        void RemoveVariable(string variableName);

        /// <summary>
        /// Embeds a host object to script code.
        /// </summary>
        /// <param name="itemName">The name for the new global variable or function that will represent the object.</param>
        /// <param name="value">The object to expose.</param>
        /// <remarks>Allows to embed instances of simple classes (or structures) and delegates.</remarks>
        void EmbedHostObject(string itemName, object value);

        /// <summary>
        /// Embeds a host type to script code.
        /// </summary>
        /// <param name="itemName">The name for the new global variable that will represent the type.</param>
        /// <param name="type">The type to expose.</param>
        /// <remarks>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members.
        /// </remarks>
        void EmbedHostType(string itemName, Type type);
    }
}
