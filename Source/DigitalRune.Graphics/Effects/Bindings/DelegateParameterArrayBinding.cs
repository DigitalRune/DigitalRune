// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to an array of values computed by a callback method.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
  /// </typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Values = {Values})")]
  public class DelegateParameterArrayBinding<T> : EffectParameterArrayBinding<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a method that computes the new values for the effect parameter.
    /// </summary>
    /// <value>The method that computes the new values for the effect parameter.</value>
    /// <remarks>
    /// The input parameters of this method are the <see cref="DelegateParameterArrayBinding{T}"/>, 
    /// the render context, and the array of values. The method has to update the values in the 
    /// pre-allocated array.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public Action<DelegateParameterArrayBinding<T>, RenderContext, T[]> ComputeParameter
    {
      get { return _computeParameter; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _computeParameter = value;
      }
    }
    private Action<DelegateParameterArrayBinding<T>, RenderContext, T[]> _computeParameter;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateParameterArrayBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateParameterArrayBinding{T}"/> class.
    /// (This constructor creates an uninitialized instance. Use this constructor only for cloning 
    /// or other special cases!)
    /// </summary>
    protected DelegateParameterArrayBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateParameterArrayBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="computeParameter">The callback method that computes the values.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/>, <paramref name="parameter"/>, or 
    /// <paramref name="computeParameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="parameter"/> does not represent an array of values.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegateParameterArrayBinding(Effect effect,
                                         EffectParameter parameter,
                                         Action<DelegateParameterArrayBinding<T>, RenderContext, T[]> computeParameter)
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
      return new DelegateParameterArrayBinding<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterArrayBinding<T> properties.
      base.CloneCore(source);

      // Clone DelegateParameterArrayBinding<T> properties.
      var sourceTyped = (DelegateParameterArrayBinding<T>)source;
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
      ComputeParameter(this, context, Values);
    }
    #endregion
  }
}
