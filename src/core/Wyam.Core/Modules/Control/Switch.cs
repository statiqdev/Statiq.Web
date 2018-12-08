using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Executes sequences of modules depending on whether the input document contains a specified value.
    /// </summary>
    /// <remarks>
    /// When creating a Switch module you specify a delegate that will get an object for each document. Cases are then
    /// defined via fluent methods that compare the returned object for each document against a supplied object (or array).
    /// If the defined object or any of the objects in the array for the case equal the one for the document, the modules
    /// in the case are executed. The output of the module is the aggregate output of executing the specified modules against
    /// documents matching each case. If a document document match a case, it is output against the default case (if defined)
    /// or output without modification (if no default is defined).
    /// </remarks>
    /// <category>Control</category>
    public class Switch : IModule
    {
        private readonly List<Tuple<object, IModule[]>> _cases
            = new List<Tuple<object, IModule[]>>();
        private readonly DocumentConfig _value;
        private IModule[] _defaultModules;

        /// <summary>
        /// Defines the delegate that will be invoked against each input document to get the case comparison value.
        /// </summary>
        /// <param name="value">A delegate that returns an object to compare cases against.</param>
        public Switch(DocumentConfig value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            _value = value;
        }

        /// <summary>
        /// Defines a case.
        /// </summary>
        /// <param name="value">The value to compare to the one returned by the document delegate. Must be a primitive object or an array of primitive objects.</param>
        /// <param name="modules">The modules to execute if the case object (or any objects in the array) matches the document object.</param>
        /// <returns>The current module instance.</returns>
        public Switch Case(object value, params IModule[] modules)
        {
            _cases.Add(new Tuple<object, IModule[]>(value, modules));
            return this;
        }

        /// <summary>
        /// Defines modules to execute against documents that don't match a case.
        /// </summary>
        /// <param name="modules">The modules to execute against documents that don't match a case.</param>
        /// <returns>The current module instance.</returns>
        public Switch Default(params IModule[] modules)
        {
            _defaultModules = modules;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            IEnumerable<IDocument> documents = inputs;
            foreach (Tuple<object, IModule[]> c in _cases)
            {
                List<IDocument> handled = new List<IDocument>();
                List<IDocument> unhandled = new List<IDocument>();

                foreach (IDocument document in documents)
                {
                    object switchValue = _value.Invoke<object>(document, context);
                    object caseValue = c.Item1 ?? Array.Empty<object>();
                    IEnumerable caseValues = caseValue.GetType().IsArray ? (IEnumerable)caseValue : Enumerable.Repeat(caseValue, 1);
                    bool matches = caseValues.Cast<object>().Any(cv => object.Equals(switchValue, cv));

                    if (matches)
                    {
                        handled.Add(document);
                    }
                    else
                    {
                        unhandled.Add(document);
                    }
                }

                results.AddRange(context.Execute(c.Item2, handled));
                documents = unhandled;
            }

            if (_defaultModules != null)
            {
                results.AddRange(context.Execute(_defaultModules, documents));
            }
            else
            {
                results.AddRange(documents);
            }

            return results;
        }
    }
}
