// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to an array of values of a given type.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. Must be one of the following types: 
  /// <see cref="bool"/>, 
  /// <see cref="int"/>, 
  /// <see cref="Matrix"/>, 
  /// <see cref="Quaternion"/>, 
  /// <see cref="float"/>, 
  /// <see cref="Vector2"/>, 
  /// <see cref="Vector3"/>, 
  /// <see cref="Vector4"/>, 
  /// </typeparam>
  /// <inheritdoc cref="EffectParameterBinding{T}"/>
  public abstract class EffectParameterArrayBinding<T> : EffectParameterBinding
  {
    // Note: XNA does not support arrays of objects (Texture, string, etc.). 
    // Therefore we do not have to check for null values.
    //
    // T cannot be a DigitalRune type like Matrix44F because that would require
    // to copy the array each time the value is applied - and it could create garbage.

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
    /// The method that sets the effect parameter to the specified array of values.
    /// </summary>
    private static readonly Action<EffectParameter, T[]> SetValue;

    // ReSharper restore StaticFieldInGenericType
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets (or sets) the values of the effect parameter.
    /// </summary>
    /// <value>The values of the effect parameter.</value>
    /// <remarks>
    /// The values are computed in <see cref="EffectParameterBinding.Update"/> and applied to 
    /// <see cref="EffectParameterBinding.Parameter"/> when 
    /// <see cref="EffectParameterBinding.Apply"/> is called.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is required for EffectParameter.")]
    public T[] Values
    {
      get { return _values; }
      protected set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        // Smaller arrays are ok, for example: Setting 58 skinning matrices out of max 72.
        // Bigger arrays are not allowed.
        if (value.Length > Parameter.Elements.Count)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Length of the array ({0}) is greater than the number of elements of the effect parameter ({1}).",
            value.Length,
            Parameter.Elements.Count);
          throw new ArgumentException(message);
        }

        _values = value;
      }
    }
    private T[] _values;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="EffectParameterArrayBinding{T}"/> class.
    /// </summary>
    /// <exception cref="EffectBindingException">
    /// Effect parameters of type <typeparamref name="T"/> are not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static EffectParameterArrayBinding()
    {
      // Get "ValidateType" and "SetValue" methods.
      Type type = typeof(T);
      Func<EffectParameter, bool> validateType;
      Delegate setValueDelegate;
      if (!ValidateTypeMethods.TryGetValue(type, out validateType)
         || !SetValueArrayMethods.TryGetValue(type, out setValueDelegate))
      {
        var message = string.Format(CultureInfo.InvariantCulture, "Value type {0} is not supported by EffectParameterArrayBinding.", type.FullName);
        throw new EffectBindingException(message);
      }

      ValidateType = validateType;
      SetValue = (Action<EffectParameter, T[]>)setValueDelegate;
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterArrayBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterArrayBinding{T}"/> class.
    /// (This constructor creates an uninitialized instance. Use this constructor only for cloning 
    /// or other special cases!)
    /// </summary>
    protected EffectParameterArrayBinding()
    {
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterArrayBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterArrayBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="parameter"/> has more elements than is supported by the effect parameter.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported or does not match the effect
    /// parameter.
    /// </exception>
    protected EffectParameterArrayBinding(Effect effect, EffectParameter parameter)
      : this(effect, parameter, default(T))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterArrayBinding{T}"/> class with 
    /// the given value.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="value">The initial value for all elements of the value array.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported or does not match the effect
    /// parameter.
    /// </exception>
    protected EffectParameterArrayBinding(Effect effect, EffectParameter parameter, T value)
      : this(effect, parameter, ToArray(value, parameter))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterArrayBinding{T}"/> class with 
    /// the given array of values.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="values">The initial values.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/>, <paramref name="parameter"/>, or <paramref name="values"/> is 
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported or does not match the effect
    /// parameter.
    /// </exception>
    protected EffectParameterArrayBinding(Effect effect, EffectParameter parameter, T[] values)
      : base(effect, parameter)
    {
      if (values == null)
        throw new ArgumentNullException("values");

      ThrowIfNotArray(effect, Description);
      ThrowIfInvalidType<T>(effect, Description, ValidateType, values.Length);

      Values = values;
    }


    private static T[] ToArray(T value, EffectParameter parameter)
    {
      if (parameter == null)
        return new T[0];

      int numberOfElements = parameter.Elements.Count;
      T[] values = new T[numberOfElements];
      for (int i = 0; i < numberOfElements; i++)
        values[i] = value;

      return values;
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

      // Clone EffectParameterArrayBinding<T> properties.
      var sourceTyped = (EffectParameterArrayBinding<T>)source;
      Values = (T[])sourceTyped.Values.Clone();
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
    /// Called when the effect parameter needs to be applied.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Derived classes need to call <see cref="OnApply"/> of
    /// the base class to ensure that the effect parameter value is properly set.
    /// </remarks>
    protected override void OnApply(RenderContext context)
    {
      SetValue(Parameter, _values);
    }
    #endregion
  }
}
