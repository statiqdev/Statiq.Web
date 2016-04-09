using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Extensibility
{
    /// <summary>
    /// Executes custom code for each input document.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// </remarks>
    /// <category>Extensibility</category>
    public class Execute : IModule
    {
        private readonly DocumentConfig _executeDocument;
        private readonly ContextConfig _executeContext;

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document. The delegate
        /// should return a <see cref="IEnumerable{IDocument}"/> or <see cref="IDocument"/>. If null is returned, this
        /// module will return the input documents. If anything else is returned, an exception will be thrown.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <see cref="IEnumerable{IDocument}"/>.</param>
        public Execute(DocumentConfig execute)
        {
            _executeDocument = execute;
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents. The delegate
        /// should return a <see cref="IEnumerable{IDocument}"/> or <see cref="IDocument"/>. If null is returned, this
        /// module will return the input documents. If anything else is returned, an exception will be thrown.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <see cref="IEnumerable{IDocument}"/>.</param>
        public Execute(ContextConfig execute)
        {
            _executeContext = execute;
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document.
        /// This will return the input documents.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <see cref="IEnumerable{IDocument}"/>.</param>
        public Execute(Action<IDocument, IExecutionContext> execute)
        {
            _executeDocument = (doc, ctx) =>
            {
                execute(doc, ctx);
                return null;
            };
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents.
        /// This will return the input documents.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <see cref="IEnumerable{IDocument}"/>.</param>
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
                return inputs.SelectMany(input => GetDocuments(_executeDocument(input, context)) ?? new [] { input });
            }
            return GetDocuments(_executeContext(context)) ?? inputs;
        }

        private IEnumerable<IDocument> GetDocuments(object result)
        {
            if (result == null)
            {
                return null;
            }
            IEnumerable<IDocument> documents = result as IEnumerable<IDocument>;
            if (documents == null)
            {
                IDocument document = result as IDocument;
                if (document != null)
                {
                    documents = new[] { document };
                }
            }
            if (documents != null)
            {
                return documents;
            }
            throw new Exception("Execute delegate must return IEnumerable<IDocument>, IDocument, or null");
        } 
    }
}