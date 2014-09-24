using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class Pipeline
    {
        private readonly List<IModule> _modules = new List<IModule>();

        public Pipeline(params IModule[] modules)
        {
            foreach(IModule module in modules)
            {
                Add(module);
            }
        }

        public void Add(IModule module)
        {
            _modules.Add(module);
        }

        internal PrepareTree Prepare(MetadataStack metadata)
        {
            PrepareBranch rootBranch = new PrepareBranch(new PipelineContext(metadata));
            List<PrepareBranch> lastBranches = new List<PrepareBranch>() 
            { 
                new PrepareBranch(null) 
                { 
                    Outputs = new List<PrepareBranch>() { rootBranch } 
                }
            };

            foreach(IModule module in _modules)
            {
                List<PrepareBranch> currentBranches = new List<PrepareBranch>();
                foreach (PrepareBranch lastBranch in lastBranches)
                {
                    foreach (PrepareBranch tree in lastBranch.Outputs)
                    {
                        tree.Module = module;
                        tree.Outputs = module.Prepare(tree.Input).Select(x => new PrepareBranch(x)).ToList();
                        currentBranches.AddRange(tree.Outputs);
                    }
                }
                lastBranches = currentBranches;
            }

            return new PrepareTree(rootBranch, lastBranches);
        }

        internal void Execute(PrepareBranch rootBranch)
        {
            
        }
    }
}
