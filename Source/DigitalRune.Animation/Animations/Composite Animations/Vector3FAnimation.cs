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
  /// Animates a <see cref="Vector3F"/> value by applying an animation to each component of the
  /// vector.
  /// </summary>
  public class Vector3FAnimation : Animation<Vector3F>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector3F> Traits
    {
      get { return Vector3FTraits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector3F.X"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector3F.X"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> X { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector3F.Y"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector3F.Y"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> Y { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="Vector3F.Z"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="Vector3F.Z"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> Z { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3FAnimation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3FAnimation"/> class.
    /// </summary>
    public Vector3FAnimation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3FAnimation"/> class with the specified
    /// animations.
    /// </summary>
    /// <param name="x">The animation of the <see cref="Vector3F.X"/> component.</param>
    /// <param name="y">The animation of the <see cref="Vector3F.Y"/> component.</param>
    /// <param name="z">The animation of the <see cref="Vector3F.Z"/> component.</param>
    public Vector3FAnimation(IAnimation<float> x, IAnimation<float> y, IAnimation<float> z)
    {
      X = x;
      Y = y;
      Z = z;
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

      return duration;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref Vector3F defaultSource, ref Vector3F defaultTarget, ref Vector3F result)
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
    }
    #endregion
  }
}
