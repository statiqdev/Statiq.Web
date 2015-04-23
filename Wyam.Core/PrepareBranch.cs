using System.Collections.Generic;

namespace Wyam.Core
{
    internal class PrepareBranch
    {
        public Module Module { get; set; }

        public IModuleContext Context { get; set; }

        public IList<PrepareBranch> Outputs { get; set; }

        public string Content { get; set; }

        public PrepareBranch(IModuleContext context)
        {
            Context = context;
            Outputs = new List<PrepareBranch>();
        }
    }
}