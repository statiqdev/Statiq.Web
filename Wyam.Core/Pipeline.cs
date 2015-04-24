using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class Pipeline : IPipeline, IPipelineContext
    {
        private readonly Engine _engine;
        private readonly IModule[] _modules;
        private readonly IReadOnlyList<IModuleContext> _completedContexts;

        public Pipeline(Engine engine, IModule[] modules, IReadOnlyList<IModuleContext> completedContexts)
        {
            _engine = engine;
            _modules = modules;
            _completedContexts = completedContexts;
        }

        public int Count
        {
            get { return _modules.Length; }
        }

        public Trace Trace
        {
            get { return _engine.Trace; }
        }

        public IReadOnlyList<IModuleContext> CompletedContexts
        {
            get { return _completedContexts; }
        }

        public IReadOnlyList<IModuleContext> Execute(IModule module, IEnumerable<IModuleContext> contexts)
        {
            string moduleName = module.GetType().Name;
            List<IModuleContext> inputs = contexts == null ? new List<IModuleContext>() : contexts.ToList();
            Trace.Verbose("Executing module {0} with {1} input(s)...", moduleName, inputs.Count);
            try
            {
                // Make sure we clone the output context if it's the same as the input
                IEnumerable<IModuleContext> outputs = module.Execute(inputs, this);
                List<IModuleContext> results = outputs == null ? new List<IModuleContext>() : outputs.ToList();
                Trace.Verbose("Executed module {0} resulting in {1} output(s).", moduleName, results.Count);
                return results.AsReadOnly();
            }
            catch (Exception ex)
            {
                Trace.Error("Error while executing module {0}: {1}", moduleName, ex.Message);
                Trace.Verbose(ex.ToString());
                return new List<IModuleContext>().AsReadOnly();
            }
        }

        public IReadOnlyList<IModuleContext> Execute(Metadata initialMetadata)
        {
            IReadOnlyList<IModuleContext> contexts = new List<IModuleContext>()
            {
                new ModuleContext(initialMetadata)
            };
            foreach (IModule module in _modules)
            {
                contexts = Execute(module, contexts);
            }
            return contexts;
        }
    }
}
