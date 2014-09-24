using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class Pipeline
    {
        private readonly Engine _engine;
        private readonly List<IModule> _modules = new List<IModule>();

        internal Pipeline(Engine engine, params IModule[] modules)
        {
            _engine = engine;
            foreach(IModule module in modules)
            {
                Add(module);
            }
        }

        public void Add(IModule module)
        {
            _modules.Add(module);
        }

        internal PrepareTree Prepare(MetadataStack metadata, IEnumerable<dynamic> documents)
        {
            PrepareBranch rootBranch = new PrepareBranch(new PipelineContext(_engine, metadata, documents));
            List<PrepareBranch> lastBranches = new List<PrepareBranch>() 
            { 
                new PrepareBranch(null) 
                { 
                    Outputs = new List<PrepareBranch>() { rootBranch } 
                }
            };

            foreach(IModule module in _modules)
            {
                _engine.Trace.Verbose("Preparing module {0}...", module.GetType().Name);
                List<PrepareBranch> currentBranches = new List<PrepareBranch>();
                int i = 0;
                foreach (PrepareBranch lastBranch in lastBranches)
                {
                    foreach (PrepareBranch tree in lastBranch.Outputs)
                    {
                        i++;
                        tree.Module = module;
                        tree.Outputs = module.Prepare(tree.Input).Select(x => new PrepareBranch(x)).ToList();
                        currentBranches.AddRange(tree.Outputs);
                    }
                }
                lastBranches = currentBranches;
                _engine.Trace.Verbose("Prepared module {0} with {1} input(s) and {2} output(s).", module.GetType().Name, i, currentBranches.Count);
            }

            return new PrepareTree(rootBranch, lastBranches);
        }

        internal void Execute(PrepareBranch branch, string content = null)
        {
            _engine.Trace.Verbose("Executing module {0}...", branch.Module.GetType().Name);
            branch.Input.Unlock();  // Unlock the context before execution so that the module can add metadata during execution (I.e., excerpts, final content, etc.)
            content = branch.Module.Execute(branch.Input, content);
            foreach(PrepareBranch child in branch.Outputs)
            {
                Execute(child, content);
            }
            _engine.Trace.Verbose("Executed module {0} with {1} input(s).", branch.Module.GetType().Name, branch.Outputs.Count);
        }
    }
}
