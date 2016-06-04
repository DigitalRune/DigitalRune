// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Particles.Effectors
{

  /// <summary>
  /// Fades a particle parameter of type <see cref="float"/> in and out.
  /// </summary>
  /// <remarks>
  /// <para>
  /// All parameters must be of type <see cref="float"/>.
  /// </para>
  /// <para>
  /// This effector changes a parameter value from 0 to a target value ("fade-in"). Then the value 
  /// is kept at the target value. Later the value is changed from the target value to 0 
  /// ("fade-out").
  /// </para>
  /// <para>
  /// The fade-in interval is defined by <see cref="FadeInStart"/> and <see cref="FadeInEnd"/>. The 
  /// fade-out interval is defined by <see cref="FadeOutStart"/> and <see cref="FadeOutEnd"/>. These
  /// intervals should be non-overlapping and the start values should be less than the corresponding
  /// end values. Usually, the fade-in interval lies before the fade-out interval, but it is allowed
  /// to swap the intervals to create a fade-out followed by a fade-in. The factor parameter (see 
  /// <see cref="TimeParameter"/>) defines the progress of the fade-in/out. This parameter is 
  /// usually the "NormalizedAge" of the particles. 
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="ValueParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that stores the result.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="TargetValueParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the target value. This parameter is optional.
  /// If it is not set, the target value is 1.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="TimeParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the fade-in/out factor. If this value is between 
  /// <see cref="FadeInStart"/> and <see cref="FadeInEnd"/>, then the value parameter is faded in. 
  /// If this value is between <see cref="FadeOutStart"/> and <see cref="FadeOutEnd"/>, then the 
  /// value parameter is faded out. Per default, the parameter "NormalizedAge" is used.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class SingleFadeEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<float> _valueParameter;
    private IParticleParameter<float> _targetValueParameter;
    private IParticleParameter<float> _timeParameter;

    // Cached values:
    private float _fadeInDuration;
    private float _fadeOutDuration;
    private bool _fadeInBeforeFadeOut;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the parameter that is faded in/out.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that is faded in/out.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.Out)]
    public string ValueParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that defines the target value.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the target value.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is <see langword="null"/>. If this parameter is missing, the
    /// target value is 1.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In, Optional = true)]
    public string TargetValueParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that defines the progress of the fade-in/out.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the progress of the fade-in/out.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is "NormalizedAge".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string TimeParameter { get; set; }


    /// <summary>
    /// Gets or sets the threshold that defines when the fade-in starts.
    /// </summary>
    /// <value>
    /// The threshold that defines when the fade-in starts. 
    /// This value should be less than <see cref="FadeInEnd"/>.
    /// The default value is 0.
    /// </value>
    public float FadeInStart { get; set; }


    /// <summary>
    /// Gets or sets the threshold that defines when the fade-in ends.
    /// </summary>
    /// <value>
    /// The threshold that defines when the fade-in ends. This value should be greater than 
    /// <see cref="FadeInStart"/>. The default value is 0.5.
    /// </value>
    public float FadeInEnd { get; set; }


    /// <summary>
    /// Gets or sets the threshold that defines when the fade-out starts.
    /// </summary>
    /// <value>
    /// The threshold that defines when the fade-out starts. This value should be less than 
    /// <see cref="FadeOutEnd"/>. The default value is 0.5.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public float FadeOutStart { get; set; }


    /// <summary>
    /// Gets or sets the threshold that defines when the fade-out ends.
    /// </summary>
    /// <value>
    /// The threshold that defines when the fade-out ends. This value should be greater than 
    /// <see cref="FadeOutStart"/>. The default value is 1.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public float FadeOutEnd { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleFadeEffector"/> class.
    /// </summary>
    public SingleFadeEffector()
    {
      TimeParameter = ParticleParameterNames.NormalizedAge;

      FadeInStart = 0.0f;
      FadeInEnd = 0.5f;
      FadeOutStart = 0.5f;
      FadeOutEnd = 1.0f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new SingleFadeEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone SingleFadeEffector properties.
      var sourceTyped = (SingleFadeEffector)source;
      ValueParameter = sourceTyped.ValueParameter;
      TargetValueParameter = sourceTyped.TargetValueParameter;
      TimeParameter = sourceTyped.TimeParameter;
      FadeInStart = sourceTyped.FadeInStart;
      FadeInEnd = sourceTyped.FadeInEnd;
      FadeOutStart = sourceTyped.FadeOutStart;
      FadeOutEnd = sourceTyped.FadeOutEnd;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _valueParameter = ParticleSystem.Parameters.Get<float>(ValueParameter);
      _targetValueParameter = ParticleSystem.Parameters.Get<float>(TargetValueParameter);
      _timeParameter = ParticleSystem.Parameters.Get<float>(TimeParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _valueParameter = null;
      _targetValueParameter = null;
      _timeParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_valueParameter == null
          || _timeParameter == null)
      {
        return;
      }

      // Cache values to avoid recomputation.
      _fadeInDuration = FadeInEnd - FadeInStart;
      _fadeOutDuration = FadeOutEnd - FadeOutStart;
      _fadeInBeforeFadeOut = (FadeInStart <= FadeOutStart);

      var values = _valueParameter.Values;
      if (values == null)
      {
        // Value is a uniform parameter.
        _valueParameter.DefaultValue = Fade(-1);
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_valueParameter == null
          || _timeParameter == null)
      {
        return;
      }

      var values = _valueParameter.Values;
      if (values == null)
      {
        // Value is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      // Value is a varying parameter.
      float[] times = _timeParameter.Values;
      float[] targetValues = (_targetValueParameter != null) ? _targetValueParameter.Values : null;

      if (times != null && targetValues != null)
      {
        // Optimized case: Time and TargetValue are varying parameters.
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = Fade(times[i], targetValues[i]);
      }
      else if (times != null)
      {
        // Optimized case: Time is varying parameter and TargetValue is uniform parameter.
        Debug.Assert(targetValues == null);
        float targetValue = (_targetValueParameter != null) ? _targetValueParameter.DefaultValue : 1;
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = Fade(times[i], targetValue);
      }
      else
      {
        // General case:
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = Fade(i);
      }
    }


    private float Fade(int index)
    {
      float time = _timeParameter.GetValue(index);
      float targetValue = (_targetValueParameter != null) ? _targetValueParameter.GetValue(index) : 1;
      return Fade(time, targetValue);
    }


    private float Fade(float time, float targetValue)
    {
      float value;

      if (_fadeInBeforeFadeOut)
      {
        // ----- Case 1: Fade In -> Fade Out
        if (time <= FadeInStart)
        {
          value = 0;
        }
        else if (time < FadeInEnd)
        {
          // Fade In.
          value = targetValue * (time - FadeInStart) / _fadeInDuration;
        }
        else if (time <= FadeOutStart)
        {
          value = targetValue;
        }
        else if (time < FadeOutEnd)
        {
          // Fade Out.
          value = targetValue * (1 - (time - FadeOutStart) / _fadeOutDuration);
        }
        else
        {
          value = 0;
        }

        Debug.Assert(value >= 0);
      }
      else
      {
        // ----- Case 2: Fade Out -> Fade In
        if (time <= FadeOutStart)
        {
          value = targetValue;
        }
        else if (time < FadeOutEnd)
        {
          // Fade Out.
          value = targetValue * (1 - (time - FadeOutStart) / _fadeOutDuration);
        }
        else if (time <= FadeInStart)
        {
          value = 0;
        }
        else if (time < FadeInEnd)
        {
          // Fade In.
          value = targetValue * (time - FadeInStart) / _fadeInDuration;
        }
        else
        {
          value = targetValue;
        }
      }

      return value;
    }
    #endregion
  }
}
