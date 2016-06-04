// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to a value of a given type.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. Must be one of the following types: 
  /// <see cref="bool"/>, 
  /// <see cref="int"/>, 
  /// <see cref="Matrix"/>, 
  /// <see cref="Matrix44F"/>, 
  /// <see cref="Quaternion"/>, 
  /// <see cref="float"/>, 
  /// <see cref="string"/>, 
  /// <see cref="Texture"/>, 
  /// <see cref="Texture2D"/>, 
  /// <see cref="Texture3D"/>, 
  /// <see cref="TextureCube"/>, 
  /// <see cref="Vector2"/>, 
  /// <see cref="Vector3"/>, 
  /// <see cref="Vector4"/>, 
  /// <see cref="Vector2F"/>, 
  /// <see cref="Vector3F"/>, 
  /// <see cref="Vector4F"/>.
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// <see cref="EffectParameterBinding{T}"/> and <see cref="EffectParameterArrayBinding{T}"/> are
  /// the base implementations of all parameter bindings of type <typeparamref name="T"/>. Classes
  /// derived from <see cref="EffectParameterBinding{T}"/> are used to bind effect parameters that 
  /// represent a single value (such as <see cref="float"/>, <see cref="Vector3"/>, etc.). Classes
  /// that are derived from <see cref="EffectParameterArrayBinding{T}"/> are used to bind effect 
  /// parameters that represent arrays of values.
  /// </para>
  /// <para>
  /// <see cref="ConstParameterBinding{T}"/> and <see cref="ConstParameterArrayBinding{T}"/> should 
  /// be used when the value of the effect parameter is static or updated manually. The value can be
  /// set directly. The binding ensures that the parameter is applied whenever the 
  /// <see cref="Effect"/> needs to be rendered.
  /// </para>
  /// <para>
  /// <see cref="DelegateParameterBinding{T}"/> and <see cref="DelegateParameterArrayBinding{T}"/> 
  /// need to be used when the value of the effect parameter is dynamic and should be updated
  /// automatically.
  /// </para>
  /// <para>
  /// <see cref="NullParameterBinding{T}"/> and <see cref="NullParameterArrayBinding{T}"/> are dummy
  /// bindings that do not modify effect parameters. These bindings can be set, if parameters should
  /// not be updated automatically. 
  /// </para>
  /// <para>
  /// User-defined bindings can be implemented by deriving from 
  /// <see cref="EffectParameterBinding"/>, <see cref="EffectParameterBinding{T}"/>, or 
  /// <see cref="EffectParameterArrayBinding{T}"/> to define new mechanisms for resolving effect 
  /// parameter values.
  /// </para>
  /// <para>
  /// <strong>Notes to Inheritors:</strong><br/>
  /// The static constructors of <see cref="EffectParameterBinding{T}"/> and 
  /// <see cref="EffectParameterArrayBinding{T}"/> automatically validate the value type 
  /// <typeparamref name="T"/> and checks whether the type is supported.
  /// </para>
  /// <para>
  /// Inheritors should override <see cref="OnUpdate"/> when the value of the effect parameter needs
  /// to be updated before rendering. The new value computed in <see cref="OnUpdate"/> must be 
  /// stored in <see cref="Value"/>. (It is not necessary to call <see cref="OnUpdate"/> of the base
  /// class in the derived class.)
  /// </para>
  /// <para>
  /// By default, <see cref="OnApply"/> assigns the current <see cref="Value"/> to 
  /// <see cref="EffectParameterBinding.Parameter"/> by using the appropriate 
  /// <strong>EffectParameter.SetValue()</strong> method. Inheritors can override this method to 
  /// perform additional work. It is necessary to call <see cref="OnApply"/> of the base class to 
  /// ensure that the value is properly set.
  /// </para>
  /// </remarks>
  public abstract class EffectParameterBinding<T> : EffectParameterBinding
  {
    // Not yet supported value types:
    // <see cref="Matrix22F"/>, 
    // <see cref="Matrix33F"/>

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType

    /// <summary>
    /// The method that checks whether the type of the effect parameter binding is compatible with
    /// the specified effect parameter.
    /// </summary>
    private static readonly Func<EffectParameter, bool> ValidateType;


    /// <summary>
    /// The method that sets the effect parameter to the specified value.
    /// </summary>
    private static readonly Action<EffectParameter, T> SetValue;


    /// <summary>
    /// An equality comparer for <typeparamref name="T"/>.
    /// </summary>
    internal static readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default;

    // ReSharper restore StaticFieldInGenericType
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets (or sets) the value of the effect parameter.
    /// </summary>
    /// <value>The value of the effect parameter.</value>
    /// <remarks>
    /// The value is computed in <see cref="EffectParameterBinding.Update"/> and applied to 
    /// <see cref="EffectParameterBinding.Parameter"/> when 
    /// <see cref="EffectParameterBinding.Apply"/> is called.
    /// </remarks>
    public T Value
    {
      get { return _value; }
      protected set
      {
        // ReSharper disable CompareNonConstrainedGenericWithNull
        //if (Parameter.ParameterClass == EffectParameterClass.Object && value == null)   // Removed because unused parameters can be null.
        //  throw new ArgumentNullException("value");
        // ReSharper restore CompareNonConstrainedGenericWithNull

        _value = value;
      }
    }
    private T _value;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="EffectParameterBinding{T}"/> class.
    /// </summary>
    /// <exception cref="EffectBindingException">
    /// Effect parameters of type <typeparamref name="T"/> are not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static EffectParameterBinding()
    {
      // Get "ValidateType" and "SetValue" methods.
      Type type = typeof(T);
      Func<EffectParameter, bool> validateType;
      Delegate setValueDelegate;
      if (!ValidateTypeMethods.TryGetValue(type, out validateType)
          || !SetValueMethods.TryGetValue(type, out setValueDelegate))
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Value type {0} is not supported by EffectParameterBinding.", type.FullName);
        throw new EffectBindingException(message);
      }

      ValidateType = validateType;
      SetValue = (Action<EffectParameter, T>)setValueDelegate;
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding{T}"/> class.
    /// (This constructor creates an uninitialized instance. Use this constructor only for cloning 
    /// or other special cases!)
    /// </summary>
    protected EffectParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported or does not match the effect
    /// parameter.
    /// </exception>
    protected EffectParameterBinding(Effect effect, EffectParameter parameter)
      : this(effect, parameter, default(T))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBinding"/> class with the given 
    /// value.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="value">The initial value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported or does not match the effect
    /// parameter.
    /// </exception>
    protected EffectParameterBinding(Effect effect, EffectParameter parameter, T value)
      : base(effect, parameter)
    {
      ThrowIfArray(effect, Description);
      ThrowIfInvalidType<T>(effect, Description, ValidateType, 0);

      _value = value;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterBinding properties.
      base.CloneCore(source);

      // Clone EffectParameterBinding<T> properties.
      var sourceTyped = (EffectParameterBinding<T>)source;
      Value = sourceTyped.Value;
    }
    #endregion


    /// <summary>
    /// Called when the effect parameter value needs to be updated.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> It is not necessary to call <see cref="OnUpdate"/> of 
    /// the base class in derived classes.
    /// </remarks>
    protected override void OnUpdate(RenderContext context)
    {
    }


    /// <summary>
    /// Called when the effect parameter value needs to be applied.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Derived classes need to call <see cref="OnApply"/> of
    /// the base class to ensure that the effect parameter value is properly set.
    /// </remarks>
    protected override void OnApply(RenderContext context)
    {
      SetValue(Parameter, Value);
    }
    #endregion
  }
}
