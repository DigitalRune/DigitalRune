// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Manages the children of an animation instance.
  /// </summary>
  public class AnimationInstanceCollection : ChildCollection<AnimationInstance, AnimationInstance>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationInstanceCollection"/> class.
    /// </summary>
    /// <param name="owner">The animation instance that owns this child collection.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="owner" /> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="owner"/> already owns a child collection.
    /// </exception>
    internal AnimationInstanceCollection(AnimationInstance owner) 
      : base(owner)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");

      if (owner.Children != null)
        throw new ArgumentException("AnimationInstance already owns an AnimationInstanceCollection.", "owner");
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationInstanceCollection"/> class.
    /// </summary>
    /// <param name="owner">The animation instance that owns this child collection.</param>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    protected AnimationInstanceCollection(AnimationInstance owner, int capacity)
      : base(owner, capacity)
    {
    }
    

    /// <summary>
    /// Gets the parent of an object.
    /// </summary>
    /// <param name="child">The child object.</param>
    /// <returns>The parent of <paramref name="child"/>.</returns>
    protected override AnimationInstance GetParent(AnimationInstance child)
    {
      return child.Parent;
    }


    /// <summary>
    /// Sets the parent of the given object.
    /// </summary>
    /// <param name="parent">The parent to set.</param>
    /// <param name="child">The child object.</param>
    protected override void SetParent(AnimationInstance child, AnimationInstance parent)
    {
      child.Parent = parent;
    }
  }
}
