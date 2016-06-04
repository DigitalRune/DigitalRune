// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a collection of lens flare elements.
  /// </summary>
  public class LensFlareElementCollection : Collection<LensFlareElement>
  {
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="LensFlareElementCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> for <see cref="LensFlareElementCollection"/>.
    /// </returns>
    public new List<LensFlareElement>.Enumerator GetEnumerator()
    {
      return ((List<LensFlareElement>)Items).GetEnumerator();
    }
  }
}
