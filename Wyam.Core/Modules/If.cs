using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    // This executes the specified modules if the specified predicate is true
    // Any results from the specified modules (if run) will be returned as the result of the If module
    // Like a Branch module, but results replace context at the end (instead of being dropped)
    public class If : IModule
    {
        private readonly List<Tuple<Func<IModuleContext, bool>, IModule[]>> _conditions 
            = new List<Tuple<Func<IModuleContext, bool>, IModule[]>>();

        public If(Func<IModuleContext, bool> predicate, params IModule[] modules)
        {
            _conditions.Add(new Tuple<Func<IModuleContext, bool>, IModule[]>(predicate, modules));
        }

        public If ElseIf(Func<IModuleContext, bool> predicate, params IModule[] modules)
        {
            _conditions.Add(new Tuple<Func<IModuleContext, bool>, IModule[]>(predicate, modules));
            return this;
        }

        // Returns IModule instead of If to discourage further conditions
        public IModule Else(params IModule[] modules)
        {
            _conditions.Add(new Tuple<Func<IModuleContext, bool>, IModule[]>(x => true, modules));
            return this;
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            List<IModuleContext> results = new List<IModuleContext>();
            IEnumerable<IModuleContext> contexts = inputs;
            foreach (Tuple<Func<IModuleContext, bool>, IModule[]> condition in _conditions)
            {
                // Split the contexts into ones that satisfy the predicate and ones that don't
                List<IModuleContext> handled = new List<IModuleContext>();
                List<IModuleContext> unhandled = new List<IModuleContext>();
                foreach (IModuleContext context in contexts)
                {
                    if (condition.Item1 == null || condition.Item1(context))
                    {
                        handled.Add(context);
                    }
                    else
                    {
                        unhandled.Add(context);
                    }
                }

                // Run the modules on the ones that satisfy the predicate
                results.AddRange(pipeline.Execute(condition.Item2, handled));

                // Continue with the ones that don't satisfy the predicate
                contexts = unhandled;
            }

            // Add back any that never matched a predicate
            results.AddRange(contexts);

            return results;
        }
    }
}
