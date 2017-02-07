using System;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

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
        private readonly List<Condition> _conditions = new List<Condition>();
        
        /// <summary>
        /// Specifies a predicate and a series of child modules to be evaluated if the predicate returns <c>true</c>.
        /// The predicate will be evaluated against every input document individually.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        public If(DocumentConfig predicate, params IModule[] modules)
        {
            _conditions.Add(new Condition(predicate, modules));
        }

        /// <summary>
        /// Specifies a predicate and a series of child modules to be evaluated if the predicate returns <c>true</c>.
        /// The predicate will be evaluated once for all input documents.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents if the predicate is <c>true</c>.</param>
        public If(ContextConfig predicate, params IModule[] modules)
        {
            _conditions.Add(new Condition(predicate, modules));
        }

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy 
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// The predicate will be evaluated against every input document individually.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        public If ElseIf(DocumentConfig predicate, params IModule[] modules)
        {
            _conditions.Add(new Condition(predicate, modules));
            return this;
        }

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy 
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// The predicate will be evaluated once for all input documents.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents if the predicate is <c>true</c>.</param>
        public If ElseIf(ContextConfig predicate, params IModule[] modules)
        {
            _conditions.Add(new Condition(predicate, modules));
            return this;
        }

        /// <summary>
        /// This should be at the end of your fluent method chain and will evaluate the 
        /// specified child modules on all documents that did not satisfy previous predicates.
        /// The predicate will be evaluated against every input document individually.
        /// </summary>
        /// <param name="modules">The modules to execute on documents where no previous predicate was <c>true</c>.</param>
        public IModule Else(params IModule[] modules)
        {
            _conditions.Add(new Condition(ctx => true, modules));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            IReadOnlyList<IDocument> documents = inputs;
            foreach (Condition condition in _conditions)
            {
                // Split the documents into ones that satisfy the predicate and ones that don't
                List<IDocument> matched = new List<IDocument>();
                List<IDocument> unmatched = new List<IDocument>();
                if (condition.ContextConfig != null && condition.ContextConfig.Invoke<bool>(context, "while evaluating condition"))
                {
                    matched.AddRange(documents);
                }
                else if(condition.ContextConfig == null)
                {
                    context.ForEach(documents, document =>
                    {
                        if (condition.DocumentConfig == null || condition.DocumentConfig.Invoke<bool>(document, context, "while evaluating condition"))
                        {
                            matched.Add(document);
                        }
                        else
                        {
                            unmatched.Add(document);
                        }
                    });
                }

                // Run the modules on the documents that satisfy the predicate
                if (matched.Count > 0)
                {
                    results.AddRange(context.Execute(condition.Modules, matched));
                }

                // Continue with the documents that don't satisfy the predicate
                documents = unmatched;
            }

            // Add back any documents that never matched a predicate
            results.AddRange(documents);

            return results;
        }

        private class Condition
        {
            public DocumentConfig DocumentConfig { get; }
            public ContextConfig ContextConfig { get; }
            public IModule[] Modules { get; }

            public Condition(DocumentConfig documentConfig, IModule[] modules)
            {
                DocumentConfig = documentConfig;
                Modules = modules;
            }

            public Condition(ContextConfig contextConfig, IModule[] modules)
            {
                ContextConfig = contextConfig;
                Modules = modules;
            }
        }
    }
}
