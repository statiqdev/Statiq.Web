using System;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Evaluates a series of child modules for each input document if a specified condition is met. 
    /// </summary>
    /// <remarks>
    /// Any result documents from the child modules will be returned as the result of the 
    /// this module. Any input modules that don't match a predicate will be returned as 
    /// outputs without modification.
    /// </remarks>
    /// <category>Control</category>
    public class If : IModule
    {
        private readonly List<Tuple<DocumentConfig, IModule[]>> _conditions 
            = new List<Tuple<DocumentConfig, IModule[]>>();
        
        /// <summary>
        /// Specifies a predicate and a series of child modules to be evaluated if the predicate returns <c>true</c>.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        public If(DocumentConfig predicate, params IModule[] modules)
        {
            _conditions.Add(new Tuple<DocumentConfig, IModule[]>(predicate, modules));
        }

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy 
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        public If ElseIf(DocumentConfig predicate, params IModule[] modules)
        {
            _conditions.Add(new Tuple<DocumentConfig, IModule[]>(predicate, modules));
            return this;
        }

        /// <summary>
        /// This should be at the end of your fluent method chain and will evaluate the 
        /// specified child modules on all documents that did not satisfy previous predicates.
        /// </summary>
        /// <param name="modules">The modules to execute on documents where no previous predicate was <c>true</c>.</param>
        public IModule Else(params IModule[] modules)
        {
            _conditions.Add(new Tuple<DocumentConfig, IModule[]>((x, y) => true, modules));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            IEnumerable<IDocument> documents = inputs;
            foreach (Tuple<DocumentConfig, IModule[]> condition in _conditions)
            {
                // Split the documents into ones that satisfy the predicate and ones that don't
                List<IDocument> handled = new List<IDocument>();
                List<IDocument> unhandled = new List<IDocument>();
                foreach (IDocument document in documents)
                {
                    if (condition.Item1 == null || condition.Item1.Invoke<bool>(document, context))
                    {
                        handled.Add(document);
                    }
                    else
                    {
                        unhandled.Add(document);
                    }
                }

                // Run the modules on the documents that satisfy the predicate
                results.AddRange(context.Execute(condition.Item2, handled));

                // Continue with the documents that don't satisfy the predicate
                documents = unhandled;
            }

            // Add back any documents that never matched a predicate
            results.AddRange(documents);

            return results;
        }
    }
}
