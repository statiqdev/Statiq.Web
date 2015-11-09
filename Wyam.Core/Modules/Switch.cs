using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    public class Switch : IModule
    {
        private readonly List<Tuple<object, IModule[]>> _cases
            = new List<Tuple<object, IModule[]>>();
        private IModule[] _defaultModules;
        private DocumentConfig _value;

        /// <summary>
        /// Must return an object
        /// </summary>
        /// <param name="value"></param>
        public Switch(DocumentConfig value)
        {
            _value = value;
        }

        /// <summary>
        /// value-parameter must be a primitive value or an array of primitives
        /// </summary>
        public Switch Case(object value, params IModule[] modules)
        {
            _cases.Add(new Tuple<object, IModule[]>(value, modules));
            return this;
        }

        public Switch Default(params IModule[] modules)
        {
            _defaultModules = modules;
            return this;
        }

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
                    object switchValue = _value.Invoke(document, context);
                    object caseValue = c.Item1 ?? Enumerable.Empty<object>();
                    IEnumerable caseValues = caseValue.GetType().IsArray ? (IEnumerable)caseValue : Enumerable.Repeat(caseValue, 1);
                    bool matches = caseValues.Cast<object>().Any(cv => object.Equals(switchValue, cv));
                    
                    if(matches)
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

            if(_defaultModules != null )
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
