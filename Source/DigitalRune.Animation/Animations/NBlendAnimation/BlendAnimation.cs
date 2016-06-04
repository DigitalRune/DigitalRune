// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Blends animations within a <see cref="BlendGroup"/>. (For internal use only.)
  /// </summary>
  public abstract class BlendAnimation : IAnimation
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="BlendGroup"/>.
    /// </summary>
    public BlendGroup Group { get; private set; }


    /// <inheritdoc/>
    public FillBehavior FillBehavior
    {
      get { return (Group != null) ? Group.FillBehavior: FillBehavior.Stop; }
    }


    /// <inheritdoc/>
    public string TargetObject
    {
      get { return (Group != null) ? Group.TargetObject : String.Empty; }
    }


    /// <inheritdoc cref="Animation{T}.TargetProperty"/>
    public string TargetProperty { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the <see cref="BlendAnimation{T}"/> for specified number of animation.
    /// </summary>
    /// <param name="blendGroup">The <see cref="BlendGroup"/>.</param>
    /// <param name="targetProperty">The property to which the animation is applied.</param>
    internal virtual void Initialize(BlendGroup blendGroup, string targetProperty)
    {
      Group = blendGroup;
      TargetProperty = targetProperty;
    }



    /// <summary>
    /// Adds the specified animation to the <see cref="BlendAnimation{T}"/>.
    /// </summary>
    /// <param name="index">The index of the animation.</param>
    /// <param name="animation">The animation.</param>
    /// <remarks>
    /// The method does nothing if <paramref name="animation"/> is not of the correct type 
    /// <see cref="IAnimation{T}"/>.
    /// </remarks>
    internal abstract void AddAnimation(int index, IAnimation animation);


    /// <inheritdoc/>
    public abstract AnimationInstance CreateInstance();


    /// <summary>
    /// Not implemented. Throws an <see cref="AnimationException"/>.
    /// </summary>
    /// <returns>
    /// Not implemented. Throws an <see cref="AnimationException"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    BlendAnimation IAnimation.CreateBlendAnimation()
    {
      throw new AnimationException("Cannot create BlendAnimation<T>. The current animation is a BlendAnimation<T>.");
    }


    /// <inheritdoc/>
    public TimeSpan? GetAnimationTime(TimeSpan time)
    {
      return (Group != null) ? Group.GetAnimationTime(time) : null;
    }


    /// <inheritdoc/>
    public AnimationState GetState(TimeSpan time)
    {
      return (Group != null) ? Group.GetState(time) : AnimationState.Stopped;
    }


    /// <inheritdoc/>
    public TimeSpan GetTotalDuration()
    {
      return (Group != null) ? Group.GetTotalDuration() : TimeSpan.Zero;
    }
    #endregion
  }
}
