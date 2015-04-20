using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class Pipeline
    {
        private readonly Engine _engine;
        private readonly List<IModule> _modules = new List<IModule>();

        public Pipeline(Engine engine)
        {
            _engine = engine;
        }

        public void Add(IModule module)
        {
            _modules.Add(module);
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

            foreach(IModule module in _modules)
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
                            currentBranch.Outputs = module.Prepare(currentBranch.Input).Select(x => new PrepareBranch(x)).ToList();
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

        public void Execute(PrepareBranch branch, string content = null)
        {
            if(branch.Module == null)
            {
                // This is a leaf, so no further recursion required
                return;
            }

            _engine.Trace.Verbose("Executing module {0}...", branch.Module.GetType().Name);
            try
            {
                content = branch.Module.Execute(branch.Input, content);
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Error while executing module {0}: {1}", branch.Module.GetType().Name, ex.Message);
                _engine.Trace.Verbose(ex.ToString());
                return;
            }
            foreach(PrepareBranch child in branch.Outputs)
            {
                Execute(child, content);
            }
            _engine.Trace.Verbose("Executed module {0} with {1} input(s).", branch.Module.GetType().Name, branch.Outputs.Count);
        }
    }
}
