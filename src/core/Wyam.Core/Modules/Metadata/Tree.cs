using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Adds metadata to the input documents that describes the position of each one in a tree structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, this module is configured to generate a tree that mimics the directory structure of each document's input path
    /// by looking at it's RelativeFilePath metadata value. Any documents with a file name of "index.*" are automatically
    /// promoted to the node that represents the parent folder level. For any folder that does not contain an "index.*" file,
    /// an empty placeholder tree node is used to represent the folder.
    /// </para>
    /// <para>
    /// Note that if you clone documents from the tree, the relationships of the cloned document (parent, child, etc.)
    /// will not be updated to the new clones. In other words, your new document will still be pointing to the old
    /// versions of it's parent, children, etc. To update the tree after cloning documents you will need to recreate it
    /// by rerunning this module on all the newly created documents again.
    /// </para>
    /// </remarks>
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
    /// <metadata name="TreePath" type="object[]">The path that represents this node in the tree.</metadata>
    /// <category>Metadata</category>
    public class Tree : IModule
    {
        private DocumentConfig _isRoot;
        private DocumentConfig _treePath;
        private Func<object[], MetadataItems, IExecutionContext, IDocument> _placeholderFactory;
        private Comparison<IDocument> _sort;
        private bool _collapseRoot = false;

        private string _parentKey = Keys.Parent;
        private string _childrenKey = Keys.Children;
        private string _previousSiblingKey = Keys.PreviousSibling;
        private string _nextSiblingKey = Keys.NextSibling;
        private string _previousKey = Keys.Previous;
        private string _nextKey = Keys.Next;
        private string _treePathKey = Keys.TreePath;

        public Tree()
        {
            _isRoot = (doc, ctx) => false;
            _treePath = (doc, ctx) =>
            {
                // Attempt to get the segments first from RelativeFilePath and then from Source
                List<string> segments = doc.FilePath(Keys.RelativeFilePath)?.Segments.ToList();
                if (segments == null)
                {
                    return null;
                }

                // Promote "index." pages up a level
                if (segments.Count > 0 && segments[segments.Count - 1].StartsWith("index."))
                {
                    segments.RemoveAt(segments.Count - 1);
                }
                return segments;
            };
            _placeholderFactory = (treePath, items, context) =>
            {
                items.Add(new MetadataItem(Keys.RelativeFilePath,
                    new FilePath(string.Join("/", treePath.Concat(new[] {"index.html"})))));
                return context.GetDocument(items);
            };
            _sort = (x, y) => Comparer.Default.Compare(
                x.Get<object[]>(Keys.TreePath)?.LastOrDefault(),
                y.Get<object[]>(Keys.TreePath)?.LastOrDefault());
        }

        /// <summary>
        /// Allows you to specify a factory function for the creation of placeholder documents which get
        /// created to represent nodes in the tree for which there was no input document. The factory
        /// gets passed the current tree path, the set of tree metadata that should be set in the document,
        /// and the execution context which can be used to create a new document. If the factory function
        /// returns null, a new document with the tree metadata is created.
        /// </summary>
        /// <param name="factory">The factory function.</param>
        public Tree WithPlaceholderFactory(Func<object[], MetadataItems, IExecutionContext, IDocument> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _placeholderFactory = factory;
            return this;
        }

        /// <summary>
        /// This specifies how the children of a given tree node should be sorted. The default behavior is to
        /// sort based on the string value of the last component of the child node's tree path (I.e., the folder
        /// or file name). The output document for each tree node is used as the input to the sort delegate.
        /// </summary>
        /// <param name="sort">A comparison delegate.</param>
        public Tree WithSort(Comparison<IDocument> sort)
        {
            if (sort == null)
            {
                throw new ArgumentNullException(nameof(sort));
            }

            _sort = sort;
            return this;
        }

        /// <summary>
        /// Specifies for each document if it is a root of a tree. This results in splitting the generated tree into multiple smaller ones,
        /// removing the root node from the set of children of it's parent and setting it's parent to <c>null</c>.
        /// </summary>
        /// <param name="isRoot">A predicate (must return <c>bool</c>) that specifies if the current document is treated as the root of a new tree.</param>
        public Tree WithRoots(DocumentConfig isRoot)
        {
            if (isRoot == null)
            {
                throw new ArgumentNullException(nameof(isRoot));
            }

            _isRoot = isRoot;
            return this;
        }

        /// <summary>
        /// Defines the structure of the tree. If the delegate returns <c>null</c> the document
        /// is excluded from the tree.
        /// </summary>
        /// <param name="treePath">A delegate that must return a sequence of objects.</param>
        public Tree WithTreePath(DocumentConfig treePath)
        {
            if (treePath == null)
            {
                throw new ArgumentNullException(nameof(treePath));
            }

            _treePath = treePath;
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
            string previousKey = Keys.Previous,
            string nextKey = Keys.Next,
            string treePathKey = Keys.TreePath)
        {
            _parentKey = parentKey;
            _childrenKey = childrenKey;
            _previousSiblingKey = previousSiblingKey;
            _nextSiblingKey = nextSiblingKey;
            _previousKey = previousKey;
            _nextKey = nextKey;
            _treePathKey = treePathKey;
            return this;
        }

        /// <summary>
        /// Indicates that the root of the tree should be collapsed and the module
        /// should output first-level documents as if they were root documents.
        /// </summary>
        /// <param name="collapseRoot"><c>tree</c> if the root should be collapsed.</param>
        public Tree CollapseRoot(bool collapseRoot = true)
        {
            _collapseRoot = collapseRoot;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Create a dictionary of tree nodes
            Dictionary<object[], TreeNode> nodes = inputs
                .AsParallel()
                .Select(x => new TreeNode(this, x, context))
                .Where(x => x.TreePath != null)
                .ToDictionary(x => x.TreePath, new TreePathEqualityComparer());

            // Add links between parent and children (creating empty tree nodes as needed)
            Queue<TreeNode> nodesToProcess = new Queue<TreeNode>(nodes.Values);
            while(nodesToProcess.Count > 0)
            {
                TreeNode node = nodesToProcess.Dequeue();

                // Skip root nodes
                if (node.TreePath.Length == 0 
                    || (node.InputDocument != null && _isRoot.Invoke<bool>(node.InputDocument, context)))
                {
                    continue;
                }

                // Skip the root node if collapsing the root
                object[] parentTreePath = node.GetParentTreePath();
                if (parentTreePath.Length == 0 && _collapseRoot)
                {
                    continue;
                }

                // Find (or create) the parent
                TreeNode parent;
                if (!nodes.TryGetValue(parentTreePath, out parent))
                {
                    parent = new TreeNode(parentTreePath);
                    nodes.Add(parentTreePath, parent);
                    nodesToProcess.Enqueue(parent);
                }

                // Add the parent and child relationship
                node.Parent = parent;
                parent.Children.Add(node);
            }

            // Return root nodes, recursively generating their child output documents
            foreach (TreeNode node in nodes.Values)
            {
                if (node.Parent == null)
                {
                    node.GenerateOutputDocuments(this, context);
                    yield return node.OutputDocument;
                }
            }
        }

        private class TreeNode
        {
            public object[] TreePath { get; }
            public IDocument InputDocument { get; }
            public IDocument OutputDocument { get; private set; }
            public TreeNode Parent { get; set; }
            public List<TreeNode> Children { get; } = new List<TreeNode>();

            // New placeholder node
            public TreeNode(object[] treePath)
            {
                if (treePath == null)
                {
                    throw new ArgumentNullException(nameof(treePath));
                }

                TreePath = treePath;
            }

            // New node from an input document
            public TreeNode(Tree tree, IDocument inputDocument, IExecutionContext context)
            {
                TreePath = tree._treePath.Invoke<object[]>(inputDocument, context);
                InputDocument = inputDocument;
            }

            // We need to build the tree from the bottom up so that the children don't have to be lazy
            // This also sorts the children once they're created
            public void GenerateOutputDocuments(Tree tree, IExecutionContext context)
            {
                // Recursively build output documents for children
                foreach (TreeNode child in Children)
                {
                    child.GenerateOutputDocuments(tree, context);
                }

                // We're done if we've already created the output document
                if (OutputDocument != null)
                {
                    return;
                }

                // Sort the child documents since they're created now
                Children.Sort((x, y) => tree._sort(x.OutputDocument, y.OutputDocument));

                // Create this output document
                MetadataItems metadata = new MetadataItems
                {
                    new MetadataItem(tree._childrenKey, 
                        new ReadOnlyCollection<IDocument> (Children.Select(x => x.OutputDocument).ToArray())),
                    new MetadataItem(tree._parentKey,
                        new CachedDelegateMetadataValue(_ => Parent?.OutputDocument)),
                    new MetadataItem(tree._previousSiblingKey,
                        new CachedDelegateMetadataValue(_ => GetPreviousSibling()?.OutputDocument)),
                    new MetadataItem(tree._nextSiblingKey,
                        new CachedDelegateMetadataValue(_ => GetNextSibling()?.OutputDocument)),
                    new MetadataItem(tree._previousKey,
                        new CachedDelegateMetadataValue(_ => GetPrevious()?.OutputDocument)),
                    new MetadataItem(tree._nextKey,
                        new CachedDelegateMetadataValue(_ => GetNext()?.OutputDocument)),
                    new MetadataItem(tree._treePathKey, TreePath)
                };
                OutputDocument = InputDocument == null
                    ? (tree._placeholderFactory(TreePath, metadata, context) ?? context.GetDocument(metadata))
                    : context.GetDocument(InputDocument, metadata);
            }

            public object[] GetParentTreePath() => TreePath.Take(TreePath.Length - 1).ToArray();

            private TreeNode GetPreviousSibling() =>
                Parent?.Children.AsEnumerable().Reverse().SkipWhile(x => x != this).Skip(1).FirstOrDefault();

            private TreeNode GetNextSibling() =>
                Parent?.Children.SkipWhile(x => x != this).Skip(1).FirstOrDefault();

            private TreeNode GetPrevious()
            {
                TreeNode previousSibling = GetPreviousSibling();
                while (previousSibling != null && previousSibling.Children.Count > 0)
                {
                    previousSibling = previousSibling.Children.Last();
                }
                return previousSibling ?? Parent;
            }

            private TreeNode GetNext()
            {
                if (Children.Count > 0)
                {
                    return Children.First();
                }

                TreeNode nextSibling = GetNextSibling();
                if (nextSibling != null)
                {
                    return nextSibling;
                }

                TreeNode current = Parent;
                while (current?.Parent != null)
                {
                    nextSibling = current.GetNextSibling();
                    if (nextSibling != null)
                    {
                        return nextSibling;
                    }
                    current = current.Parent;
                }
                return null;
            }
        }

        private class TreePathEqualityComparer : IEqualityComparer<object[]>
        {
            public bool Equals(object[] x, object[] y) => x.SequenceEqual(y);

            public int GetHashCode(object[] obj) =>
                obj?.Aggregate(17, (index, x) => index * 23 + (x?.GetHashCode() ?? 0)) ?? 0;
        }
    }
}
