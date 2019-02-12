using System;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Shortcodes
{
    public interface IShortcodeCollection : IReadOnlyShortcodeCollection
    {
        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="name">The name of the shortcode.</param>
        void Add<TShortcode>(string name)
            where TShortcode : IShortcode;

        /// <summary>
        /// Adds a shortcode by type, infering the name from the type name.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        void Add<TShortcode>()
            where TShortcode : IShortcode;

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        void Add(string name, Type type);

        /// <summary>
        /// Adds a shortcode by type, infering the name from the type name.
        /// </summary>
        /// <param name="type">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        void Add(Type type);

        /// <summary>
        /// Adds a shortcode and specifies the result content.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="result">The result of the shortcode.</param>
        void Add(string name, string result);

        /// <summary>
        /// Adds a shortcode and uses a <see cref="ContextConfig"/> to determine
        /// the shortcode result.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="contextConfig">A delegate that should return a <see cref="string"/>.</param>
        void Add(string name, ContextConfig contextConfig);

        /// <summary>
        /// Adds a shortcode and uses a <see cref="DocumentConfig"/> to determine
        /// the shortcode result.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="documentConfig">A delegate that should return a <see cref="string"/>.</param>
        void Add(string name, DocumentConfig documentConfig);

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared content.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">A function that has the declared content as an input and the result content as an output.</param>
        void Add(string name, Func<string, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared arguments.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">A function that has the declared arguments as an input and the result content as an output.</param>
        void Add(string name, Func<KeyValuePair<string, string>[], string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content using the declared arguments and content.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">A function that has the declared arguments and content as inputs and the result content as an output.</param>
        void Add(string name, Func<KeyValuePair<string, string>[], string, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and content and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        void Add(string name, Func<KeyValuePair<string, string>[], string, IExecutionContext, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and the current execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        void Add(string name, Func<KeyValuePair<string, string>[], IExecutionContext, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        void Add(string name, Func<KeyValuePair<string, string>[], IDocument, IExecutionContext, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared content and the current execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared content and the current execution context as inputs
        /// and the result content as an output.
        /// </param>
        void Add(string name, Func<string, IExecutionContext, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared content and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared content and the current document and execution context as inputs
        /// and the result content as an output.
        /// </param>
        void Add(string name, Func<string, IDocument, IExecutionContext, string> func);

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="func">
        /// A function that has the declared arguments and content and the current document and execution context as inputs
        /// and a <see cref="IShortcodeResult"/> as an output which allows the shortcode to add metadata to the document.
        /// </param>
        void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IShortcodeResult> func);
    }
}
