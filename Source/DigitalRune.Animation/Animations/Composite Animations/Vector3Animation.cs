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
  /// Animates a <see cref="Vector3"/> value by applying an animation to each component of the
  /// vector. (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class Vector3Animation : Animation<Vector3>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector3> Traits
    {
      get { return Vector3Traits.Instance; }
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


    /// <summary>
    /// Gets or sets the animation of the z component.
    /// </summary>
    /// <value>The animation of the z component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> Z { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Animation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Animation"/> class.
    /// </summary>
    public Vector3Animation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Animation"/> class with the specified
    /// animations.
    /// </summary>
    /// <param name="x">The animation of the x component.</param>
    /// <param name="y">The animation of the y component.</param>
    /// <param name="z">The animation of the z component.</param>
    public Vector3Animation(IAnimation<float> x, IAnimation<float> y, IAnimation<float> z)
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
    protected override void GetValueCore(TimeSpan time, ref Vector3 defaultSource, ref Vector3 defaultTarget, ref Vector3 result)
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
#endif
