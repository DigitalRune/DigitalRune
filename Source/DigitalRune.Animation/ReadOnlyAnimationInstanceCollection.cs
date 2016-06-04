// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents an empty read-only version of the <see cref="AnimationInstanceCollection"/>.
  /// </summary>
  /// <remarks>
  /// This collection acts like a singleton that is used when AnimationInstance will not have any
  /// children.
  /// </remarks>
  internal sealed class ReadOnlyAnimationInstanceCollection : AnimationInstanceCollection
  {
    /// <summary>
    /// Gets a read-only instance of the <see cref="AnimationInstanceCollection"/>.
    /// </summary>
    public static ReadOnlyAnimationInstanceCollection Instance
    {
      get
      {
        if (_instance == null)
          _instance = new ReadOnlyAnimationInstanceCollection();

        return _instance;
      }
    }
    private static ReadOnlyAnimationInstanceCollection _instance;


    /// <summary>
    /// Prevents a default instance of the <see cref="ReadOnlyAnimationInstanceCollection"/> class 
    /// from being created.
    /// </summary>
    private ReadOnlyAnimationInstanceCollection()
      : base(null, 0)
    {
    }


    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="child"/> should be inserted.
    /// </param>
    /// <param name="child">The object to insert.</param>
    /// <exception cref="InvalidOperationException">
    /// Cannot add animation instance. The current animation instance cannot have children.
    /// </exception>
    protected override void InsertItem(int index, AnimationInstance child)
    {
      throw new InvalidOperationException("Cannot add animation instance. The current animation instance cannot have children.");
    }
  }
}
