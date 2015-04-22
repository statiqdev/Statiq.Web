using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class Pipeline : IPipeline
    {
        private readonly Engine _engine;
        private readonly List<Module> _modules = new List<Module>();

        public Pipeline(Engine engine)
        {
            _engine = engine;
        }

        public IPipeline AddModule(Module module)
        {
            _modules.Add(module);
            return this;
        }

        public int Count
        {
            get { return _modules.Count; }
        }

        public PrepareTree Prepare(Metadata metadata, IEnumerable<IMetadata> allMetadata)
        {
            PrepareBranch rootBranch = new PrepareBranch(new PipelineContext(_engine, metadata, allMetadata));
            List<PrepareBranch> lastBranches = new List<PrepareBranch>() 
            { 
                new PrepareBranch(null) 
                { 
                    Outputs = new List<PrepareBranch>() { rootBranch } 
                }
            };

            foreach(Module module in _modules)
            {
                _engine.Trace.Verbose("Preparing module {0}...", module.GetType().Name);
                List<PrepareBranch> currentBranches = new List<PrepareBranch>();
                int i = 0;
                foreach (PrepareBranch lastBranch in lastBranches)
                {
                    foreach (PrepareBranch currentBranch in lastBranch.Outputs)
                    {
                        try
                        {
                            currentBranch.Module = module;
                            currentBranch.Outputs = module.Prepare(currentBranch.Context)
                                .Select(x => new PrepareBranch(x == currentBranch.Context ? x.Clone() : x))  // Make sure we clone the context if it's the same as the input
                                .ToList();
                            currentBranches.Add(currentBranch);
                            i++;
                        }
                        catch (Exception ex)
                        {
                            _engine.Trace.Error("Error while preparing module {0}: {1}", module.GetType().Name, ex.Message);
                            _engine.Trace.Verbose(ex.ToString());
                        }
                    }
                }
                lastBranches = currentBranches;
                _engine.Trace.Verbose("Prepared module {0} with {1} input(s) and resulting in {2} output(s).", module.GetType().Name, i, currentBranches.Sum(x => x.Outputs.Count));
            }

            return new PrepareTree(rootBranch, lastBranches.SelectMany(x => x.Outputs));
        }

        public void Execute(PrepareBranch branch)
        {
            if(branch.Module == null)
            {
                // This is a leaf, so no further recursion required
                return;
            }

            // We need to execute the module for each output that was returned during prepare
            foreach (PrepareBranch output in branch.Outputs)
            {
                _engine.Trace.Verbose("Executing module {0}...", branch.Module.GetType().Name);
                try
                {
                    output.Content = branch.Module.Execute(output.Context, branch.Content);
                }
                catch (Exception ex)
                {
                    _engine.Trace.Error("Error while executing module {0}: {1}", branch.Module.GetType().Name,
                        ex.Message);
                    _engine.Trace.Verbose(ex.ToString());
                    return;
                }
            }

            // Only once all outputs for the current module have been run should we descend to the next module
            foreach(PrepareBranch output in branch.Outputs)
            {
                Execute(output);
            }

            _engine.Trace.Verbose("Executed module {0} with {1} input(s).", branch.Module.GetType().Name, branch.Outputs.Count);
        }
    }
}
