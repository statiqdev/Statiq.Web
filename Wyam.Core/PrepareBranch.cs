using System.Collections.Generic;

namespace Wyam.Core
{
    internal class PrepareBranch
    {
        public IModule Module { get; set; }

        public PipelineContext Input { get; set; }

        public IList<PrepareBranch> Outputs { get; set; }

        public PrepareBranch(PipelineContext input)
        {
            input.Lock();   // Lock the context once it's added to the tree
            Input = input;
            Outputs = new List<PrepareBranch>();
        }
    }
}