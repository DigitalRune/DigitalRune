// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Particles.Effectors
{

  /// <summary>
  /// Evaluates a custom (delegate) function for a single input particle parameter and stores the 
  /// result in another particle parameter.
  /// </summary>
  /// <typeparam name="T">The type of the function parameter.</typeparam>
  /// <typeparam name="TResult">The return type of the function.</typeparam>
  /// <remarks>
  /// <para>
  /// This effector executes a given delegate <see cref="Func"/> to compute the particle parameter 
  /// value. The delegate function takes a single parameter (<see cref="InputParameter"/>) and the 
  /// function result is stored in another parameter (<see cref="OutputParameter"/>).
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
  /// <term><see cref="InputParameter"/></term>
  /// <description>
  /// A <typeparamref name="T"/> parameter that provides the function parameter.
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
  public class FuncEffector<T, TResult> : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<T> _inputParameter;
    private IParticleParameter<TResult> _outputParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the input parameter.
    /// (A varying or uniform parameter of type <typeparamref name="T"/>.)
    /// </summary>
    /// <value>
    /// The name of the input parameter.
    /// (Parameter type: varying or uniform, value type: <typeparamref name="T"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string InputParameter { get; set; }


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
    public Func<T, TResult> Func { get; set; }
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
      return new FuncEffector<T, TResult>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone FuncEffector<T, TResult> properties.
      var sourceTyped = (FuncEffector<T, TResult>)source;
      InputParameter = sourceTyped.InputParameter;
      OutputParameter = sourceTyped.OutputParameter;
      Func = sourceTyped.Func;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _inputParameter = ParticleSystem.Parameters.Get<T>(InputParameter);
      _outputParameter = ParticleSystem.Parameters.Get<TResult>(OutputParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _inputParameter = null;
      _outputParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_inputParameter == null || _outputParameter == null || Func == null)
        return;

      if (_outputParameter.Values == null)
      {
        // Output is a uniform parameter.
        _outputParameter.DefaultValue = Func(_inputParameter.DefaultValue);
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_inputParameter == null || _outputParameter == null || Func == null)
      {
        return;
      }

      var outputs = _outputParameter.Values;
      if (outputs == null)
      {
        // Output is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      var inputs = _inputParameter.Values;
      if (inputs != null)
      {
        // Optimized case: Input is varying.
        for (int i = startIndex; i < startIndex + count; i++)
          outputs[i] = Func(inputs[i]);
      }
      else
      {
        // Optimized case: Input is uniform.
        var output = Func(_inputParameter.DefaultValue);
        for (int i = startIndex; i < startIndex + count; i++)
          outputs[i] = output;
      }
    }
    #endregion
  }
}
