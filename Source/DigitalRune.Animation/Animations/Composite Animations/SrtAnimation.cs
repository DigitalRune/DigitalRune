// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Character;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates an <see cref="SrtTransform"/> by applying an animation to each component (scale,
  /// rotate, translate) of the transform.
  /// </summary>
  public class SrtAnimation : Animation<SrtTransform>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<SrtTransform> Traits
    {
      get { return SrtTransformTraits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the <see cref="SrtTransform.Scale"/> value.
    /// </summary>
    /// <value>The animation of the <see cref="SrtTransform.Scale"/> value.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<Vector3F> Scale { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="SrtTransform.Rotation"/> value.
    /// </summary>
    /// <value>The animation of the <see cref="SrtTransform.Rotation"/> value.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<QuaternionF> Rotation { get; set; }


    /// <summary>
    /// Gets or sets the animation of the <see cref="SrtTransform.Translation"/> value.
    /// </summary>
    /// <value>The animation of the <see cref="SrtTransform.Translation"/> value.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<Vector3F> Translation { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SrtAnimation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SrtAnimation"/> class.
    /// </summary>
    public SrtAnimation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SrtAnimation"/> class with the 
    /// specified animations.
    /// </summary>
    /// <param name="scale">The animation of the <see cref="SrtTransform.Scale"/> component.</param>
    /// <param name="rotation">The animation of the <see cref="SrtTransform.Rotation"/> component.</param>
    /// <param name="translation">The animation of the <see cref="SrtTransform.Translation"/> component.</param>
    public SrtAnimation(IAnimation<Vector3F> scale, IAnimation<QuaternionF> rotation, IAnimation<Vector3F> translation)
    {
      Scale = scale;
      Rotation = rotation;
      Translation = translation;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override TimeSpan GetTotalDuration()
    {
      TimeSpan duration = TimeSpan.Zero;

      if (Scale != null)
        duration = AnimationHelper.Max(duration, Scale.GetTotalDuration());

      if (Rotation != null)
        duration = AnimationHelper.Max(duration, Rotation.GetTotalDuration());

      if (Translation != null)
        duration = AnimationHelper.Max(duration, Translation.GetTotalDuration());

      return duration;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref SrtTransform defaultSource, ref SrtTransform defaultTarget, ref SrtTransform result)
    {
      if (Scale != null)
        Scale.GetValue(time, ref defaultSource.Scale, ref defaultTarget.Scale, ref result.Scale);
      else
        result.Scale = defaultSource.Scale;

      if (Rotation != null)
        Rotation.GetValue(time, ref defaultSource.Rotation, ref defaultTarget.Rotation, ref result.Rotation);
      else
        result.Rotation = defaultSource.Rotation;

      if (Translation != null)
        Translation.GetValue(time, ref defaultSource.Translation, ref defaultTarget.Translation, ref result.Translation);
      else
        result.Translation = defaultSource.Translation;
    }
    #endregion
  }
}
