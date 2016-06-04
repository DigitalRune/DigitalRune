// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace DigitalRune.Geometry.Partitioning
{
  partial class CompressedAabbTree
  {
    /// <summary>
    /// Represents a node of an <see cref="CompressedAabbTree"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum and maximum of the AABB are stored as quantized 16-bit integer values.
    /// </para>
    /// <para>
    /// The <see cref="CompressedAabbTree"/> supports a stackless, non-recursive traversal of the 
    /// tree. The tree is traversed in pre-order traversal order. The nodes are stored in the order 
    /// as they are visited in the traversal: The left child follows the parent node. The right 
    /// child follows after the left subtree. If a node is a leaf then the right sibling directly 
    /// follows. The leaf nodes store the actual items of the AABB tree. Each non-leaf stores an 
    /// 'escape offset'. This offset points to the node that follows in the traversal if we stop
    /// traversing a left subtree and continue with its right subtree. By using an escape offset we 
    /// do not need to store the root of the right subtree on a stack (implicitly or explicitly).
    /// </para>
    /// <para>
    /// The field <see cref="EscapeOffsetOrItem"/> is negative if the node is an internal node and 
    /// the field contains an escape offset (<c>escapeOffset = -node.EscapeOffsetOrItem</c>). The 
    /// value is positive if the node is a leaf and contains an item index.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Node
    {
      /// <summary>
      /// The quantized minimum of the AABB (x component).
      /// </summary>
      [FieldOffset(0)]
      public ushort MinimumX;


      /// <summary>
      /// The quantized minimum of the AABB (y component).
      /// </summary>
      [FieldOffset(2)]
      public ushort MinimumY;


      /// <summary>
      /// The quantized minimum of the AABB (z component).
      /// </summary>
      [FieldOffset(4)]
      public ushort MinimumZ;


      /// <summary>
      /// The quantized maximum of the AABB (x component).
      /// </summary>
      [FieldOffset(6)]
      public ushort MaximumX;


      /// <summary>
      /// The quantized maximum of the AABB (y component).
      /// </summary>
      [FieldOffset(8)]
      public ushort MaximumY;


      /// <summary>
      /// The quantized maximum of the AABB (z component).
      /// </summary>
      [FieldOffset(10)]
      public ushort MaximumZ;


      /// <summary>
      /// The escape offset (if negative) or the item index.
      /// </summary>
      [FieldOffset(12)]
      internal int EscapeOffsetOrItem;


      /// <summary>
      /// Gets a value indicating whether this instance is leaf node.
      /// </summary>
      /// <value>
      /// <see langword="true"/> if this instance is leaf; otherwise, <see langword="false"/> if it
      /// is an internal node.
      /// </value>
      public bool IsLeaf
      {
        get { return EscapeOffsetOrItem >= 0; }
      }


      /// <summary>
      /// Gets or sets the data held in this node.
      /// </summary>
      /// <value>The data of this node.</value>
      /// <exception cref="ArgumentOutOfRangeException">
      /// <paramref name="value"/> is negative.
      /// </exception>
      public int Item
      {
        get
        {
          Debug.Assert(IsLeaf, "AABB tree node does not contain data.");
          return EscapeOffsetOrItem;
        }
        set
        {
          if (value < 0)
            throw new ArgumentOutOfRangeException("value", "A compressed AABB tree cannot store negative values.");

          EscapeOffsetOrItem = value;
        }
      }


      /// <summary>
      /// Gets or sets the escape offset of this node.
      /// </summary>
      /// <value>The escape offset of this node.</value>
      public int EscapeOffset
      {
        get
        {
          Debug.Assert(!IsLeaf, "AABB tree node does not contain an escape offset.");
          return -EscapeOffsetOrItem;
        }
        set
        {
          Debug.Assert(value > 0, "Invalid escape offset.");
          EscapeOffsetOrItem = -value;
        }
      }
    }
  }
}
