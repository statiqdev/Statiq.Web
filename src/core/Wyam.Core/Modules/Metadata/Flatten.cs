using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Flattens a tree structure given child documents are stored in a given metadata key ("Children" by default).
    /// The flattened documents are returned in no particular order.
    /// </summary>
    /// <category>Metadata</category>
    public class Flatten : IModule
    {
        private readonly string _childrenKey = Keys.Children;

        public Flatten()
        {
        }

        public Flatten(string childrenKey)
        {
            _childrenKey = childrenKey;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>();
            foreach (IDocument root in inputs)
            {
                stack.Push(root);
            }
            while (stack.Count > 0)
            {
                IDocument current = stack.Pop();
                yield return current;
                IEnumerable<IDocument> children = current.DocumentList(_childrenKey);
                if (children != null)
                {
                    foreach (IDocument child in children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }
}