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
  /// Animates a <see cref="Vector4F"/> value by applying an animation to each component of the
  /// vector.
  /// </summary>
  public class Vector4FAnimation : Animation<Vector4F>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector4F> Traits
    {
      get { return Vector4FTraits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector4F.X"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector4F.X"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> X { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector4F.Y"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector4F.Y"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> Y { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector4F.Z"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector4F.Z"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> Z { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector4F.W"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector4F.W"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> W { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector4FAnimation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector4FAnimation"/> class.
    /// </summary>
    public Vector4FAnimation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector4FAnimation"/> class with the specified
    /// animations.
    /// </summary>
    /// <param name="x">The animation of the <see cref="Vector4F.X"/> component.</param>
    /// <param name="y">The animation of the <see cref="Vector4F.Y"/> component.</param>
    /// <param name="z">The animation of the <see cref="Vector4F.Z"/> component.</param>
    /// <param name="w">The animation of the <see cref="Vector4F.W"/> component.</param>
    public Vector4FAnimation(IAnimation<float> x, IAnimation<float> y, IAnimation<float> z, IAnimation<float> w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
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

      if (Z != null)
        duration = AnimationHelper.Max(duration, Z.GetTotalDuration());

      if (W != null)
        duration = AnimationHelper.Max(duration, W.GetTotalDuration());

      return duration;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref Vector4F defaultSource, ref Vector4F defaultTarget, ref Vector4F result)
    {
      if (X != null)
        X.GetValue(time, ref defaultSource.X, ref defaultTarget.X, ref result.X);
      else
        result.X = defaultSource.X;

      if (Y != null)
        Y.GetValue(time, ref defaultSource.Y, ref defaultTarget.Y, ref result.Y);
      else
        result.Y = defaultSource.Y;

      if (Z != null)
        Z.GetValue(time, ref defaultSource.Z, ref defaultTarget.Z, ref result.Z);
      else
        result.Y = defaultSource.Z;

      if (W != null)
        W.GetValue(time, ref defaultSource.W, ref defaultTarget.W, ref result.W);
      else
        result.W = defaultSource.W;
    }
    #endregion
  }
}
