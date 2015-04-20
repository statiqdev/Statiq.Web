using System.Collections.Generic;

namespace Wyam.Core
{
    internal class PrepareBranch
    {
        public IModule Module { get; set; }

        public IPipelineContext Input { get; set; }

        public IList<PrepareBranch> Outputs { get; set; }

        public PrepareBranch(IPipelineContext input)
        {
            Input = input;
            Outputs = new List<PrepareBranch>();
        }
    }
}