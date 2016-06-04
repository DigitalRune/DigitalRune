// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using System;
using DigitalRune.Animation.Traits;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see cref="Vector2"/> value by applying an animation to each component of the
  /// vector. (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class Vector2Animation : Animation<Vector2>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector2> Traits
    {
      get { return Vector2Traits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the x component.
    /// </summary>
    /// <value>The animation of the x component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> X { get; set; }


    /// <summary>
    /// Gets or sets the animation of the y component.
    /// </summary>
    /// <value>The animation of the y component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> Y { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2Animation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2Animation"/> class.
    /// </summary>
    public Vector2Animation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2Animation"/> class with the specified
    /// animations.
    /// </summary>
    /// <param name="x">The animation of the x component.</param>
    /// <param name="y">The animation of the y component.</param>
    public Vector2Animation(IAnimation<float> x, IAnimation<float> y)
    {
      X = x;
      Y = y;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override TimeSpan GetTotalDuration()
    {
      TimeSpan duration = TimeSpan.Zero;

      if (X != null)
        duration = AnimationHelper.Max(duration, X.GetTotalDuration());

      if (Y != null)
        duration = AnimationHelper.Max(duration, Y.GetTotalDuration());

      return duration;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref Vector2 defaultSource, ref Vector2 defaultTarget, ref Vector2 result)
    {
      if (X != null)
        X.GetValue(time, ref defaultSource.X, ref defaultTarget.X, ref result.X);
      else
        result.X = defaultSource.X;

      if (Y != null)
        Y.GetValue(time, ref defaultSource.Y, ref defaultTarget.Y, ref result.Y);
      else
        result.Y = defaultSource.Y;
    }
    #endregion
  }
}
#endif
