using System.Collections.Generic;

namespace Wyam.Core
{
    internal class PrepareTree
    {
        public PrepareBranch RootBranch { get; private set; }

        public IEnumerable<PrepareBranch> Leaves { get; private set; }// The final leaves are used for populating the aggregate document list

        public PrepareTree(PrepareBranch rootBranch, IEnumerable<PrepareBranch> leaves)
        {
            RootBranch = rootBranch;
            Leaves = leaves;
        }
    }
}