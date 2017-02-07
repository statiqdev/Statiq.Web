using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;

namespace Wyam.Core.Modules.Extensibility
{
    /// <summary>
    /// Executes custom code that returns documents, modules, or new content.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the delegate is also useful for customizing existing modules based on the
    /// current set of documents. For example, you can use this module to execute the <see cref="Replace"/> module
    /// with customized search strings based on the results of other pipelines.
    /// </remarks>
    /// <category>Extensibility</category>
    public class Execute : IModule
    {
        private readonly DocumentConfig _executeDocument;
        private readonly bool _parallel;
        private readonly ContextConfig _executeContext;

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document. If the delegate
        /// returns a <see cref="IEnumerable{IDocument}"/> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns a <see cref="IEnumerable{IModule}"/> or
        /// <see cref="IModule"/>, the module(s) will be executed with each input document as their input
        /// and the results will be the output of this module. If the delegate returns null, 
        /// this module will just output the input document. If anything else is returned, the input
        /// document will be output with the string value of the delegate result as it's content.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <see cref="IEnumerable{IDocument}"/>,
        /// <see cref="IDocument"/>, <see cref="IEnumerable{IModule}"/>, <see cref="IModule"/>, object, or null.</param>
        /// <param name="parallel">The delegate is usually evaluated and each input document is processed in parallel.
        /// Setting this to <c>false</c> runs evaluates and processes each document in their original input order.</param>
        public Execute(DocumentConfig execute, bool parallel = true)
        {
            _executeDocument = execute;
            _parallel = parallel;
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents. If the delegate
        /// returns a <see cref="IEnumerable{IDocument}"/> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns a <see cref="IEnumerable{IModule}"/> or
        /// <see cref="IModule"/>, the module(s) will be executed with the input documents as their input
        /// and the results will be the output of this module. If the delegate returns null, 
        /// this module will just output the input documents. If anything else is returned, an exception will be thrown.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <see cref="IEnumerable{IDocument}"/>,
        /// <see cref="IDocument"/>, <see cref="IEnumerable{IModule}"/>, <see cref="IModule"/>, or null.</param>
        public Execute(ContextConfig execute)
        {
            _executeContext = execute;
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute on each input document.</param>
        /// <param name="parallel">The delegate is usually evaluated and each input document is processed in parallel.
        /// Setting this to <c>false</c> runs evaluates and processes each document in their original input order.</param>
        public Execute(Action<IDocument, IExecutionContext> execute, bool parallel = true)
        {
            _executeDocument = (doc, ctx) =>
            {
                execute(doc, ctx);
                return null;
            };
            _parallel = parallel;
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute.</param>
        public Execute(Action<IExecutionContext> execute)
        {
            _executeContext = ctx =>
            {
                execute(ctx);
                return null;
            };
        }

        IEnumerable<IDocument> IModule.Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_executeDocument != null)
            {
                Func<IDocument, IEnumerable<IDocument>> selectMany = input =>
                {
                    object documentResult = _executeDocument(input, context);
                    if (documentResult == null)
                    {
                        return new[] {input};
                    }
                    return GetDocuments(documentResult)
                           ?? ExecuteModules(documentResult, context, new[] {input})
                           ?? ChangeContent(documentResult, context, input);
                };
                return _parallel 
                    ? inputs.AsParallel().SelectMany(context, selectMany) 
                    : inputs.SelectMany(context, selectMany);
            }

            object contextResult = _executeContext(context);
            if (contextResult == null)
            {
                return inputs;
            }
            return GetDocuments(contextResult)
                ?? ExecuteModules(contextResult, context, inputs)
                ?? ThrowInvalidDelegateResult(contextResult);
        }


        private IEnumerable<IDocument> GetDocuments(object result)
        {
            IEnumerable<IDocument> documents = result as IEnumerable<IDocument>;
            if (documents == null)
            {
                IDocument document = result as IDocument;
                if (document != null)
                {
                    documents = new[] { document };
                }
            }
            return documents;
        }

        private IEnumerable<IDocument> ExecuteModules(object results, IExecutionContext context, IEnumerable<IDocument> inputs)
        {
            IEnumerable<IModule> modules = results as IEnumerable<IModule>;
            if (modules == null)
            {
                IModule module = results as IModule;
                if (module != null)
                {
                    modules = new[] {module};
                }
            }
            return modules != null ? context.Execute(modules, inputs) : null;
        }

        private IEnumerable<IDocument> ChangeContent(object result, IExecutionContext context, IDocument document) => 
            new[] {context.GetDocument(document, result.ToString())};

        private IEnumerable<IDocument> ThrowInvalidDelegateResult(object result)
        {
            throw new Exception($"Execute delegate must return IEnumerable<IDocument>, IDocument, IEnumerable<IModule>, IModule, or null; {result.GetType().Name} is an invalid return type");
        }
    }
}