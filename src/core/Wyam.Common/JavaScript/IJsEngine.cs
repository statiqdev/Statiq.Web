using System;
using System.Reflection;
using System.Text;

namespace Wyam.Common.JavaScript
{
    /// <summary>
    /// Defines a interface of JS engine
    /// </summary>
    public interface IJsEngine : IDisposable
    {
        /// <summary>
        /// Gets the name of JS engine
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets the version of original JS engine
        /// </summary>
        string Version
        {
            get;
        }

        /// <summary>
        /// Evaluates an expression
        /// </summary>
        /// <param name="expression">JS-expression</param>
        /// <returns>Result of the expression</returns>
        object Evaluate(string expression);

        /// <summary>
        /// Evaluates an expression
        /// </summary>
        /// <typeparam name="T">Type of result</typeparam>
        /// <param name="expression">JS-expression</param>
        /// <returns>Result of the expression</returns>
        T Evaluate<T>(string expression);

        /// <summary>
        /// Executes code
        /// </summary>
        /// <param name="code">Code</param>
        void Execute(string code);

        /// <summary>
        /// Executes code from JS-file. If the file should only be loaded once per lifetime of the 
        /// JsEngine, see <see cref="RequireFile"/>
        /// </summary>
        /// <param name="path">Path to the JS-file</param>
        /// <param name="encoding">Text encoding</param>
        void ExecuteFile(string path, Encoding encoding = null);

        /// <summary>
        /// Executes code from embedded JS-resource. If the resource should only be loaded once per lifetime of the 
        /// JsEngine, see <see cref="RequireResource"/>
        /// </summary>
        /// <param name="resourceName">The case-sensitive resource name without the namespace of the specified type</param>
        /// <param name="type">The type, that determines the assembly and whose namespace is used to scope
        /// the resource name</param>
        void ExecuteResource(string resourceName, Type type);

        /// <summary>
        /// Executes code from embedded JS-resource. If the resource should only be loaded once per lifetime of the 
        /// JsEngine, see <see cref="RequireResource"/>
        /// </summary>
        /// <param name="resourceName">The case-sensitive resource name</param>
        /// <param name="assembly">The assembly, which contains the embedded resource</param>
        void ExecuteResource(string resourceName, Assembly assembly);

        /// <summary>
        /// Calls a function
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <param name="args">Function arguments</param>
        /// <returns>Result of the function execution</returns>
        object CallFunction(string functionName, params object[] args);

        /// <summary>
        /// Calls a function
        /// </summary>
        /// <typeparam name="T">Type of function result</typeparam>
        /// <param name="functionName">Function name</param>
        /// <param name="args">Function arguments</param>
        /// <returns>Result of the function execution</returns>
        T CallFunction<T>(string functionName, params object[] args);

        /// <summary>
        /// Сhecks for the existence of a variable
        /// </summary>
        /// <param name="variableName">Variable name</param>
        /// <returns>Result of check (true - exists; false - not exists</returns>
        bool HasVariable(string variableName);

        /// <summary>
        /// Gets the value of variable
        /// </summary>
        /// <param name="variableName">Variable name</param>
        /// <returns>Value of variable</returns>
        object GetVariableValue(string variableName);

        /// <summary>
        /// Gets the value of variable
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <param name="variableName">Variable name</param>
        /// <returns>Value of variable</returns>
        T GetVariableValue<T>(string variableName);

        /// <summary>
        /// Sets the value of variable
        /// </summary>
        /// <param name="variableName">Variable name</param>
        /// <param name="value">Value of variable</param>
        void SetVariableValue(string variableName, object value);

        /// <summary>
        /// Removes a variable
        /// </summary>
        /// <param name="variableName">Variable name</param>
        void RemoveVariable(string variableName);

        /// <summary>
        /// Embeds a host object to script code
        /// </summary>
        /// <param name="itemName">The name for the new global variable or function that will represent the object</param>
        /// <param name="value">The object to expose</param>
        /// <remarks>Allows to embed instances of simple classes (or structures) and delegates.</remarks>
        void EmbedHostObject(string itemName, object value);

        /// <summary>
        /// Embeds a host type to script code
        /// </summary>
        /// <param name="itemName">The name for the new global variable that will represent the type</param>
        /// <param name="type">The type to expose</param>
        /// <remarks>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members.
        /// </remarks>
        void EmbedHostType(string itemName, Type type);

        /// <summary>
        /// Ensures a resource is loaded for this engine. If it is already 
        /// loaded then nothing is executed.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="type">The type.</param>
        void RequireResource(string resourceName, Type type);

        /// <summary>
        /// Ensures a resource is loaded for this engine. If it is already 
        /// loaded then nothing is executed.
        /// </summary>
        /// <param name="path">The path is load.</param>
        void RequireFile(string path);
    }
}
