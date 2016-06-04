// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines a method to compare <see cref="IKeyFrame{T}"/> objects.
  /// </summary>
  /// <typeparam name="T">The type of the value stored in the key frame.</typeparam>
  internal sealed class AscendingKeyFrameComparer<T> : Singleton<AscendingKeyFrameComparer<T>>, IComparer<IKeyFrame<T>>
  {
    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or
    /// greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of x and y, as shown in the following 
    /// table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description><paramref name="x"/> is less than <paramref name="y"/>.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description><paramref name="x"/> equals <paramref name="y"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description><paramref name="x"/> is greater than <paramref name="y"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public int Compare(IKeyFrame<T> x, IKeyFrame<T> y)
    {
      return x.Time.CompareTo(y.Time);
    }
  }
}
