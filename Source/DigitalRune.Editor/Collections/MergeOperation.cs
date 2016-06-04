// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Collections
{
    /// <summary>
    /// Describes how a node should be merged into collection of nodes. Some operations are relative
    /// to a node ("target node") that is in the collection.
    /// </summary>
    public enum MergeOperation
    {
        /// <summary>
        /// Ignores the current node. (Does nothing.)
        /// </summary>
        Ignore,

        /// <summary>
        /// Searches for the given target node and merges the children of the nodes.
        /// </summary>
        Match,

        /// <summary>
        /// Inserts the node at the beginning of the current level. (Ignores the target node.)
        /// </summary>
        Prepend,

        /// <summary>
        /// Adds the node at the end of the current level. (Ignores the target node.)
        /// </summary>
        Append,

        /// <summary>
        /// Inserts the node before the target node.
        /// </summary>
        InsertBefore,

        /// <summary>
        /// Inserts the node after the target node.
        /// </summary>
        InsertAfter,

        /// <summary>
        /// Replaces the target node with the node.
        /// </summary>
        Replace,

        /// <summary>
        /// Removes the target node.
        /// </summary>
        Remove
    }
}
