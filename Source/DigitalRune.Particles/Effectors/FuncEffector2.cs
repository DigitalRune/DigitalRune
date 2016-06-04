// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Particles.Effectors
{

  /// <summary>
  /// Evaluates a custom (delegate) function for two input particle parameters and stores the result
  /// in another particle parameter.
  /// </summary>
  /// <typeparam name="T1">The type of the first function parameter.</typeparam>
  /// <typeparam name="T2">The type of the second function parameter.</typeparam>
  /// <typeparam name="TResult">The return type of the function.</typeparam>
  /// <remarks>
  /// <para>
  /// This effector executes a given delegate <see cref="Func"/> to compute the particle 
  /// parameter value. The delegate function takes a two parameters (see 
  /// <see cref="InputParameter1"/> and <see cref="InputParameter2"/>) and the function result is 
  /// stored in another parameter (see <see cref="OutputParameter"/>).
  /// </para>
  /// <para>
  /// Please note: Since this effector executes a delegate call for each particle, it is less 
  /// efficient than other particle effectors.
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="InputParameter1"/></term>
  /// <description>
  /// A <typeparamref name="T1"/> parameter that provides the first function parameter.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="InputParameter2"/></term>
  /// <description>
  /// A <typeparamref name="T2"/> parameter that provides the second function parameter.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="OutputParameter"/></term>
  /// <description>
  /// A <typeparamref name="TResult"/> parameter that stores the result of the function.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
  public class FuncEffector<T1, T2, TResult> : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<T1> _inputParameter1;
    private IParticleParameter<T2> _inputParameter2;
    private IParticleParameter<TResult> _outputParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the first input parameter.
    /// (A varying or uniform parameter of type <typeparamref name="T1"/>.)
    /// </summary>
    /// <value>
    /// The name of the first input parameter.
    /// (Parameter type: varying or uniform, value type: <typeparamref name="T1"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string InputParameter1 { get; set; }


    /// <summary>
    /// Gets or sets the name of the second input parameter.
    /// (A varying or uniform parameter of type <typeparamref name="T2"/>.)
    /// </summary>
    /// <value>
    /// The name of the second input parameter.
    /// (Parameter type: varying or uniform, value type: <typeparamref name="T2"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string InputParameter2 { get; set; }


    /// <summary>
    /// Gets or sets the name of the output parameter.
    /// (A varying or uniform parameter of type <typeparamref name="TResult"/>.)
    /// </summary>
    /// <value>
    /// The name of the output parameter.
    /// (Parameter type: varying or uniform, value type: <typeparamref name="TResult"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.Out)]
    public string OutputParameter { get; set; }


    /// <summary>
    /// Gets or sets the delegate function.
    /// </summary>
    /// <value>The delegate function.</value>
    public Func<T1, T2, TResult> Func { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new FuncEffector<T1, T2, TResult>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone FuncEffector<T1, T2, TResult> properties.
      var sourceTyped = (FuncEffector<T1, T2, TResult>)source;
      InputParameter1 = sourceTyped.InputParameter1;
      InputParameter2 = sourceTyped.InputParameter2;
      OutputParameter = sourceTyped.OutputParameter;
      Func = sourceTyped.Func;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _inputParameter1 = ParticleSystem.Parameters.Get<T1>(InputParameter1);
      _inputParameter2 = ParticleSystem.Parameters.Get<T2>(InputParameter2);
      _outputParameter = ParticleSystem.Parameters.Get<TResult>(OutputParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _inputParameter1 = null;
      _inputParameter2 = null;
      _outputParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_inputParameter1 == null
          || _inputParameter2 == null
          || _outputParameter == null
          || Func == null)
      {
        return;
      }

      if (_outputParameter.Values == null)
      {
        // Output is a uniform parameter.
        _outputParameter.DefaultValue = Func(_inputParameter1.DefaultValue, _inputParameter2.DefaultValue);
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_inputParameter1 == null
          || _inputParameter2 == null
          || _outputParameter == null
          || Func == null)
      {
        return;
      }

      var outputs = _outputParameter.Values;
      if (outputs == null)
      {
        // Output is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      var inputs1 = _inputParameter1.Values;
      var inputs2 = _inputParameter2.Values;
      if (inputs1 != null && inputs2 != null)
      {
        // Optimized case: Input1 and Input2 are varying.
        for (int i = startIndex; i < startIndex + count; i++)
          outputs[i] = Func(inputs1[i], inputs2[i]);
      }
      else if (inputs2 != null)
      {
        // Optimized case: Input2 is varying.
        var input1 = _inputParameter1.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
          outputs[i] = Func(input1, inputs2[i]);
      }
      else if (inputs1 != null)
      {
        // Optimized case: Input1 is varying.
        var input2 = _inputParameter2.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
          outputs[i] = Func(inputs1[i], input2);
      }
      else
      {
        // Optimized case: Input1 and Input2 are uniform.
        var output = Func(_inputParameter1.DefaultValue, _inputParameter2.DefaultValue);
        for (int i = startIndex; i < startIndex + count; i++)
          outputs[i] = output;
      }
    }
    #endregion
  }
}
