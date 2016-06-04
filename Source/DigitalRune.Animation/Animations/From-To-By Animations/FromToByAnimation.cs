// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Animation.Easing;
using DigitalRune.Mathematics;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a value from/to/by a certain value. (Base implementation.)
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// <para>
  /// This type of animation changes a value from, to, or by a certain value depending on the 
  /// properties set:
  /// <list type="table">
  /// <listheader>
  /// <term>Properties Specified</term>
  /// <description>Resulting Behavior</description>
  /// </listheader>
  /// <item>
  /// <term>From and To</term>
  /// <description>
  /// The animation interpolates between the value specified by <see cref="From"/> and the value 
  /// specified by <see cref="To"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <term>From and By</term>
  /// <description>
  /// The animation interpolates between the value specified by <see cref="From"/> and the sum of 
  /// the values specified by <see cref="From"/> and <see cref="By"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <term>From</term>
  /// <description>
  /// <para>
  /// The animation interpolates between the value specified by <see cref="From"/> and the default 
  /// source value (see parameters of <see cref="Animation{T}.GetValue"/>). 
  /// </para>
  /// <para>
  /// When an 
  /// <see cref="IAnimatableProperty"/> is animated this means that the <see cref="From"/> value is 
  /// animated towards the output of the previous animation. If there is no previous animation, then
  /// the <see cref="From"/> value is animated towards the base value of the property. 
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>To</term>
  /// <description>
  /// <para>
  /// The animation interpolates from the default source value (see parameters of 
  /// <see cref="Animation{T}.GetValue"/>) to the value specified by <see cref="To"/>. 
  /// </para>
  /// <para>
  /// When an 
  /// <see cref="IAnimatableProperty"/> is animated this means that the output of the previous 
  /// animation is animated towards the <see cref="To"/> value. If there is no previous animation,
  /// then the base value of the property is animated towards the <see cref="To"/> value.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>By</term>
  /// <description>
  /// <para>
  /// The animation interpolates from the default source value (see parameters of 
  /// <see cref="Animation{T}.GetValue"/>) to the sum of this value plus the value specified by 
  /// <see cref="By"/>. 
  /// </para>
  /// <para>
  /// When an <see cref="IAnimatableProperty"/> is animated this means that the output of the
  /// previous animation is animated towards the sum of the this value plus the <see cref="By"/> 
  /// value. If there is no previous animation, then the base value of the property is animated 
  /// towards the sum of the base value plus the <see cref="By"/> value.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>No properties set</term>
  /// <description>
  /// <para>
  /// The animation interpolates between the default source value to the default target value (see
  /// parameters of <see cref="Animation{T}.GetValue"/>). 
  /// </para>
  /// <para>
  /// When an <see cref="IAnimatableProperty"/> is animated this means that the previous animation's
  /// output value is animated towards the base value of the property.
  /// </para>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// If both <see cref="To"/> and <see cref="By"/> are specified, the <see cref="By"/> value will
  /// be ignored.
  /// </para>
  /// <para>
  /// The property <see cref="Duration"/> specifies the period of time over which the interpolation
  /// takes place.
  /// </para>
  /// <para>
  /// By default, the animation interpolates linearly between the specified values. An easing
  /// function (see property <see cref="EasingFunction"/>) can be applied to control the pace of the
  /// transition. 
  /// </para>
  /// <para>
  /// The property <see cref="Animation{T}.IsAdditive"/> can be set to add the output of the 
  /// animation to the property that is being animated. Note that the animation needs to be fully 
  /// defined to be additive. A from/to/by animation is fully defined if either <see cref="From"/> 
  /// and <see cref="To"/> or <see cref="From"/> and <see cref="By"/> are set. The result is 
  /// undefined if the animation is only partially defined (either the start or the final value is 
  /// missing)!
  /// </para>
  /// </remarks>
  public abstract class FromToByAnimation<T> : Animation<T> where T : struct
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the duration of the interpolation.
    /// </summary>
    /// <value>The duration of the interpolation. (The default value is 1 second.)
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public TimeSpan Duration
    {
      get { return _duration; }
      set
      {
        if (value < TimeSpan.Zero)
          throw new ArgumentOutOfRangeException("value", "The duration of an animation must not be negative.");

        _duration = value;
      }
    }
    private TimeSpan _duration;


    /// <summary>
    /// Gets or sets the start value of the animation.
    /// </summary>
    /// <value>
    /// The start value of the animation. The default value is <see langword="null"/>.
    /// </value>
    public T? From { get; set; }


    /// <summary>
    /// Gets or sets the final value of the animation.
    /// </summary>
    /// <value>
    /// The final value of the animation. The default value is <see langword="null"/>.
    /// </value>
    public T? To { get; set; }


    /// <summary>
    /// Gets or sets the final value of the animation relative to the start value.
    /// </summary>
    /// <value>
    /// The final value of the animation relative to the start value. The default value is 
    /// <see langword="null"/>.
    /// </value>
    public T? By { get; set; }


    /// <summary>
    /// Gets or sets the easing function that controls the pace of the interpolation.
    /// </summary>
    /// <value>
    /// The easing function that controls the pace of the interpolation. The default value is 
    /// <see langword="null"/>, which means that a linear interpolation is applied.
    /// </value>
    /// <remarks>
    /// An <see cref="IEasingFunction"/> can be applied to control the pace of the interpolation.
    /// For example, a <see cref="CubicEase"/> can be used to start slow, but then accelerate 
    /// towards the target value. Special easing functions, such as the <see cref="BounceEase"/> or 
    /// <see cref="ElasticEase"/> can be used to create special animation effects, such as bounces
    /// or oscillations.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IEasingFunction EasingFunction { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FromToByAnimation{T}"/> class.
    /// </summary>
    protected FromToByAnimation()
    {
      _duration = new TimeSpan(0, 0, 0, 1); // 1 s
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override TimeSpan GetTotalDuration()
    {
      return _duration;
    }


    /// <inheritdoc cref="Animation{T}.GetValueCore"/>
    protected override void GetValueCore(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result)
    {
      TimeSpan duration = Duration;

      // Compute the normalized time [0, 1]:
      float normalizedTime;
      if (duration == TimeSpan.Zero)
      {
        // Animation should stop immediately. (Avoid division by zero.)
        normalizedTime = 1.0f;
      }
      else
      {
        Debug.Assert(duration > TimeSpan.Zero, "Positive duration expected.");
        if (duration == TimeSpan.MaxValue)
          normalizedTime = 0.0f;
        else
          normalizedTime = (float)((double)time.Ticks / duration.Ticks);
      }

      // Clamp normalizedTime to [0, 1].
      normalizedTime = MathHelper.Clamp(normalizedTime, 0.0f, 1.0f);

      // ----- EasingFunction
      if (EasingFunction != null)
        normalizedTime = EasingFunction.Ease(normalizedTime);

      // ----- From/To/By
      var traits = Traits;

      T source;
      if (From.HasValue)
      {
        source = From.Value;
      }
      else
      {
        source = defaultSource;
      }

      // Optimization: Early out if we don't need target.
      if (normalizedTime == 0.0f)
      {
        traits.Copy(ref source, ref result);
        return;
      }

      T target;
      bool recycleTarget = false;
      if (To.HasValue)
      {
        target = To.Value;
      }
      else if (By.HasValue)
      {
        T by = By.Value;
        
        traits.Create(ref defaultSource, out target);
        Traits.Add(ref source, ref by, ref target);
        recycleTarget = true;
      }
      else if (From.HasValue)
      {
        target = defaultSource;
      }
      else
      {
        target = defaultTarget;
      }

      // Optimization: Early out if we don't need to interpolate values.
      if (normalizedTime == 1.0f)
      {
        traits.Copy(ref target, ref result);
        if (recycleTarget)
          traits.Recycle(ref target);

        return;
      }

      // ----- Interpolation
      Traits.Interpolate(ref source, ref target, normalizedTime, ref result);

      if (recycleTarget)
        traits.Recycle(ref target);
    }
    #endregion
  }
}
