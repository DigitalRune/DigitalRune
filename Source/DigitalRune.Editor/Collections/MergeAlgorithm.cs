// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;


namespace DigitalRune.Collections
{
    /// <summary>
    /// Merges a collection of <see cref="MergeableNode{T}"/>s to another collection of 
    /// <see cref="MergeableNode{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the node <see cref="MergeableNode{T}.Content"/>.</typeparam>
    /// <remarks>
    /// The default merging algorithm merges all additional nodes (except for duplicates) into an
    /// existing collection of nodes. 
    /// </remarks>
    public class MergeAlgorithm<T> where T : class, INamedObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether merged nodes are copied or moved.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if nodes that are merged should be cloned (leaving the input
        /// collection intact); otherwise, <see langword="false"/> to move nodes (destroying the
        /// input collection). The default value is <see langword="false"/>.
        /// </value>
        public bool CloneNodesOnMerge { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Merges a collection of nodes to an existing collection.
        /// </summary>
        /// <param name="targetNodes">
        /// The existing collection into which the additional nodes are merged.
        /// </param>
        /// <param name="additionalNodes">
        /// The new nodes that shall be added to <paramref name="targetNodes"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="targetNodes"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="additionalNodes"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Cannot merge nodes. <see cref="MergeableNode{T}.Content"/> of a 
        /// <see cref="MergeableNode{T}"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="MergeException">
        /// Merge operations failed. Could not find merge points.
        /// </exception>
        public void Merge(MergeableNodeCollection<T> targetNodes, MergeableNodeCollection<T> additionalNodes)
        {
            if (targetNodes == null)
                throw new ArgumentNullException(nameof(targetNodes));
            if (additionalNodes == null)
                throw new ArgumentNullException(nameof(additionalNodes));

            foreach (var node in additionalNodes.ToArray())
                Merge(targetNodes, node);
        }


        /// <summary>
        /// Merges a single node into an existing collection.
        /// </summary>
        /// <param name="targetNodes">
        /// The existing collection into which the additional node is merged.
        /// </param>
        /// <param name="node">The node to be merged.</param>
        /// <exception cref="NotSupportedException">
        /// Cannot merge nodes. <paramref name="node"/>.<see cref="MergeableNode{T}.Content"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="MergeException">
        /// Merge operations failed. Could not find merge points.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void Merge(MergeableNodeCollection<T> targetNodes, MergeableNode<T> node)
        {
            if (node == null)
                return;

            if (node.Content == null)
                throw new NotSupportedException("Cannot merge nodes. MergeableNode must not be empty (Content != null).");

            if (node.MergePoints == null)
                return;

            foreach (var mergePoint in node.MergePoints)
            {
                string targetName = mergePoint.Target;
                var mergeOperation = mergePoint.Operation;

                if (mergeOperation == MergeOperation.Ignore)
                    return;   // Ignore this node.

                // Check whether node with the same action name already exists.
                int indexOfExistingNode = FindTargetNode(targetNodes, node.Content.Name);
                bool nodeExists = (indexOfExistingNode >= 0);

                int indexOfTarget = -1;
                bool targetFound = false;

                // Find target node.
                // (MergeOperation.Append and MergeOperation.Prepend do not need a target node.)
                if (!nodeExists && mergeOperation != MergeOperation.Append && mergeOperation != MergeOperation.Prepend)
                {
                    indexOfTarget = FindTargetNode(targetNodes, targetName);
                    targetFound = (indexOfTarget >= 0);

                    if (!targetFound)
                        continue;       // Target not found. Try next merge point.
                }

                switch (mergeOperation)
                {
                    case MergeOperation.Ignore:
                        // Do nothing. 
                        Debug.Fail("We should never get here, cause all MergeOperation.Ignore are handled above.");
                        break;

                    case MergeOperation.Match:
                        // Do not add this node, but merge children
                        Debug.Assert(nodeExists || targetFound, "Sanity check.");

                        if (nodeExists)
                        {
                            if (node.Children != null && node.Children.Count > 0)
                            {
                                var targetNode = targetNodes[indexOfExistingNode];
                                EnsureChildren(targetNode);
                                Merge(targetNode.Children, node.Children);
                            }
                        }
                        else
                        {
                            if (node.Children != null && node.Children.Count > 0)
                            {
                                var targetNode = targetNodes[indexOfTarget];
                                EnsureChildren(targetNode);
                                Merge(targetNode.Children, node.Children);
                            }
                        }
                        break;

                    case MergeOperation.Prepend:
                        // Merge node with existing node, or prepend at the beginning.
                        if (nodeExists)
                            OnMerge(targetNodes[indexOfExistingNode], node);
                        else
                            OnInsert(targetNodes, node, 0);
                        break;

                    case MergeOperation.Append:
                        // Merge node with existing node, or append at the end.
                        if (nodeExists)
                            OnMerge(targetNodes[indexOfExistingNode], node);
                        else
                            OnInsert(targetNodes, node, targetNodes.Count);
                        break;

                    case MergeOperation.InsertBefore:
                        // Merge node with existing, or insert before merge point.
                        Debug.Assert(nodeExists || targetFound, "Sanity check.");
                        if (nodeExists)
                            OnMerge(targetNodes[indexOfExistingNode], node);
                        else
                            OnInsert(targetNodes, node, indexOfTarget);
                        break;

                    case MergeOperation.InsertAfter:
                        // Merge node with existing or insert after merge point.
                        Debug.Assert(nodeExists || targetFound, "Sanity check.");
                        if (nodeExists)
                            OnMerge(targetNodes[indexOfExistingNode], node);
                        else
                            OnInsert(targetNodes, node, indexOfTarget + 1);
                        break;

                    case MergeOperation.Replace:
                        // Replace existing node (or node at merge point).
                        Debug.Assert(nodeExists || targetFound, "Sanity check.");
                        if (nodeExists)
                        {
                            targetNodes.RemoveAt(indexOfExistingNode);
                            OnInsert(targetNodes, node, indexOfExistingNode);
                        }
                        else
                        {
                            targetNodes.RemoveAt(indexOfTarget);
                            OnInsert(targetNodes, node, indexOfTarget);
                        }
                        break;

                    case MergeOperation.Remove:
                        // Remove existing node (or node at merge point)
                        Debug.Assert(nodeExists || targetFound, "Sanity check.");
                        if (nodeExists)
                            targetNodes.RemoveAt(indexOfExistingNode);
                        else
                            targetNodes.RemoveAt(indexOfTarget);
                        break;
                }
            }
        }


        private static void EnsureChildren(MergeableNode<T> node)
        {
            Debug.Assert(node != null);

            if (node.Children == null)
                node.Children = new MergeableNodeCollection<T>();
        }


        /// <summary>
        /// Finds the merge target node.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="target">The name of the target node.</param>
        /// <returns>
        /// The index of the target node. <c>-1</c> if the target node is not found.
        /// </returns>
        private static int FindTargetNode(MergeableNodeCollection<T> nodes, string target)
        {
            int index;
            var numberOfNodes = nodes.Count;
            for (index = 0; index < numberOfNodes; index++)
                if (nodes[index].Content.Name == target)
                    break;

            if (index == numberOfNodes)
            {
                // No match found.
                return -1;
            }

            return index;
        }


        /// <summary>
        /// Called when a node should be merged with an existing node.
        /// </summary>
        /// <param name="existingNode">
        /// The existing node to which <paramref name="node"/> shall be merged.
        /// </param>
        /// <param name="node">
        /// The additional node which is about to be merged to <paramref name="existingNode"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// Override this method to perform a custom merge operation.
        /// </para>
        /// <para>
        /// The base implementation of this method merges the children of <paramref name="node"/> 
        /// to the children of <paramref name="existingNode"/>.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected virtual void OnMerge(MergeableNode<T> existingNode, MergeableNode<T> node)
        {
            // Process children.
            if (node.Children != null && node.Children.Count > 0)
            {
                EnsureChildren(existingNode);
                Merge(existingNode.Children, node.Children);
            }
        }


        /// <summary>
        /// Called when a node should be inserted into a collection of nodes.
        /// </summary>
        /// <param name="existingNodes">
        /// The collection of nodes to which <paramref name="node"/> should be merged.
        /// </param>
        /// <param name="node">
        /// The additional node which is about to be merged to <paramref name="existingNodes"/>.
        /// </param>
        /// <param name="index">The index where the node should be inserted.</param>
        /// <remarks>
        /// <para>
        /// Override this method to perform additional perform a custom insert operation.
        /// </para>
        /// <para>
        /// The base implementation of this method detaches the <paramref name="node"/> from its 
        /// current parent and
        /// </para>
        /// <para>
        /// <strong>Notes to Inheritors: </strong>
        /// Do not call the base method (<c>base.InsertNode</c>) when overriding this method. The base
        /// implementation removes <paramref name="node"/> from its original collection and
        /// inserts it into <paramref name="existingNodes"/> at <paramref name="index"/>.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected virtual void OnInsert(MergeableNodeCollection<T> existingNodes, MergeableNode<T> node, int index)
        {
            if (!CloneNodesOnMerge)
            {
                // Moving nodes - destroying the input collection:
                node.Parent?.Children?.Remove(node);   // First detach node from parent.
                existingNodes.Insert(index, node);
            }
            else
            {
                // Copying nodes - leaving the input collection intact.
                var copy = new MergeableNode<T>(node.Content);

                existingNodes.Insert(index, copy);

                // Process children
                if (node.Children != null && node.Children.Count > 0)
                {
                    EnsureChildren(copy);
                    Merge(copy.Children, node.Children);
                }
            }
        }
        #endregion
    }
}
