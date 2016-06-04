// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents the current LOD or LOD transition.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [DebuggerDisplay("{GetType().Name,nq}(CurrentIndex = {CurrentIndex}, NextIndex = {NextIndex}, Transition = {Transition})")]
  public struct LodSelection : IEquatable<LodSelection>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// The index of the current LOD.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public int CurrentIndex;


    /// <summary>
    /// The current LOD node.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public SceneNode Current;


    /// <summary>
    /// The index of the next LOD, if the object is transitioning between two LODs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public int NextIndex;


    /// <summary>
    /// The next LOD node, if the object is transitioning between two LODs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public SceneNode Next;


    /// <summary>
    /// The transition progress [0, 1[, if the object is transitioning between two LODs.
    /// </summary>
    /// <remarks>
    /// 0 means that no transition is active, <see cref="Current"/> references the current LOD.
    /// A value between 0 and 1 means that a LOD transition from <see cref="Current"/> to 
    /// <see cref="Next"/> is in progress.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public float Transition;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="LodSelection"/> struct.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="LodSelection"/> struct describing the current 
    /// LOD.
    /// </summary>
    /// <param name="currentIndex">The index of the current LOD.</param>
    /// <param name="current">The current LOD node.</param>
    internal LodSelection(int currentIndex, SceneNode current)
    {
      Debug.Assert(currentIndex >= 0, "The index of the current LOD must not be negative.");
      // current can be null. See LodData.

      CurrentIndex = currentIndex;
      Current = current;
      NextIndex = -1;
      Next = null;
      Transition = 0;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LodSelection"/> struct describing the current 
    /// LOD transition.
    /// </summary>
    /// <param name="currentIndex">The index of the current LOD.</param>
    /// <param name="nextIndex">The index of the next LOD.</param>
    /// <param name="current">The current LOD node.</param> 
    /// <param name="next">The next LOD node.</param>
    /// <param name="transition">The progress of the transition.</param>
    internal LodSelection(int currentIndex, SceneNode current, int nextIndex, SceneNode next, float transition)
    {
      Debug.Assert(currentIndex >= 0, "The index of the current LOD must not be negative.");
      Debug.Assert(current != null, "The current LOD node must not be null.");
      Debug.Assert(nextIndex >= 0, "The index of the next LOD must not be negative.");
      Debug.Assert(next != null, "The current LOD node must not be null.");
      Debug.Assert(0 <= transition && transition < 1, "LOD transition progress expected to be in the range [0, 1[.");

      CurrentIndex = currentIndex;
      Current = current;
      NextIndex = nextIndex;
      Next = next;
      Transition = transition;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Equality Members -----
    
    /// <summary>
    /// Determines whether the specified <see cref="Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is LodSelection && Equals((LodSelection)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(LodSelection other)
    {
      return CurrentIndex == other.CurrentIndex 
             && Current == other.Current
             && NextIndex == other.NextIndex
             && Next == other.Next
             // ReSharper disable once CompareOfFloatsByEqualityOperator
             && Transition == other.Transition;
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        var hashCode = CurrentIndex;
        hashCode = (hashCode * 397) ^ (Current != null ? Current.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ NextIndex;
        hashCode = (hashCode * 397) ^ (Next != null ? Next.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ Transition.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Compares two <see cref="LodSelection"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="LodSelection"/>.</param>
    /// <param name="right">The second <see cref="LodSelection"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(LodSelection left, LodSelection right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="LodSelection"/>s to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="LodSelection"/>.</param>
    /// <param name="right">The second <see cref="LodSelection"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(LodSelection left, LodSelection right)
    {
      return !left.Equals(right);
    }
    #endregion

    #endregion
  }
}
