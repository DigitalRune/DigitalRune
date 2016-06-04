// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see cref="Vector2F"/> value by applying an animation to each component of the
  /// vector.
  /// </summary>
  public class Vector2FAnimation : Animation<Vector2F>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector2F> Traits
    {
      get { return Vector2FTraits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector2F.X"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector2F.X"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> X { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector2F.Y"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector2F.Y"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> Y { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2FAnimation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2FAnimation"/> class.
    /// </summary>
    public Vector2FAnimation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2FAnimation"/> class with the specified
    /// animations.
    /// </summary>
    /// <param name="x">The animation of the <see cref="Vector2F.X"/> component.</param>
    /// <param name="y">The animation of the <see cref="Vector2F.Y"/> component.</param>
    public Vector2FAnimation(IAnimation<float> x, IAnimation<float> y)
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
    protected override void GetValueCore(TimeSpan time, ref Vector2F defaultSource, ref Vector2F defaultTarget, ref Vector2F result)
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
