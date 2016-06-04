// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Manages a collection of key frames.
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// <strong>Important:</strong> The <see cref="KeyFrameAnimation{T}"/> expects that the key frames
  /// are sorted by their time. But the <see cref="KeyFrameCollection{T}"/> does not automatically
  /// order the key frames. The method <see cref="Sort"/> needs to be called if key frames are added
  /// in the wrong order!
  /// </remarks>
  public class KeyFrameCollection<T> : Collection<IKeyFrame<T>>
  {
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="KeyFrameCollection{T}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="KeyFrameCollection{T}"/>.
    /// </returns>
    public new List<IKeyFrame<T>>.Enumerator GetEnumerator()
    {
      return ((List<IKeyFrame<T>>)Items).GetEnumerator();
    }


    /// <summary>
    /// Sorts the key frames in the collection by their time value.
    /// </summary>
    public void Sort()
    {
      ((List<IKeyFrame<T>>)Items).Sort(AscendingKeyFrameComparer<T>.Instance);
    }
  }
}
