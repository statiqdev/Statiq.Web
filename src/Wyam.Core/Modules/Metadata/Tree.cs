using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Add Metadata to the Documents that describes the position of
    /// the Document in a tree structure. 
    /// </summary>
    public class Tree : IModule
    {
        private DocumentConfig isRoot;
        private DocumentConfig getPath;
        private DocumentConfig getOrder;

        private string parentMetadata = "Parent";
        private string childrenMetadata = "Children";
        private string previousSilblingMetadata = "PreviosSilbling";
        private string nextSilblingMetadata = "NextSilbling";

        private string nextNodeMetadata = "Next";
        private string previousNodeMetadata = "Previous";


        public Tree()
        {
            isRoot = (ctx, doc) => false;
            getPath = (doc, ctx) => doc.Source.Segments;
        }

        /// <summary>
        /// Specifying for each Document if it is a root of a Tree. This Results in splitting the generated tree in multiple smaller ones.
        /// </summary>
        /// <param name="config">A Predicate (must return <c>bool</c>) that specifies if the current Document is treated as the root of a new tree.</param>
        /// <returns></returns>
        public Tree WithRoots(DocumentConfig config)
        {
            isRoot = config;
            return this;
        }
        /// <summary>
        /// Specifies the order of the Children in the Tree
        /// </summary>
        /// <param name="config">The DocumentConfig must return an <c>int</c>.</param>
        /// <returns></returns>
        public Tree WithOrder(DocumentConfig config)
        {
            getOrder = config;
            return this;
        }

        /// <summary>
        /// Defines the structure of the Tree
        /// </summary>
        /// <param name="config">Must return <c>string[]</c></param>
        /// <returns></returns>
        public Tree WithPath(DocumentConfig config)
        {
            getPath = config;
            return this;
        }




        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {


            var treeElementLookup = new Dictionary<string, TreeNode<IDocument>>();

            var documentOrder = getOrder != null
                ? inputs.ToDictionary(x => x, x => (int)getOrder(x, context))
                : inputs.Select((value, index) => new { Index = index, Document = value }).ToDictionary(x => x.Document, x => x.Index);

            // Generate TreeNodes
            foreach (var keypair in inputs.Select(x => new { Key = x, Value = (string[])getPath(x, context) }))
            {
                var splitedPath = keypair.Value;
                var document = keypair.Key;

                for (int i = 1; i <= splitedPath.Length; i++)
                {
                    string id = System.IO.Path.Combine(splitedPath.Take(i).ToArray());
                    if (treeElementLookup.ContainsKey(id))
                        continue;
                    string parent = System.IO.Path.Combine(splitedPath.Take(i - 1).ToArray());
                    var treeNode = new TreeNode<IDocument>(id, parent);
                    if (i == splitedPath.Length)
                        treeNode.Value = document;
                    treeElementLookup.Add(id, treeNode);
                }
            }

            // Concatenating TreeNodes
            foreach (var treeElement in treeElementLookup.Values)
            {
                treeElement.Parent = treeElement.ParentId == ""
                    ? null
                    : treeElementLookup[treeElement.ParentId];
            }
            foreach (var treeElement in treeElementLookup.Values)
            {
                treeElement.Childrean = treeElementLookup.Values.Where(x => x.ParentId == treeElement.Id).ToList();
            }


            // Order Children
            foreach (var treeElement in treeElementLookup.Values)
            {
                treeElement.Childrean = treeElement.Childrean.OrderBy(x => x.Value != null ? documentOrder[x.Value] : -1).ToList();
            }

            // Split up Trees
            foreach (var treeElement in treeElementLookup.Values.Where(x => x.Value != null))
            {
                if ((bool)isRoot(treeElement.Value, context))
                {
                    treeElement.Parent.Childrean.Remove(treeElement);
                    treeElement.Parent = null;
                }
            }

            // Adding Empty Documents
            foreach (var treeElement in treeElementLookup.Values.Where(x => x.Value == null))
            {
                treeElement.Value = context.GetDocument();
            }

            // Adding Metadata
            foreach (var treeElement in treeElementLookup.Values)
            {
                treeElement.Value = context.GetDocument(treeElement.Value, new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>(this.childrenMetadata,
                        new CachedDelegateMetadataValue((str, meta)=>
                            new ReadOnlyCollection<IDocument> (treeElement.Childrean.Select(x=>x.Value).ToArray())
                        )
                    ),
                    new KeyValuePair<string,object>(this.parentMetadata,
                        new CachedDelegateMetadataValue((str, meta)=>
                            treeElement.Parent?.Value
                        )
                    ),
                    new KeyValuePair<string,object>(this.nextSilblingMetadata,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetNextSilbling(treeElement)
                        )
                    ),
                    new KeyValuePair<string,object>(this.previousSilblingMetadata,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetPreviousSilbling(treeElement)
                        )
                    ),
                    new KeyValuePair<string,object>(this.nextNodeMetadata,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetNextNodeMetadata(treeElement)
                        )
                    ),
                    new KeyValuePair<string,object>(this.previousNodeMetadata,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetPreviousNode(treeElement)
                        )
                    )
                });
            }

            return treeElementLookup.Values.Select(x => x.Value);
        }

        private IDocument GetPreviousNode(TreeNode<IDocument> treeElement)
        {
            TreeNode<IDocument> previoussilbling = GetPreviousSilblingTree(treeElement);
            while (previoussilbling != null && previoussilbling.Childrean.Any())
            {
                previoussilbling = previoussilbling.Childrean.Last();
            }
            if (previoussilbling != null)
            {
                return previoussilbling.Value;
            }

            return treeElement.Parent?.Value;
        }

        private IDocument GetNextNodeMetadata(TreeNode<IDocument> treeElement)
        {
            // Calculate the next node using Depth-first search
            if (treeElement.Childrean.Any())
                return treeElement.Childrean[0].Value;

            var nextSilbling = GetNextSilbling(treeElement);
            if (nextSilbling != null)
                return nextSilbling;

            var currentElement = treeElement;

            while (currentElement.Parent != null)
            {
                var parrentSilbling = GetNextSilbling(currentElement);
                if (parrentSilbling != null)
                    return parrentSilbling;
                currentElement = currentElement.Parent;
            }
            return null;
        }

        private TreeNode<IDocument> GetPreviousSilblingTree(TreeNode<IDocument> treeElement)
        {
            if (treeElement.Parent == null)
                return null;
            var indexInParent = treeElement.Parent.Childrean.IndexOf(treeElement);
            if (indexInParent == 0)
                return null;
            return treeElement.Parent.Childrean[indexInParent - 1];
        }

        private IDocument GetPreviousSilbling(TreeNode<IDocument> treeElement)
        => GetPreviousSilblingTree(treeElement)?.Value;

        private IDocument GetNextSilbling(TreeNode<IDocument> treeElement)
        {
            if (treeElement.Parent == null)
                return null;
            var indexInParent = treeElement.Parent.Childrean.IndexOf(treeElement);
            if (indexInParent == treeElement.Parent.Childrean.Count - 1)
                return null;
            return treeElement.Parent.Childrean[indexInParent + 1].Value;
        }

        [System.Diagnostics.DebuggerDisplay("TreeElement {Id}")]
        private class TreeNode<T>
        {
            public TreeNode(string id, string parentId)
            {
                Id = id;
                ParentId = parentId;
            }
            public string Id { get; }
            public string ParentId { get; }
            public T Value { get; set; }
            public TreeNode<T> Parent { get; set; }
            public List<TreeNode<T>> Childrean { get; set; }
        }
    }
}
