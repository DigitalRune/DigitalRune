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
  /// Animates a <see cref="Color"/> value by applying an animation to each component of the color.
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class ColorAnimation : Animation<Color>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override IAnimationValueTraits<Color> Traits
    {
      get { return ColorTraits.Instance; }
    }


    /// <summary>
    /// Gets or sets the animation of the red component.
    /// </summary>
    /// <value>The animation of the red component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> R { get; set; }


    /// <summary>
    /// Gets or sets the animation of the green component.
    /// </summary>
    /// <value>The animation of the green component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> G { get; set; }


    /// <summary>
    /// Gets or sets the animation of the blue component.
    /// </summary>
    /// <value>The animation of the blue component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> B { get; set; }


    /// <summary>
    /// Gets or sets the animation of the alpha component.
    /// </summary>
    /// <value>The animation of the alpha component.</value>
    [ContentSerializer(SharedResource = true)]
    public IAnimation<float> A { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorAnimation"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorAnimation"/> class.
    /// </summary>
    public ColorAnimation()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ColorAnimation"/> class with the specified
    /// animations.
    /// </summary>
    /// <param name="r">The animation of the red component.</param>
    /// <param name="g">The animation of the green component.</param>
    /// <param name="b">The animation of the blue component.</param>
    /// <param name="a">The animation of the alpha component.</param>
    public ColorAnimation(IAnimation<float> r, IAnimation<float> g, IAnimation<float> b, IAnimation<float> a)
    {
      R = r;
      G = g;
      B = b;
      A = a;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override TimeSpan GetTotalDuration()
    {
      TimeSpan duration = TimeSpan.Zero;

      if (R != null)
        duration = AnimationHelper.Max(duration, R.GetTotalDuration());

      if (G != null)
        duration = AnimationHelper.Max(duration, G.GetTotalDuration());

      if (B != null)
        duration = AnimationHelper.Max(duration, B.GetTotalDuration());

      if (A != null)
        duration = AnimationHelper.Max(duration, A.GetTotalDuration());

      return duration;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref Color defaultSource, ref Color defaultTarget, ref Color result)
    {
      var source = defaultSource.ToVector4();
      var target = defaultTarget.ToVector4();

      Vector4 value = source;

      if (R != null)
        R.GetValue(time, ref source.X, ref target.X, ref value.X);

      if (G != null)
        G.GetValue(time, ref source.Y, ref target.Y, ref value.Y);

      if (B != null)
        B.GetValue(time, ref source.Z, ref target.Z, ref value.Z);

      if (A != null) 
        A.GetValue(time, ref source.W, ref target.W, ref value.W);

      result = new Color(value.X, value.Y, value.Z, value.W);
    }
    #endregion
  }
}
#endif
