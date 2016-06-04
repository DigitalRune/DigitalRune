// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to a value computed by a callback method.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. See <see cref="EffectParameterBinding{T}"/>.
  /// </typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Value = {Value})")]
  public class DelegateParameterBinding<T> : EffectParameterBinding<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Get or sets a method that computes the new value for the effect parameter.
    /// </summary>
    /// <value>The method that computes the new value for the effect parameter.</value>
    /// <remarks>
    /// The input parameters of this method are the <see cref="DelegateParameterBinding{T}"/> and 
    /// the render context. The method must return the new parameter value.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public Func<DelegateParameterBinding<T>, RenderContext, T> ComputeParameter
    {
      get { return _computeParameter; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _computeParameter = value;
      }
    }
    private Func<DelegateParameterBinding<T>, RenderContext, T> _computeParameter;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateParameterBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateParameterBinding{T}"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected DelegateParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateParameterBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="computeParameter">The callback method that computes the value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/>, <paramref name="parameter"/>, or 
    /// <paramref name="computeParameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegateParameterBinding(Effect effect,
                                    EffectParameter parameter,
                                    Func<DelegateParameterBinding<T>, RenderContext, T> computeParameter)
      : base(effect, parameter)
    {
      if (computeParameter == null)
        throw new ArgumentNullException("computeParameter");

      _computeParameter = computeParameter;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new DelegateParameterBinding<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterBinding<T> properties.
      base.CloneCore(source);

      // Clone DelegateParameterBinding<T> properties.
      var sourceTyped = (DelegateParameterBinding<T>)source;
      ComputeParameter = sourceTyped.ComputeParameter;
    }
    #endregion



    /// <summary>
    /// Called when the effect parameter value needs to be updated.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="OnUpdate"/> calls 
    /// <see cref="ComputeParameter"/> and stores the result in 
    /// <see cref="EffectParameterBinding{T}.Value"/>. Derived classes should either call
    /// <see cref="ComputeParameter"/> directly or call <see cref="OnUpdate"/> of the base class.
    /// </remarks>
    protected override void OnUpdate(RenderContext context)
    {
      Value = ComputeParameter(this, context);
    }
    #endregion
  }
}
