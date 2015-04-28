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

        public IReadOnlyList<IModuleContext> Execute(IEnumerable<IModule> modules, IEnumerable<IModuleContext> inputContexts)
        {
            List<IModuleContext> contexts = inputContexts == null ? new List<IModuleContext>() : inputContexts.ToList();
            foreach (IModule module in modules.Where(x => x != null))
            {
                string moduleName = module.GetType().Name;
                Trace.Verbose("Executing module {0} with {1} input(s)...", moduleName, contexts.Count);
                try
                {
                    // Make sure we clone the output context if it's the same as the input
                    IEnumerable<IModuleContext> outputs = module.Execute(contexts, this);
                    contexts = outputs == null ? new List<IModuleContext>() : outputs.Where(x => x != null).ToList();
                    Trace.Verbose("Executed module {0} resulting in {1} output(s).", moduleName, contexts.Count);
                }
                catch (Exception ex)
                {
                    Trace.Error("Error while executing module {0}: {1}", moduleName, ex.Message);
                    Trace.Verbose(ex.ToString());
                    contexts = new List<IModuleContext>();
                }
            }
            return contexts.AsReadOnly();
        }

        public IReadOnlyList<IModuleContext> Execute(Metadata initialMetadata)
        {
            IReadOnlyList<IModuleContext> contexts = new List<IModuleContext>
            {
                new ModuleContext(initialMetadata)
            };
            return Execute(_modules, contexts);
        }
    }
}
