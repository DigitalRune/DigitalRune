// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Evaluates 3 piecewise linear functions consisting of 3 segments and multiplies the result with
  /// a particle parameter. (This effector is the <see cref="Vector3F"/> version of the 
  /// <see cref="SingleLinearSegment3Effector"/>.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector is similar to the <see cref="SingleLinearSegment3Effector"/>, only that a
  /// piecewise linear function is evaluated for each component of a <see cref="Vector3F"/>.
  /// </para>
  /// <para>
  /// A piecewise linear function is defined using several data points:
  /// (<see cref="Time0"/>, <see cref="Value0"/>), (<see cref="Time1"/>, <see cref="Value1"/>),
  /// (<see cref="Time2"/>, <see cref="Value2"/>) and (<see cref="Time3"/>, <see cref="Value3"/>).
  /// If the time (see <see cref="TimeParameter"/>) is less than <see cref="Time0"/>, then the
  /// function value is <see cref="Value0"/>. If the time is greater than <see cref="Time3"/>, then 
  /// the function value is <see cref="Value3"/>. If the time is between those limits, the function
  /// value is the interpolation of the nearest data point. The time values <see cref="Time0"/> to 
  /// <see cref="Time3"/> must be given in ascending order.
  /// </para>
  /// <para>
  /// The result of the function is muliplied with the <see cref="InputParameter"/> (component-wise)
  /// and stored in the <see cref="OutputParameter"/>. (If <see cref="InputParameter"/> is not set, 
  /// then the function value is directly stored in the <see cref="OutputParameter"/>.)
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="TimeParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that is the input to the piecewise linear function.
  /// Per default, the parameter "NormalizedAge" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="InputParameter"/></term>
  /// <description>
  /// The <see cref="Vector3F"/> parameter that is multiplied with the function value.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="TimeParameter"/></term>
  /// <description>
  /// The <see cref="Vector3F"/> parameter that stores the result of the function value multiplied
  /// with the <see cref="InputParameter"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class Vector3FLinearSegment3Effector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<Vector3F> _inputParameter;
    private IParticleParameter<Vector3F> _outputParameter;
    private IParticleParameter<float> _timeParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the parameter that is multiplied with the value of the piecewise 
    /// linear function. (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that is multiplied with the value of the piecewise linear 
    /// function. This parameter is optional.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In, Optional = true)]
    public string InputParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that stores the result.
    /// (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that stores the result.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.Out)]
    public string OutputParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that is the input for the piecewise linear function.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that is the input for the piecewise linear function.
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
    /// Gets or sets the time of the first data point.
    /// </summary>
    /// <value>The time of the first data point.</value>
    public float Time0
    {
      get { return _time0; }
      set { _time0 = value; }
    }
    private float _time0;


    /// <summary>
    /// Gets or sets the value of the first data point.
    /// </summary>
    /// <value>The value of the first data point.</value>
    public Vector3F Value0
    {
      get { return _value0; }
      set { _value0 = value; }
    }
    private Vector3F _value0;


    /// <summary>
    /// Gets or sets the time of the second data point.
    /// </summary>
    /// <value>The time of the second data point.</value>
    public float Time1
    {
      get { return _time1; }
      set { _time1 = value; }
    }
    private float _time1;


    /// <summary>
    /// Gets or sets the value of the second data point.
    /// </summary>
    /// <value>The value of the second data point.</value>
    public Vector3F Value1
    {
      get { return _value1; }
      set { _value1 = value; }
    }
    private Vector3F _value1;


    /// <summary>
    /// Gets or sets the time of the third data point.
    /// </summary>
    /// <value>The time of the third data point.</value>
    public float Time2
    {
      get { return _time2; }
      set { _time2 = value; }
    }
    private float _time2;


    /// <summary>
    /// Gets or sets the value of the third data point.
    /// </summary>
    /// <value>The value of the third data point.</value>
    public Vector3F Value2
    {
      get { return _value2; }
      set { _value2 = value; }
    }
    private Vector3F _value2;


    /// <summary>
    /// Gets or sets the time of the last data point.
    /// </summary>
    /// <value>The time of the last data point.</value>
    public float Time3
    {
      get { return _time3; }
      set { _time3 = value; }
    }
    private float _time3;


    /// <summary>
    /// Gets or sets the value of the last data point.
    /// </summary>
    /// <value>The value of the last data point.</value>
    public Vector3F Value3
    {
      get { return _value3; }
      set { _value3 = value; }
    }
    private Vector3F _value3;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3FLinearSegment3Effector"/> class.
    /// </summary>
    public Vector3FLinearSegment3Effector()
    {
      TimeParameter = ParticleParameterNames.NormalizedAge;
      Time3 = 1;
      Value3 = new Vector3F(1);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new Vector3FLinearSegment3Effector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone Vector3FLinearSegment3Effector properties.
      var sourceTyped = (Vector3FLinearSegment3Effector)source;
      InputParameter = sourceTyped.InputParameter;
      OutputParameter = sourceTyped.OutputParameter;
      TimeParameter = sourceTyped.TimeParameter;
      Time0 = sourceTyped.Time0;
      Value0 = sourceTyped.Value0;
      Time1 = sourceTyped.Time1;
      Value1 = sourceTyped.Value1;
      Time2 = sourceTyped.Time2;
      Value2 = sourceTyped.Value2;
      Time3 = sourceTyped.Time3;
      Value3 = sourceTyped.Value3;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _inputParameter = ParticleSystem.Parameters.Get<Vector3F>(InputParameter);
      _outputParameter = ParticleSystem.Parameters.Get<Vector3F>(OutputParameter);
      _timeParameter = ParticleSystem.Parameters.Get<float>(TimeParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _inputParameter = null;
      _outputParameter = null;
      _timeParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_outputParameter == null || _timeParameter == null)
        return;

      Vector3F[] values = _outputParameter.Values;
      if (values == null)
      {
        // Value is a uniform parameter.
        var t = _timeParameter.DefaultValue;
        Vector3F y;

        if (t <= _time0)
          y = _value0;
        else if (t <= _time1)
          y = _value0 + (_value1 - _value0) * (t - _time0) / (_time1 - _time0);
        else if (t <= _time2)
          y = _value1 + (_value2 - _value1) * (t - _time1) / (_time2 - _time1);
        else if (t < _time3)
          y = _value2 + (_value3 - _value2) * (t - _time2) / (_time3 - _time2);
        else
          y = _value3;
        
        if (_inputParameter != null)
          y = y * _inputParameter.DefaultValue;

        _outputParameter.DefaultValue = y;
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_outputParameter == null || _timeParameter == null)
        return;

      Vector3F[] values = _outputParameter.Values;
      if (values == null)
      {
        // Value is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      // Value is a varying parameter.
      float[] times = _timeParameter.Values;
      Vector3F[] inputs = null;
      Vector3F defaultInput = new Vector3F(1);
      if (_inputParameter != null)
      {
        inputs = _inputParameter.Values;
        defaultInput = _inputParameter.DefaultValue;
      }

      // Inverse time deltas.
      var dt10 = 1 / (_time1 - _time0);
      var dt21 = 1 / (_time2 - _time1);
      var dt32 = 1 / (_time3 - _time2);

      // Value deltas.
      var dv10 = _value1 - _value0;
      var dv21 = _value2 - _value1;
      var dv32 = _value3 - _value2;

      if (times != null && inputs != null)
      {
        // Optimized case: All varying parameters.
        for (int i = startIndex; i < startIndex + count; i++)
        {
          var t = times[i];

          Vector3F y;
          if (t <= _time0)
            y = _value0;
          else if (t <= _time1)
            y = _value0 + dv10 * (t - _time0) * dt10;
          else if (t <= _time2)
            y = _value1 + dv21 * (t - _time1) * dt21;
          else if (t < _time3)
            y = _value2 + dv32 * (t - _time2) * dt32;
          else
            y = _value3;

          values[i] = y * inputs[i];
        }
      }
      else if (times != null)
      {
        // Optimized case: Time is varying. Input is uniform.
        for (int i = startIndex; i < startIndex + count; i++)
        {
          var t = times[i];

          Vector3F y;
          if (t <= _time0)
            y = _value0;
          else if (t <= _time1)
            y = _value0 + dv10 * (t - _time0) * dt10;
          else if (t <= _time2)
            y = _value1 + dv21 * (t - _time1) * dt21;
          else if (t < _time3)
            y = _value2 + dv32 * (t - _time2) * dt32;
          else
            y = _value3;

          values[i] = y * defaultInput;
        }
      }
      else if (inputs != null)
      {
        // Optimized case: All time is uniform. Input is varying.
        var t = _timeParameter.DefaultValue;
        Vector3F y;
        if (t <= _time0)
          y = _value0;
        else if (t <= _time1)
          y = _value0 + dv10 * (t - _time0) * dt10;
        else if (t <= _time2)
          y = _value1 + dv21 * (t - _time1) * dt21;
        else if (t < _time3)
          y = _value2 + dv32 * (t - _time2) * dt32;
        else
          y = _value3;
        
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = y * inputs[i];
      }
      else
      {
        // Optimized case: Time and Input are uniform.
        var t = _timeParameter.DefaultValue;
        Vector3F y;
        if (t <= _time0)
          y = _value0;
        else if (t <= _time1)
          y = _value0 + dv10 * (t - _time0) * dt10;
        else if (t <= _time2)
          y = _value1 + dv21 * (t - _time1) * dt21;
        else if (t < _time3)
          y = _value2 + dv32 * (t - _time2) * dt32;
        else
          y = _value3;

        y = y * defaultInput;


        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = y;
      }
    }
    #endregion
  }
}
