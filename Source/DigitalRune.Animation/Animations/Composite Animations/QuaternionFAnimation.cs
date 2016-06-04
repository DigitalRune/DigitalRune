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
  /// Animates a <see cref="QuaternionF"/> value by applying an animation to each component of the
  /// quaternion.
  /// </summary>
  public class QuaternionFAnimation : Animation<QuaternionF>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<QuaternionF> Traits
    {
      get { return QuaternionFTraits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the <see cref="QuaternionF.W"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="QuaternionF.W"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> W { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="QuaternionF.X"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="QuaternionF.X"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> X { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="QuaternionF.Y"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="QuaternionF.Y"/> component.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<float> Y { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="QuaternionF.Z"/> component.
    /// </summary>
    /// <value>The animation of the <see cref="QuaternionF.Z"/> component.</value>
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
    /// Initializes a new instance of the <see cref="QuaternionFAnimation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionFAnimation"/> class.
    /// </summary>
    public QuaternionFAnimation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionFAnimation"/> class with the 
    /// specified animations.
    /// </summary>
    /// <param name="w">The animation of the <see cref="QuaternionF.W"/> component.</param>
    /// <param name="x">The animation of the <see cref="QuaternionF.X"/> component.</param>
    /// <param name="y">The animation of the <see cref="QuaternionF.Y"/> component.</param>
    /// <param name="z">The animation of the <see cref="QuaternionF.Z"/> component.</param>
    public QuaternionFAnimation(IAnimation<float> w, IAnimation<float> x, IAnimation<float> y, IAnimation<float> z)
    {
      W = w;
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

      if (W != null)
        duration = AnimationHelper.Max(duration, W.GetTotalDuration());

      if (X != null)
        duration = AnimationHelper.Max(duration, X.GetTotalDuration());

      if (Y != null)
        duration = AnimationHelper.Max(duration, Y.GetTotalDuration());

      if (Z != null)
        duration = AnimationHelper.Max(duration, Z.GetTotalDuration());

      return duration;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref QuaternionF defaultSource, ref QuaternionF defaultTarget, ref QuaternionF result)
    {
      if (W != null)
        W.GetValue(time, ref defaultSource.W, ref defaultTarget.W, ref result.W);
      else
        result.W = defaultSource.W;

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
        result.Z = defaultSource.Z;
    }
    #endregion
  }
}
