// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Linq;


namespace DigitalRune.Collections
{
    /// <summary>
    /// Describes a node in a tree, which can be merged with another tree.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Content"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// A collection (tree) of <see cref="MergeableNode{T}"/> instances can be merged into an
    /// existing collection. Each <see cref="MergeableNode{T}"/> describes where it should be
    /// merged.
    /// </para>
    /// <para>
    /// The <see cref="MergePoints"/> define where the node should be merged. The
    /// <see cref="MergeAlgorithm{T}"/> will enumerate the merge points and merge the node at the
    /// first valid merge point in the target collection. If no valid merge point can be found in
    /// the target collection, the <see cref="MergeableNode{T}"/> is ignored and not merged into the
    /// target collection.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mergeable")]
    public sealed class MergeableNode<T> where T : class, INamedObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public T Content { get; set; }


        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        /// <value>The parent of this node.</value>
        public MergeableNode<T> Parent { get; internal set; }


        /// <summary>
        /// Gets the children of this node.
        /// </summary>
        /// <value>The children of this node. The default value is <see langword="null"/>.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public MergeableNodeCollection<T> Children
        {
            get { return _children; }
            set
            {
                if (_children == value)
                    return;

                if (_children != null)
                    _children.Parent = null;

                _children = value;

                if (_children != null)
                    _children.Parent = this;
            }
        }
        private MergeableNodeCollection<T> _children;


        /// <summary>
        /// Gets the next sibling of this node.
        /// </summary>
        /// <value>The next node.</value>
        public MergeableNode<T> Next
        {
            get { return GetRelativeNodeInGroup(+1); }
        }


        /// <summary>
        /// Gets the previous sibling of this node.
        /// </summary>
        /// <value>The previous node.</value>
        public MergeableNode<T> Previous
        {
            get { return GetRelativeNodeInGroup(-1); }
        }


        /// <summary>
        /// Gets or sets the merge points.
        /// </summary>
        /// <value>
        /// The merge points. Per default, this property contains a single <see cref="MergePoint"/>
        /// with <see cref="MergeOperation.Append"/> and no target node.
        /// </value>
        public IEnumerable<MergePoint> MergePoints { get; set; } = MergePoint.DefaultMergePoints;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class.
        /// </summary>
        public MergeableNode()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class with the given 
        /// content.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null"/>.)</param>
        public MergeableNode(T content)
        {
            Content = content;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}" /> class with the given
        /// content.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null" />.)</param>
        /// <param name="mergePoint">The first merge point.</param>
        public MergeableNode(T content, MergePoint mergePoint)
            : this(content, new [] { mergePoint })
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}" /> class with the given
        /// content.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null" />.)</param>
        /// <param name="mergePoint0">The first merge point.</param>
        /// <param name="mergePoint1">The second merge point.</param>
        public MergeableNode(T content, MergePoint mergePoint0, MergePoint mergePoint1)
            : this(content, new[] { mergePoint0, mergePoint1 })
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}" /> class with the given
        /// content.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null" />.)</param>
        /// <param name="mergePoint0">The first merge point.</param>
        /// <param name="mergePoint1">The second merge point.</param>
        /// <param name="mergePoint2">The third merge point.</param>
        public MergeableNode(T content, MergePoint mergePoint0, MergePoint mergePoint1, MergePoint mergePoint2)
            : this(content, new[] { mergePoint0, mergePoint1, mergePoint2 })
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}" /> class with the given
        /// content.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null" />.)</param>
        /// <param name="mergePoints">The merge points.</param>
        public MergeableNode(T content, params MergePoint[] mergePoints)
        {
            Content = content;
            MergePoints = mergePoints;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class with the given 
        /// content and a set of child nodes.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null"/>.)</param>
        /// <param name="children">The children. (Can be <see langword="null"/>.)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public MergeableNode(T content, params MergeableNode<T>[] children)
          : this(content)
        {
            if (children != null)
            {
                Children = new MergeableNodeCollection<T>();
                Children.AddRange(children);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class with the given 
        /// content and a set of child nodes.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null"/>.)</param>
        /// <param name="mergePoint">The merge point.</param>
        /// <param name="children">The children. (Can be <see langword="null"/>.)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public MergeableNode(T content, MergePoint mergePoint, params MergeableNode<T>[] children)
          : this(content, new[] { mergePoint })
        {
            if (children != null)
            {
                Children = new MergeableNodeCollection<T>();
                Children.AddRange(children);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class with the given 
        /// content and a set of child nodes.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null"/>.)</param>
        /// <param name="mergePoint0">The first merge point.</param>
        /// <param name="mergePoint1">The second merge point.</param>
        /// <param name="children">The children. (Can be <see langword="null"/>.)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public MergeableNode(T content, MergePoint mergePoint0, MergePoint mergePoint1, params MergeableNode<T>[] children)
          : this(content, new[] { mergePoint0, mergePoint1 })
        {
            if (children != null)
            {
                Children = new MergeableNodeCollection<T>();
                Children.AddRange(children);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class with the given 
        /// content and a set of child nodes.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null"/>.)</param>
        /// <param name="mergePoint0">The first merge point.</param>
        /// <param name="mergePoint1">The second merge point.</param>
        /// <param name="mergePoint2">The third merge point.</param>
        /// <param name="children">The children. (Can be <see langword="null"/>.)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public MergeableNode(T content, MergePoint mergePoint0, MergePoint mergePoint1, MergePoint mergePoint2, params MergeableNode<T>[] children)
          : this(content, new[] { mergePoint0, mergePoint1, mergePoint2 })
        {
            if (children != null)
            {
                Children = new MergeableNodeCollection<T>();
                Children.AddRange(children);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNode{T}"/> class with the given 
        /// content and a set of child nodes.
        /// </summary>
        /// <param name="content">The content. (Can be <see langword="null"/>.)</param>
        /// <param name="mergePoints">The merge points.</param>
        /// <param name="children">The children. (Can be <see langword="null"/>.)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public MergeableNode(T content, MergePoint[] mergePoints, params MergeableNode<T>[] children)
          : this(content, mergePoints)
        {
            if (children != null)
            {
                Children = new MergeableNodeCollection<T>();
                Children.AddRange(children);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the relative node in group.
        /// </summary>
        /// <param name="offset">The offset relative to the current node (for example: -2).</param>
        /// <returns>
        /// The node relative to the current. Returns <see langword="null"/> if no node exists at the 
        /// specified positions or the current node is not a child of another 
        /// <see cref="MergeableNode{T}"/>.
        /// </returns>
        private MergeableNode<T> GetRelativeNodeInGroup(int offset)
        {
            if (offset == 0)
                return this;

            if (Parent == null)
                return null;

            var nodes = Parent.Children; // Siblings including self.
            Debug.Assert(nodes != null);
            int self = nodes.IndexOf(this);

            var index = self + offset;
            if (index < 0 || index >= nodes.Count)
                return null;

            return nodes[index];
        }
        #endregion


        //--------------------------------------------------------------
        #region Traversal Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the children of the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The children of <paramref name="node"/>.</returns>
        private static IEnumerable<MergeableNode<T>> GetChildren(MergeableNode<T> node)
        {
            return node.Children ?? Enumerable.Empty<MergeableNode<T>>();
        }


        /// <summary>
        /// Gets the root node.
        /// </summary>
        /// <returns>The root node.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public MergeableNode<T> GetRoot()
        {
            MergeableNode<T> node = this;
            while (node.Parent != null)
                node = node.Parent;

            return node;
        }


        /// <summary>
        /// Gets the ancestors of the given node.
        /// </summary>
        /// <returns>The ancestors of this node.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<MergeableNode<T>> GetAncestors()
        {
            return TreeHelper.GetAncestors(this, node => node.Parent);
        }


        /// <summary>
        /// Gets the given node and its ancestors.
        /// </summary>
        /// <returns>The current node and its ancestors.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<MergeableNode<T>> GetSelfAndAncestors()
        {
            return TreeHelper.GetSelfAndAncestors(this, node => node.Parent);
        }


        /// <overloads>
        /// <summary>
        /// Gets the descendants of the given scene node.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the descendants of the given node using a depth-first search.
        /// </summary>
        /// <returns>The descendants of this node in depth-first order.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<MergeableNode<T>> GetDescendants()
        {
            return TreeHelper.GetDescendants(this, GetChildren, true);
        }


        /// <summary>
        /// Gets the descendants of the given node using a depth-first or a breadth-first search.
        /// </summary>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The descendants of this node.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<MergeableNode<T>> GetDescendants(bool depthFirst)
        {
            return TreeHelper.GetDescendants(this, GetChildren, depthFirst);
        }


        /// <overloads>
        /// <summary>
        /// Gets the subtree (the given node and all of its descendants).
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the subtree (the given node and all of its descendants) using a depth-first 
        /// search.
        /// </summary>
        /// <returns>
        /// The subtree (the given node and all of its descendants) in depth-first order.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<MergeableNode<T>> GetSubtree()
        {
            return TreeHelper.GetSubtree(this, GetChildren, true);
        }


        /// <summary>
        /// Gets the subtree (the given node and all of its descendants) using a depth-first or a 
        /// breadth-first search.
        /// </summary>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The subtree (the given node and all of its descendants).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<MergeableNode<T>> GetSubtree(bool depthFirst)
        {
            return TreeHelper.GetSubtree(this, GetChildren, depthFirst);
        }
        #endregion

    }
}
