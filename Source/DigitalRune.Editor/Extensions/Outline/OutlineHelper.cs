// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Linq;


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Provides helper methods for outlines.
    /// </summary>
    public static class OutlineHelper
    {
        //--------------------------------------------------------------
        #region LINQ to Outline tree
        //--------------------------------------------------------------

        private static readonly Func<OutlineItem, OutlineItem> GetParentCallback = item => item.Parent;
        private static readonly Func<OutlineItem, IEnumerable<OutlineItem>> GetChildrenCallback = GetChildren;


        /// <summary>
        /// Gets the children of the given outline item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The children of the given item or an empty <see cref="IEnumerable{T}"/> if 
        /// <paramref name="item"/> or <see cref="OutlineItem.Children"/> is <see langword="null"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IEnumerable<OutlineItem> GetChildren(this OutlineItem item)
        {
            if (item?.Children == null)
                return LinqHelper.Empty<OutlineItem>();

            return item.Children;
        }


        /// <summary>
        /// Gets the root item.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <returns>The root item.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static OutlineItem GetRoot(this OutlineItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            while (item.Parent != null)
                item = item.Parent;

            return item;
        }


        /// <summary>
        /// Gets the ancestors of the given outline item.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <returns>The ancestors of this outline item.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetAncestors(this OutlineItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetAncestors(item, GetParentCallback);
        }


        /// <summary>
        /// Gets the outline item and its ancestors scene.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <returns>The <paramref name="item"/> and its ancestors of the scene.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetSelfAndAncestors(this OutlineItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetSelfAndAncestors(item, GetParentCallback);
        }


        /// <overloads>
        /// <summary>
        /// Gets the descendants of the given outline item.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the descendants of the given outline item using a depth-first search.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <returns>The descendants of this outline item in depth-first order.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetDescendants(this OutlineItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetDescendants(item, GetChildrenCallback, true);
        }


        /// <summary>
        /// Gets the descendants of the given outline item using a depth-first or a breadth-first search.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The descendants of this outline item.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetDescendants(this OutlineItem item, bool depthFirst)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetDescendants(item, GetChildrenCallback, depthFirst);
        }


        /// <overloads>
        /// <summary>
        /// Gets the subtree (the given outline item and all of its descendants).
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the subtree (the given outline item and all of its descendants) using a depth-first 
        /// search.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <returns>
        /// The subtree (the given outline item and all of its descendants) in depth-first order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetSubtree(this OutlineItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetSubtree(item, GetChildrenCallback, true);
        }


        /// <summary>
        /// Gets the subtree (the given outline item and all of its descendants) using a depth-first or a 
        /// breadth-first search.
        /// </summary>
        /// <param name="item">The outline item.</param>
        /// <param name="depthFirst">
        /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
        /// otherwise a breadth-first search will be made.
        /// </param>
        /// <returns>The subtree (the given outline item and all of its descendants).</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetSubtree(this OutlineItem item, bool depthFirst)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetSubtree(item, GetChildren, depthFirst);
        }


        /// <summary>
        /// Gets the leaves of the outline item.
        /// </summary>
        /// <param name="item">The outline item where to start the search.</param>
        /// <returns>The leaves of the outline item.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<OutlineItem> GetLeaves(this OutlineItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return TreeHelper.GetLeaves(item, GetChildren);
        }
        #endregion
    }
}
