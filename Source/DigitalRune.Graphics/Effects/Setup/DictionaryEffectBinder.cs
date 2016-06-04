// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Creates bindings for effect parameters using dictionaries with factory methods.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effect binder owns several dictionaries that contain factory methods, which create effect
  /// parameter bindings for different value types. 
  /// </para>
  /// <para>
  /// All dictionaries are empty by default. The dictionary key is a <strong>case-sensitive</strong>
  /// usage name, e.g. "WorldViewProjection". The dictionary value is a factory method that creates 
  /// an <see cref="EffectParameterBinding"/>. New dictionary entries can be added to support new 
  /// effect parameters.
  /// </para>
  /// </remarks>
  public class DictionaryEffectBinder : IEffectBinder
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="EffectParameterBinding"/> for the given effect parameter.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <returns>
    /// The new <see cref="EffectParameterBinding"/> for <paramref name="parameter"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
    public delegate EffectParameterBinding CreateEffectParameterBinding(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData);
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="bool"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> BoolBindings
    {
      get
      {
        if (_boolBindings == null)
          _boolBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _boolBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _boolBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="bool"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> BoolArrayBindings
    {
      get
      {
        if (_boolArrayBindings == null)
          _boolArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _boolArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _boolArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="int"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Int32Bindings
    {
      get
      {
        if (_int32Bindings == null)
          _int32Bindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _int32Bindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _int32Bindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="int"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Int32ArrayBindings
    {
      get
      {
        if (_int32ArrayBindings == null)
          _int32ArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _int32ArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _int32ArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="float"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> SingleBindings 
    {
      get
      {
        if (_singleBindings == null)
          _singleBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _singleBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _singleBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="float"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> SingleArrayBindings 
    {
      get
      {
        if (_singleArrayBindings == null)
          _singleArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _singleArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _singleArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Matrix"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> MatrixBindings 
    {
      get
      {
        if (_matrixBindings == null)
          _matrixBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _matrixBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _matrixBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Matrix"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> MatrixArrayBindings 
    {
      get
      {
        if (_matrixArrayBindings == null)
          _matrixArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _matrixArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _matrixArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Vector2"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Vector2Bindings
    {
      get
      {
        if (_vector2Bindings == null)
          _vector2Bindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _vector2Bindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _vector2Bindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Vector2"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Vector2ArrayBindings
    {
      get
      {
        if (_vector2ArrayBindings == null)
          _vector2ArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _vector2ArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _vector2ArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Vector3"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Vector3Bindings 
    {
      get
      {
        if (_vector3Bindings == null)
          _vector3Bindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _vector3Bindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _vector3Bindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Vector3"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Vector3ArrayBindings 
    {
      get
      {
        if (_vector3ArrayBindings == null)
          _vector3ArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _vector3ArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _vector3ArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Vector4"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Vector4Bindings 
    {
      get
      {
        if (_vector4Bindings == null)
          _vector4Bindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _vector4Bindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _vector4Bindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Vector4"/>[] parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Vector4ArrayBindings 
    {
      get
      {
        if (_vector4ArrayBindings == null)
          _vector4ArrayBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _vector4ArrayBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _vector4ArrayBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Texture"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> TextureBindings 
    {
      get
      {
        if (_textureBindings == null)
          _textureBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _textureBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _textureBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Texture2D"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Texture2DBindings
    {
      get
      {
        if (_texture2DBindings == null)
          _texture2DBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _texture2DBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _texture2DBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="Texture3D"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> Texture3DBindings
    {
      get
      {
        if (_texture3DBindings == null)
          _texture3DBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _texture3DBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _texture3DBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <see cref="TextureCube"/> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> TextureCubeBindings
    {
      get
      {
        if (_textureCubeBindings == null)
          _textureCubeBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _textureCubeBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _textureCubeBindings;


    /// <summary>
    /// Gets or sets the factory methods that create effect parameter bindings for 
    /// <c>struct</c> parameters.
    /// </summary>
    /// <value>The factory methods. The default value is an empty dictionary.</value>
    public Dictionary<string, CreateEffectParameterBinding> StructBindings
    {
      get
      {
        if (_structBindings == null)
          _structBindings = new Dictionary<string, CreateEffectParameterBinding>();

        return _structBindings;
      }
    }
    private Dictionary<string, CreateEffectParameterBinding> _structBindings;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public virtual EffectTechniqueBinding GetBinding(Effect effect)
    {
      return null;
    }


    /// <inheritdoc/>
    public virtual EffectParameterBinding GetBinding(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData)
    {
      var description = effect.GetParameterDescriptions()[parameter];
      if (description.Semantic == null)
        return null;

      // Get dictionary by type.
      var dictionary = GetDictionary(parameter);
      if (dictionary != null)
      {
        // Look up semantic in dictionary.
        CreateEffectParameterBinding createBindingMethod;
        if (dictionary.TryGetValue(description.Semantic, out createBindingMethod))
          return createBindingMethod(effect, parameter, opaqueData);
      }

      return null;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private Dictionary<string, CreateEffectParameterBinding> GetDictionary(EffectParameter parameter)
    {
      Dictionary<string, CreateEffectParameterBinding> dictionary = null;

      var isArray = parameter.Elements.Count > 0;

      if (parameter.ParameterType == EffectParameterType.Bool)
      {
        switch (parameter.ParameterClass)
        {
          case EffectParameterClass.Scalar:
            dictionary = isArray ? _boolArrayBindings : _boolBindings;
            break;

          case EffectParameterClass.Matrix:
          case EffectParameterClass.Vector:
            break;
        }
      }
      else if (parameter.ParameterType == EffectParameterType.Int32)
      {
        switch (parameter.ParameterClass)
        {
          case EffectParameterClass.Scalar:
            dictionary = isArray ? _int32ArrayBindings : _int32Bindings;
            break;

          case EffectParameterClass.Matrix:
          case EffectParameterClass.Vector:
            break;
        }
      }
      else if (parameter.ParameterType == EffectParameterType.Single)
      {
        switch (parameter.ParameterClass)
        {
          case EffectParameterClass.Matrix:
            dictionary = isArray ? _matrixArrayBindings : _matrixBindings;
            break;

          case EffectParameterClass.Vector:
            // Note: On Windows and Xbox 360 vectors are stored as column vectors. (RowCount == 1).
            if (parameter.ColumnCount == 4)
              dictionary = isArray ? _vector4ArrayBindings : _vector4Bindings;
            else if (parameter.ColumnCount == 3)
              dictionary = isArray ? _vector3ArrayBindings : _vector3Bindings;
            else if (parameter.ColumnCount == 2)
              dictionary = isArray ? _vector2ArrayBindings : _vector2Bindings;
            break;

          case EffectParameterClass.Scalar:
            dictionary = isArray ? _singleArrayBindings : _singleBindings;
            break;
        }
      }
      else if (parameter.ParameterType == EffectParameterType.Texture)
      {
        dictionary = _textureBindings;
      }
      else if (parameter.ParameterType == EffectParameterType.Texture2D)
      {
        dictionary = _texture2DBindings;
      }
      else if (parameter.ParameterType == EffectParameterType.Texture3D)
      {
        dictionary = _texture3DBindings;
      }
      else if (parameter.ParameterType == EffectParameterType.TextureCube)
      {
        dictionary = _textureCubeBindings;
      }
      else if (parameter.ParameterClass == EffectParameterClass.Struct)
      {
        dictionary = _structBindings;
      }


      return dictionary;
    }


    /// <overloads>
    /// <summary>
    /// Creates the <see cref="DelegateParameterBinding{T}"/> for an effect parameter.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates the <see cref="DelegateParameterBinding{T}"/> for an effect parameter.
    /// </summary>
    /// <typeparam name="T">The type of the effect parameter.</typeparam>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="computeParameter">The callback method that computes the value.</param>
    /// <returns>The <see cref="DelegateParameterBinding{T}"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static EffectParameterBinding CreateDelegateParameterBinding<T>(Effect effect, EffectParameter parameter, Func<DelegateParameterBinding<T>, RenderContext, T> computeParameter)
    {
      return new DelegateParameterBinding<T>(effect, parameter, computeParameter);
    }


    /// <summary>
    /// Creates the <see cref="DelegateParameterArrayBinding{T}"/> for an effect parameter that 
    /// represents an array of values.
    /// </summary>
    /// <typeparam name="T">The type of the effect parameter.</typeparam>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="computeParameter">The callback method that computes the values.</param>
    /// <returns>The <see cref="DelegateParameterArrayBinding{T}"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static EffectParameterBinding CreateDelegateParameterArrayBinding<T>(Effect effect, EffectParameter parameter, Action<DelegateParameterArrayBinding<T>, RenderContext, T[]> computeParameter)
    {
      return new DelegateParameterArrayBinding<T>(effect, parameter, computeParameter);
    }


    /// <summary>
    /// Creates the <see cref="ConstParameterBinding{T}"/> for an effect parameter with a
    /// default value defined in opaque data.
    /// </summary>
    /// <typeparam name="T">The type of the effect parameter.</typeparam>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <param name="key">The key of an item in the opaque data.</param>
    /// <returns>The <see cref="ConstParameterBinding{T}"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public static EffectParameterBinding CreateConstParameterBinding<T>(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData, string key)
    {
      object value;
      if (opaqueData != null && opaqueData.TryGetValue(key, out value) && value is T)
        return new ConstParameterBinding<T>(effect, parameter, (T)value);

      return null;
    }


    /// <summary>
    /// Creates the <see cref="ConstParameterBinding{T}"/> for an <see cref="Vector3"/>
    /// effect parameter with a default value defined in opaque data. 
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <param name="key">The key of an item in the opaque data.</param>
    /// <returns>The <see cref="ConstParameterBinding{T}"/>.</returns>
    public static EffectParameterBinding CreateConstParameterBindingVector3(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData, string key)
    {
      object value;
      if (opaqueData != null && opaqueData.TryGetValue(key, out value) && value != null)
      {
        if (value is Vector3)
          return new ConstParameterBinding<Vector3>(effect, parameter, (Vector3)value);
        
        if (value is Vector4)
        {
          var v4 = (Vector4)value;
          return new ConstParameterBinding<Vector3>(effect, parameter, new Vector3(v4.X, v4.Y, v4.Z));
        }
      }

      return null;
    }


    /// <summary>
    /// Creates the <see cref="ConstParameterBinding{T}"/> for an <see cref="Vector4"/>
    /// effect parameter with a default value defined in opaque data.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <param name="key">The key of an item in the opaque data.</param>
    /// <param name="defaultW">
    /// The default value for the fourth vector component. (If the default value in the opaque data
    /// is of type <strong>Vector3</strong> then the w component of the <see cref="Vector4"/>
    /// is set to this default value.)
    /// </param>
    /// <returns>The <see cref="ConstParameterBinding{T}"/>.</returns>
    public static EffectParameterBinding CreateConstParameterBindingVector4(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData, string key, float defaultW)
    {
      object value;
      if (opaqueData != null && opaqueData.TryGetValue(key, out value) && value != null)
      {
        if (value is Vector4)
          return new ConstParameterBinding<Vector4>(effect, parameter, (Vector4)value);

        if (value is Vector3)
        {
          var v3 = (Vector3)value;
          return new ConstParameterBinding<Vector4>(effect, parameter, new Vector4(v3.X, v3.Y, v3.Z, defaultW));
        }
      }

      return null;
    }
    #endregion
  }
}
