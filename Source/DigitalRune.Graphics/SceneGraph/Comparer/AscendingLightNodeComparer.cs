// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Sorts <see cref="LightNode"/>s by their <see cref="LightNode.Priority"/> and then by
  /// their <see cref="SceneNode.SortTag"/> in ascending order.
  /// </summary>
  internal sealed class AscendingLightNodeComparer
    : Singleton<AscendingLightNodeComparer>, IComparer<LightNode>
  {
    /// <summary>
    /// Compares two <see cref="LightNode"/>s.
    /// </summary>
    /// <param name="x">The first <see cref="SceneNode"/> to compare.</param>
    /// <param name="y">The second <see cref="SceneNode"/> to compare.</param>
    /// <returns>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Condition</description>
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public int Compare(LightNode x, LightNode y)
    {
      if (x.Priority < y.Priority)
        return -1;
      if (x.Priority > y.Priority)
        return +1;

      return x.SortTag.CompareTo(y.SortTag);
    }
  }
}
