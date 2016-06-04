// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System;
//using System.Collections.Generic;
//using DigitalRune.Mathematics.Interpolation;


//namespace DigitalRune.Particles.Effectors
//{
//  /// <summary>
//  /// Evaluates a piecewise curve (<see cref="Curve2F"/>) and multiplies the result with a 
//  /// particle parameter.
//  /// </summary>
//  /// <remarks>
//  /// <para>
//  /// All parameters must be of type <see cref="float"/>.
//  /// </para>
//  /// <para>
//  /// This effector evaluates the given <see cref="Curve"/> using the <see cref="TimeParameter"/>.
//  /// The result is multiplied with the <see cref="InputParameter"/> (optional) and stored in
//  /// the <see cref="OutputParameter"/>.
//  /// </para>
//  /// <para>
//  /// This effector is significantly slower than other effectors because if <see cref="TimeParameter"/>
//  /// is a varying parameter, the curve is evaluated for each particle. If possible, replace this
//  /// effector with a simpler effector, for instance, the <see cref="SingleLinearSegment3Effector"/>.
//  /// </para>
//  /// <para>
//  /// Used particle parameters:
//  /// <list type="table">
//  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
//  /// <item>
//  /// <term><see cref="TimeParameter"/></term>
//  /// <description>
//  /// A <see cref="float"/> parameter that defines curve parameter (= the position where the
//  /// curve is evaluated).
//  /// Per default, the parameter "NormalizedAge" is used.
//  /// </description>
//  /// </item>
//  /// <item>
//  /// <term><see cref="InputParameter"/></term>
//  /// <description>
//  /// The <see cref="float"/> parameter that is multiplied with the curve value.
//  /// </description>
//  /// </item>
//  /// <item>
//  /// <term><see cref="TimeParameter"/></term>
//  /// <description>
//  /// The <see cref="float"/> parameter that stores the result of the function value multiplied
//  /// with the <see cref="InputParameter"/>.
//  /// </description>
//  /// </item>
//  /// </list>
//  /// </para>
//  /// <para>
//  /// <strong>Cloning:</strong> When an instance is of this class is cloned, the clone references 
//  /// the same <see cref="Curve"/>. The <see cref="Curve"/> is not cloned.
//  /// </para>
//  /// </remarks>
//  public class CurveEffector : ParticleEffector
//  {
//    //--------------------------------------------------------------
//    #region Fields
//    //--------------------------------------------------------------

//    private IParticleParameter<float> _inputParameter;
//    private IParticleParameter<float> _outputParameter;
//    private IParticleParameter<float> _timeParameter;
//    #endregion


//    //--------------------------------------------------------------
//    #region Properties & Events
//    //--------------------------------------------------------------

//    /// <summary>
//    /// Gets or sets the name of the parameter that is multiplied with curve value.
//    /// (A varying or uniform parameter of type <see cref="float"/>.)
//    /// </summary>
//    /// <value>
//    /// The name of the parameter that is multiplied with the curve value.
//    /// This parameter is optional.
//    /// (Parameter type: varying or uniform, value type: <see cref="float"/>)
//    /// The default value is <see langword="null"/>.
//    /// </value>
//    /// <remarks>
//    /// This property needs to be set before the particle system is running. The particle effector 
//    /// might ignore any changes that occur while the particle system is running.
//    /// </remarks>
//    public string InputParameter { get; set; }


//    /// <summary>
//    /// Gets or sets the name of the parameter that stores the result.
//    /// (A varying or uniform parameter of type <see cref="float"/>.)
//    /// </summary>
//    /// <value>
//    /// The name of the parameter that stores the result.
//    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
//    /// The default value is <see langword="null"/>.
//    /// </value>
//    /// <remarks>
//    /// This property needs to be set before the particle system is running. The particle effector 
//    /// might ignore any changes that occur while the particle system is running.
//    /// </remarks>
//    public string OutputParameter { get; set; }


//    /// <summary>
//    /// Gets or sets the name of the parameter that defines the curve parameter (= the position
//    /// where the curve is evaluated).
//    /// (A varying or uniform parameter of type <see cref="float"/>.)
//    /// </summary>
//    /// <value>
//    /// The name of the parameter that defines the curve parameter.
//    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
//    /// The default value is "NormalizedAge".
//    /// </value>
//    /// <remarks>
//    /// This property needs to be set before the particle system is running. The particle effector 
//    /// might ignore any changes that occur while the particle system is running.
//    /// </remarks>
//    public string TimeParameter { get; set; }


//    /// <summary>
//    /// Gets or sets the curve.
//    /// </summary>
//    /// <value>The curve.</value>
//    public Curve2F Curve
//    {
//      get { return _curve; }
//      set { _curve = value; }
//    }
//    private Curve2F _curve;


//    /// <inheritdoc/>
//    public override IEnumerable<string> InputParameters
//    {
//      get
//      {
//        yield return InputParameter;
//        yield return TimeParameter;
//      }
//    }


//    /// <inheritdoc/>
//    public override IEnumerable<string> OutputParameters
//    {
//      get { yield return OutputParameter; }
//    }
//    #endregion


//    //--------------------------------------------------------------
//    #region Creation & Cleanup
//    //--------------------------------------------------------------

//    /// <summary>
//    /// Initializes a new instance of the <see cref="CurveEffector"/> class.
//    /// </summary>
//    public CurveEffector()
//    {
//      TimeParameter = ParticleParameterNames.NormalizedAge;
//    }
//    #endregion


//    //--------------------------------------------------------------
//    #region Methods
//    //--------------------------------------------------------------

//    /// <inheritdoc/>
//    protected override ParticleEffector CreateInstanceCore()
//    {
//      return new CurveEffector();
//    }


//    /// <inheritdoc/>
//    protected override void CloneCore(ParticleEffector source)
//    {
//      // Clone ParticleEffector properties.
//      base.CloneCore(source);

//      // Clone CurveEffector properties.
//      var sourceTyped = (CurveEffector)source;
//      InputParameter = sourceTyped.InputParameter;
//      OutputParameter = sourceTyped.OutputParameter;
//      TimeParameter = sourceTyped.TimeParameter;
//      Curve = sourceTyped.Curve;
//    }


//    /// <inheritdoc/>
//    protected override void OnRequeryParameters()
//    {
//      _inputParameter = ParticleSystem.Parameters.Get<float>(InputParameter);
//      _outputParameter = ParticleSystem.Parameters.Get<float>(OutputParameter);
//      _timeParameter = ParticleSystem.Parameters.Get<float>(TimeParameter);
//    }


//    /// <inheritdoc/>
//    protected override void OnUninitialize()
//    {
//      _inputParameter = null;
//      _outputParameter = null;
//      _timeParameter = null;
//    }


//    /// <inheritdoc/>
//    protected override void OnBeginUpdate(TimeSpan deltaTime)
//    {
//      if (_outputParameter == null
//          || _timeParameter == null
//          || _curve == null)
//      {
//        return;
//      }

//      float[] values = _outputParameter.Values;
//      if (values == null)
//      {
//        // Value is a uniform parameter.
//        var t = _timeParameter.DefaultValue;
//        float y = _curve.GetPoint(t).Y;

//        if (_inputParameter != null)
//          y = y * _inputParameter.DefaultValue;

//        _outputParameter.DefaultValue = y;
//      }
//    }


//    /// <inheritdoc/>
//    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
//    {
//      if (_outputParameter == null
//          || _timeParameter == null
//          || _curve == null)
//      {
//        return;
//      }

//      float[] values = _outputParameter.Values;
//      if (values == null)
//      {
//        // Value is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
//        return;
//      }

//      // Value is a varying parameter.
//      float[] times = _timeParameter.Values;
//      float[] inputs = null;
//      float defaultInput = 1;
//      if (_inputParameter != null)
//      {
//        inputs = _inputParameter.Values;
//        defaultInput = _inputParameter.DefaultValue;
//      }

//      if (times != null && inputs != null)
//      {
//        // Optimized case: All varying parameters.
//        for (int i = startIndex; i < startIndex + count; i++)
//        {
//          var t = times[i];
//          float y = _curve.GetPoint(t).Y;
//          values[i] = y * inputs[i];
//        }
//      }
//      else if (times != null)
//      {
//        // Optimized case: Time is varying. Input is uniform.
//        for (int i = startIndex; i < startIndex + count; i++)
//        {
//          var t = times[i];
//          float y = _curve.GetPoint(t).Y;
//          values[i] = y * defaultInput;
//        }
//      }
//      else if (inputs != null)
//      {
//        // Optimized case: All time is uniform. Input is varying.
//        var t = _timeParameter.DefaultValue;
//        float y = _curve.GetPoint(t).Y;
//        for (int i = startIndex; i < startIndex + count; i++)
//          values[i] = y * inputs[i];
//      }
//      else
//      {
//        // Optimized case: Time and Input are uniform.
//        var t = _timeParameter.DefaultValue;
//        float y = _curve.GetPoint(t).Y;
//        y = y * defaultInput;
        
//        for (int i = startIndex; i < startIndex + count; i++)
//          values[i] = y;
//      }
//    }
//    #endregion
//  }
//}
