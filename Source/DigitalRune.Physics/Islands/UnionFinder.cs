// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Mathematics;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Describes one element in the <see cref="UnionFinder"/>.
  /// </summary>
  [DebuggerDisplay("Union {Id}: Size={Size})")]
  internal struct UnionElement : IComparable<UnionElement>
  {
    public int Id;
    public int Size;  // Island size = Number of UnionElements in this island.

    public UnionElement(int id, int size)
    {
      Id = id;
      Size = size;
    }

    public int CompareTo(UnionElement other)
    {
      return Id - other.Id;
    }
  }


  /// <summary>
  /// Implements Weighted Quick Union with Path Compression - an optimal algorithm for finding 
  /// connected parts.
  /// </summary>
  internal sealed class UnionFinder : IComparer<UnionElement>
  {
    // See http://www.cs.princeton.edu/algs4/15uf/.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the elements.
    /// </summary>
    /// <value>The elements.</value>
    public UnionElement[] Elements { get; private set; }


    /// <summary>
    /// Gets the number of unions.
    /// </summary>
    /// <value>The number of unions.</value>
    public int NumberOfUnions { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="UnionFinder"/> class.
    /// </summary>
    public UnionFinder()
    {
      Elements = new UnionElement[256];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets the union finder.
    /// </summary>
    /// <param name="numberOfElements">
    /// The number of elements that should be in the <see cref="Elements"/> list.
    /// </param>
    /// <remarks>
    /// This method fills the <see cref="Elements"/> list with <paramref name="numberOfElements"/>
    /// elements. The element IDs are identical to their index in the <see cref="Elements"/> list.
    /// </remarks>
    public void Reset(int numberOfElements)
    {
      if (numberOfElements > 256)
      {
        // Elements array has always at least 256 entries.
        // Grow if necessary. Shrink if 
        if (numberOfElements > Elements.Length || numberOfElements * 4 < Elements.Length)
        {
          var capacity = MathHelper.Bitmask((uint)numberOfElements) + 1; // Next power of 2.
          Elements = new UnionElement[2 * capacity];
        }
      }

      for (int i = 0; i < numberOfElements; i++)
        Elements[i] = new UnionElement(i, 1);

      // Each element is in its own union.
      NumberOfUnions = numberOfElements;
    }


    /// <summary>
    /// Checks whether two elements belong to the same union.
    /// </summary>
    /// <param name="p">The index of the first element.</param>
    /// <param name="q">The index of the second element..</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="p"/> and <paramref name="q"/> belong to the same
    /// union; otherwise, <see langword="false"/>.
    /// </returns>
    public bool AreUnited(int p, int q)
    {
      return FindUnion(p) == FindUnion(q);
    }


    /// <summary>
    /// Compares the specified <see cref="UnionElement"/>s.
    /// </summary>
    /// <param name="elementA">The first element.</param>
    /// <param name="elementB">The second element.</param>
    /// <returns>
    /// +1 if the ID of <paramref name="elementA"/> is greater than the ID of
    /// <paramref name="elementB"/>. -1 if the ID of <paramref name="elementA"/>
    /// is less than the ID of <paramref name="elementB"/>. Otherwise, 0
    /// is returned.
    /// </returns>
    public int Compare(UnionElement elementA, UnionElement elementB)
    {
      return elementA.Id - elementB.Id;
    }


    /// <summary>
    /// Gets the number of the union of the given element.
    /// </summary>
    /// <param name="p">The index of the element.</param>
    /// <returns>The union number of the element.</returns>
    public int FindUnion(int p)
    {
      // p = -1 can happen if a constraint is in the simulation but the constrained bodies
      // are not. 
      if (p < 0)
        return -1;

      Debug.Assert(p >= 0 && p <= Elements.Length);

      int parentIndex = Elements[p].Id;
      while (p != parentIndex)
      {
        // Path compression: update element p so that it moves up the union tree.
        Elements[p] = Elements[parentIndex];

        p = parentIndex;
        parentIndex = Elements[p].Id;
      }

      return p;
    }


    /// <summary>
    /// Gets the size of the union that contains the given element.
    /// </summary>
    /// <param name="p">The index of the element.</param>
    /// <returns>
    /// The number of elements that are in the same union as the given element.
    /// </returns>
    public int GetUnionSize(int p)
    {
      // Note: Only the size of the top element is up-to-date.
      return Elements[FindUnion(p)].Size;
    }


    /// <summary>
    /// Merges the unions of the two given elements.
    /// </summary>
    /// <param name="p">The index of the first element.</param>
    /// <param name="q">The index of the second element.</param>
    public void Unite(int p, int q)
    {
      // Find union numbers.
      int i = FindUnion(p);
      int j = FindUnion(q);

      // Already in the same union? - Abort.
      if (i == j)
        return;

      // Get the union sizes.
      int sizeP = Elements[i].Size;
      int sizeQ = Elements[j].Size;

      // The elements will be replaced by this entry.
      UnionElement mergedElement;

      // Connect smaller tree to larger tree.
      if (sizeP < sizeQ)
        mergedElement = new UnionElement(j, sizeP + sizeQ);
      else
        mergedElement = new UnionElement(i, sizeP + sizeQ);

      Elements[i] = Elements[j] = mergedElement;

      // One union less.
      NumberOfUnions--;
    }
    #endregion
  }
}
