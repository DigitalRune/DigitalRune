// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to an array of values.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
  /// </typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Value = {Values})")]
  public class ConstParameterArrayBinding<T> : EffectParameterArrayBinding<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the values of the effect parameter.
    /// </summary>
    /// <value>The values of the effect parameter.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is required for EffectParameter.")]
    public new T[] Values
    {
      get { return base.Values; }
      set { base.Values = value; }  // Same as in base class, but this time the setter is public.
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstParameterArrayBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstParameterArrayBinding{T}"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or 
    /// other special cases!)
    /// </summary>
    protected ConstParameterArrayBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConstParameterArrayBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="values">The array of values.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/>, <paramref name="parameter"/>, or <paramref name="values"/> is 
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of elements in <paramref name="values"/> does not match the number of elements of
    /// <paramref name="parameter"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="parameter"/> does not represent an array of values.
    /// </exception>
    public ConstParameterArrayBinding(Effect effect, EffectParameter parameter, T[] values)
      : base(effect, parameter, values)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new ConstParameterArrayBinding<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterArrayBinding<T> properties.
      base.CloneCore(source);

      // Clone ConstParameterArrayBinding<T> properties.
      var sourceTyped = (ConstParameterArrayBinding<T>)source;
      Values = (T[])sourceTyped.Values.Clone();
    }
    #endregion

    #endregion
  }
}
