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
    /// Adds metadata to the input documents that describes the position of each one in a tree structure. 
    /// </summary>
    /// <metadata name="Parent" type="IDocument">The parent of this node or <c>null</c> if it is a root.</metadata> 
    /// <metadata name="Children" type="ReadOnlyCollection&lt;IDocument&gt;">All the children of this node.</metadata> 
    /// <metadata name="PreviousSibling" type="IDocument">The previous sibling, that is the previous node in the children 
    /// collection of the parent or <c>null</c> if this is the first node in the collection or the parent is null.</metadata>
    /// <metadata name="NextSibling" type="IDocument">The next sibling, that is the next node in the children collection 
    /// of the parent or <c>null</c> if this is the last node in the collection or the parent is null.</metadata> 
    /// <metadata name="Next" type="IDocument">The next node in the tree using a depth-first 
    /// search or <c>null</c> if this was the last node.</metadata> 
    /// <metadata name="Previous" type="IDocument">The previous node in the tree using a depth-first 
    /// search or <c>null</c> if this was the first node.</metadata>
    public class Tree : IModule
    {
        private DocumentConfig _isRoot;
        private DocumentConfig _getPath;
        private DocumentConfig _getOrder;

        private string _parentKey = Keys.Parent;
        private string _childrenKey = Keys.Children;
        private string _previousSiblingKey = Keys.PreviousSibling;
        private string _nextSiblingKey = Keys.NextSibling;
        private string _nextKey = Keys.Next;
        private string _previousKey = Keys.Previous;


        public Tree()
        {
            _isRoot = (ctx, doc) => false;
            _getPath = (doc, ctx) => doc.Source.Segments;
        }

        /// <summary>
        /// Specifies for each document if it is a root of a tree. This results in splitting the generated tree into multiple smaller ones.
        /// </summary>
        /// <param name="config">A predicate (must return <c>bool</c>) that specifies if the current document is treated as the root of a new tree.</param>
        public Tree WithRoots(DocumentConfig config)
        {
            _isRoot = config;
            return this;
        }
        /// <summary>
        /// Specifies the order of the children in the tree.
        /// </summary>
        /// <param name="config">A predicate that must return an <c>int</c>.</param>
        public Tree WithOrder(DocumentConfig config)
        {
            _getOrder = config;
            return this;
        }

        /// <summary>
        /// Defines the structure of the tree.
        /// </summary>
        /// <param name="config">A predicate that must return a <c>string[]</c>.</param>
        public Tree WithPath(DocumentConfig config)
        {
            _getPath = config;
            return this;
        }

        /// <summary>
        /// Changes the standard metadata keys used by this module.
        /// </summary>
        public Tree WithMetadataNames(
            string parentKey = Keys.Parent, 
            string childrenKey = Keys.Children, 
            string previousSiblingKey = Keys.PreviousSibling, 
            string nextSiblingKey = Keys.NextSibling, 
            string nextKey = Keys.Next, 
            string previousKey = Keys.Previous)
        {
            _parentKey = parentKey;
            _childrenKey = childrenKey;
            _previousSiblingKey = previousSiblingKey;
            _nextSiblingKey = nextSiblingKey;
            _nextKey = nextKey;
            _previousKey = previousKey;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            Dictionary<string, TreeNode<IDocument>> treeElementLookup = new Dictionary<string, TreeNode<IDocument>>();

            Dictionary<IDocument, int> documentOrder = _getOrder != null
                ? inputs.ToDictionary(x => x, x => (int)_getOrder(x, context))
                : inputs.Select((value, index) => new { Index = index, Document = value }).ToDictionary(x => x.Document, x => x.Index);

            // Generate TreeNodes
            foreach (var keypair in inputs.Select(x => new { Key = x, Value = (string[])_getPath(x, context) }))
            {
                string[] splitedPath = keypair.Value;
                IDocument document = keypair.Key;

                for (int i = 1; i <= splitedPath.Length; i++)
                {
                    string id = System.IO.Path.Combine(splitedPath.Take(i).ToArray());
                    if (treeElementLookup.ContainsKey(id))
                    {
                        continue;
                    }
                    string parent = System.IO.Path.Combine(splitedPath.Take(i - 1).ToArray());
                    TreeNode<IDocument> treeNode = new TreeNode<IDocument>(id, parent);
                    if (i == splitedPath.Length)
                    {
                        treeNode.Value = document;
                    }
                    treeElementLookup.Add(id, treeNode);
                }
            }

            // Concatenating TreeNodes
            foreach (TreeNode<IDocument> treeElement in treeElementLookup.Values)
            {
                treeElement.Parent = treeElement.ParentId == ""
                    ? null
                    : treeElementLookup[treeElement.ParentId];
            }
            foreach (TreeNode<IDocument> treeElement in treeElementLookup.Values)
            {
                treeElement.Childrean = treeElementLookup.Values.Where(x => x.ParentId == treeElement.Id).ToList();
            }


            // Order Children
            foreach (TreeNode<IDocument> treeElement in treeElementLookup.Values)
            {
                treeElement.Childrean = treeElement.Childrean.OrderBy(x => x.Value != null ? documentOrder[x.Value] : -1).ToList();
            }

            // Split up Trees
            foreach (TreeNode<IDocument> treeElement in treeElementLookup.Values.Where(x => x.Value != null))
            {
                if ((bool)_isRoot(treeElement.Value, context))
                {
                    treeElement.Parent.Childrean.Remove(treeElement);
                    treeElement.Parent = null;
                }
            }

            // Adding Empty Documents
            foreach (TreeNode<IDocument> treeElement in treeElementLookup.Values.Where(x => x.Value == null))
            {
                treeElement.Value = context.GetDocument();
            }

            // Adding Metadata
            foreach (TreeNode<IDocument> treeElement in treeElementLookup.Values)
            {
                treeElement.Value = context.GetDocument(treeElement.Value, new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>(this._childrenKey,
                        new CachedDelegateMetadataValue((str, meta)=>
                            new ReadOnlyCollection<IDocument> (treeElement.Childrean.Select(x=>x.Value).ToArray())
                        )
                    ),
                    new KeyValuePair<string,object>(this._parentKey,
                        new CachedDelegateMetadataValue((str, meta)=>
                            treeElement.Parent?.Value
                        )
                    ),
                    new KeyValuePair<string,object>(this._nextSiblingKey,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetNextSilbling(treeElement)
                        )
                    ),
                    new KeyValuePair<string,object>(this._previousSiblingKey,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetPreviousSilbling(treeElement)
                        )
                    ),
                    new KeyValuePair<string,object>(this._nextKey,
                        new CachedDelegateMetadataValue((str, meta)=>
                            GetNextNodeMetadata(treeElement)
                        )
                    ),
                    new KeyValuePair<string,object>(this._previousKey,
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
            // Calculate the next node using a depth-first search
            if (treeElement.Childrean.Any())
            {
                return treeElement.Childrean[0].Value;
            }

            IDocument nextSilbling = GetNextSilbling(treeElement);
            if (nextSilbling != null)
            {
                return nextSilbling;
            }

            TreeNode<IDocument> currentElement = treeElement;

            while (currentElement.Parent != null)
            {
                IDocument parrentSilbling = GetNextSilbling(currentElement);
                if (parrentSilbling != null)
                {
                    return parrentSilbling;
                }
                currentElement = currentElement.Parent;
            }
            return null;
        }

        private TreeNode<IDocument> GetPreviousSilblingTree(TreeNode<IDocument> treeElement)
        {
            if (treeElement.Parent == null)
            {
                return null;
            }
            int indexInParent = treeElement.Parent.Childrean.IndexOf(treeElement);
            if (indexInParent == 0)
            {
                return null;
            }
            return treeElement.Parent.Childrean[indexInParent - 1];
        }

        private IDocument GetPreviousSilbling(TreeNode<IDocument> treeElement) => 
            GetPreviousSilblingTree(treeElement)?.Value;

        private IDocument GetNextSilbling(TreeNode<IDocument> treeElement)
        {
            if (treeElement.Parent == null)
            {
                return null;
            }
            int indexInParent = treeElement.Parent.Childrean.IndexOf(treeElement);
            if (indexInParent == treeElement.Parent.Childrean.Count - 1)
            {
                return null;
            }
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
