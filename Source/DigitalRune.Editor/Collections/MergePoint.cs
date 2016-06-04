// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Collections
{
    /// <summary>
    /// Describes where a <see cref="MergeableNode{T}"/> should be merged into a
    /// <see cref="MergeableNodeCollection{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="MergePoint"/> defines the name of a target <see cref="MergeableNode{T}"/>
    /// (property <see cref="Target"/>). The node which should be merged will be placed relative to
    /// this target node. A merge operation (property <see cref="Operation"/>) defines where the
    /// node should be merged, for example:
    /// </para>
    /// <list type="bullet">
    /// <item>Should it be added before the target?</item>
    /// <item>Should it be added after the target?</item>
    /// <item>Should it replace the target?</item>
    /// <item>Etc.</item>
    /// </list>
    /// </remarks>
    public struct MergePoint : IEquatable<MergePoint>
    {
        /// <summary>
        /// A merge point with the <see cref="Operation"/> <see cref="MergeOperation.Append"/>.
        /// </summary>
        public static readonly MergePoint Append = new MergePoint(MergeOperation.Append, null);


        /// <summary>
        /// A merge point with the <see cref="Operation"/> <see cref="MergeOperation.Prepend"/>.
        /// </summary>
        public static readonly MergePoint Prepend = new MergePoint(MergeOperation.Prepend, null);


        internal static readonly MergePoint[] DefaultMergePoints = { Append };


        /// <summary>
        /// Gets or sets the merge operation (e.g. prepend, append, replace).
        /// </summary>
        /// <value>The merge operation (e.g. prepend, append, replace).</value>
        public MergeOperation Operation { get; }


        /// <summary>
        /// Gets or sets the name of the target node. 
        /// </summary>
        /// <value>The name of the target node. The default value is <see langword="null"/>.</value>
        public string Target { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergePoint" /> struct.
        /// </summary>
        /// <param name="operation">The operation.</param>
        public MergePoint(MergeOperation operation)
        {
            Operation = operation;
            Target = null;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MergePoint" /> struct.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="target">The target. Can be <see langword="null"/>.</param>
        public MergePoint(MergeOperation operation, string target)
        {
            Operation = operation;
            Target = target;
        }


        /// <summary>
        /// Determines whether the specified <see cref="MergePoint"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="MergePoint"/> to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="MergePoint"/> is equal to this
        /// instance; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(MergePoint other)
        {
            return Operation == other.Operation && string.Equals(Target, other.Target);
        }


        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this
        /// instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is MergePoint && Equals((MergePoint)obj);
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Operation * 397) ^ (Target?.GetHashCode() ?? 0);
            }
        }


        /// <summary>
        /// Compares two <see cref="MergePoint"/>s to determine whether they are the same.
        /// </summary>
        /// <param name="left">The first <see cref="MergePoint"/>.</param>
        /// <param name="right">The second <see cref="MergePoint"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
        /// same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(MergePoint left, MergePoint right)
        {
            return left.Equals(right);
        }


        /// <summary>
        /// Compares two <see cref="MergePoint"/>s to determine whether they are different.
        /// </summary>
        /// <param name="left">The first <see cref="MergePoint"/>.</param>
        /// <param name="right">The second <see cref="MergePoint"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
        /// different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(MergePoint left, MergePoint right)
        {
            return !left.Equals(right);
        }
    }
}
