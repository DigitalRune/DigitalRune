// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Collections
{
    /// <summary>
    /// Manages a collection of <see cref="MergeableNode{T}"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of content.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public sealed class MergeableNodeCollection<T> : ChildCollection<MergeableNode<T>, MergeableNode<T>> where T : class, INamedObject
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNodeCollection{T}"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeableNodeCollection{T}"/> class
        /// which has no owner.
        /// </summary>
        public MergeableNodeCollection()
            : base(null)
        {
        }


        /// <summary>
        /// Gets or sets the parent which owns this child collection.
        /// </summary>
        /// <value>The parent.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        internal new MergeableNode<T> Parent
        {
            get { return base.Parent; }
            set
            {
                if (base.Parent == value)
                    return;

                if (base.Parent != null)
                    throw new InvalidOperationException("Cannot assign MergeableNodeCollection<T> to MergeableNode<T>. The specified collection is already owned by another node.");

                base.Parent = value;
            }
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override MergeableNode<T> GetParent(MergeableNode<T> child)
        {
            return child.Parent;
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void SetParent(MergeableNode<T> child, MergeableNode<T> parent)
        {
            child.Parent = parent;
        }
    }
}
