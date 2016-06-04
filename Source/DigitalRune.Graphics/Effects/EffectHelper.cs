// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides extension methods for the <see cref="Effect"/> class and related types.
  /// </summary>
  public static partial class EffectHelper
  {
    // Note: Helper methods with arrays are commented out because they create garbage 
    // and should not be used.

    #region ----- Effect extensions -----

    ///// <summary>
    ///// Gets the effect parameter with the given name.
    ///// </summary>
    ///// <param name="effect">The effect.</param>
    ///// <param name="name">The name of the effect parameter. (Case-sensitive)</param>
    ///// <returns>
    ///// The effect parameter with the given name or <see langword="null"/> no matching effect 
    ///// parameter was found.
    ///// </returns>
    //public static EffectParameter GetParameterByName(this Effect effect, string name)
    //{
    //  return effect.Parameters[name];
    //}


    ///// <summary>
    ///// Gets the effect parameter with the given semantic.
    ///// </summary>
    ///// <param name="effect">The effect.</param>
    ///// <param name="semantic">
    ///// The semantic of the effect parameter as defined in the effect file (.fx). (Case-insensitive)
    ///// </param>
    ///// <returns>
    ///// The effect parameter with the given semantic or <see langword="null"/> no matching effect 
    ///// parameter was found.
    ///// </returns>
    //public static EffectParameter GetParameterBySemantic(this Effect effect, string semantic)
    //{
    //  return effect.Parameters.GetParameterBySemantic(semantic);
    //}


    ///// <summary>
    ///// Gets the effect parameter with the given name or semantic.
    ///// </summary>
    ///// <param name="effect">The effect.</param>
    ///// <param name="name">The name of the effect parameter. (Case-sensitive)</param>
    ///// <param name="semantic">
    ///// The semantic of the effect parameter as defined in the effect file (.fx). (Case-insensitive)
    ///// </param>
    ///// <returns>
    ///// The effect parameter with the given name/semantic or <see langword="null"/> no matching 
    ///// effect parameter was found.
    ///// </returns>
    ///// <remarks>
    ///// This method first searches for an effect parameter with the given name. If no match is found
    ///// it searches for an effect parameter with the given semantic.
    ///// </remarks>
    //public static EffectParameter GetParameterByNameOrSemantic(this Effect effect, string name, string semantic)
    //{
    //  EffectParameter parameter = effect.Parameters[name];

    //  if (parameter == null)
    //    parameter = effect.Parameters.GetParameterBySemantic(semantic);

    //  return parameter;
    //}


    ///// <summary>
    ///// Gets the effect parameter with the given semantic or name.
    ///// </summary>
    ///// <param name="effect">The effect.</param>
    ///// <param name="semantic">
    ///// The semantic of the effect parameter as defined in the effect file (.fx). (Case-insensitive)
    ///// </param>
    ///// <param name="name">The name of the effect parameter. (Case-sensitive)</param>
    ///// <returns>
    ///// The effect parameter with the given name/semantic or <see langword="null"/> no 
    ///// matching effect parameter was found.
    ///// </returns>
    ///// <remarks>
    ///// This method first searches for an effect parameter with the given semantic. If 
    ///// no match is found it searches for an effect parameter with the given name.
    ///// </remarks>
    //public static EffectParameter GetParameterBySemanticOrName(this Effect effect, string semantic, string name)
    //{
    //  EffectParameter parameter = effect.Parameters.GetParameterBySemantic(semantic);

    //  if (parameter == null)
    //    parameter = effect.Parameters[name];

    //  return parameter;
    //}
    #endregion


    #region ----- EffectParameter extensions for DigitalRune Mathematics -----

    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="Matrix22F"/>.
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="Matrix22F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix22F"/>.
    ///// </exception>
    //public static Matrix22F GetValueMatrix22F(this EffectParameter parameter)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="Matrix22F"/>.
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="Matrix22F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix22F"/>.
    ///// </exception>
    //public static Matrix22F[] GetValueMatrix22FArray(this EffectParameter parameter, int count)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="Matrix33F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="Matrix33F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix33F"/>.
    ///// </exception>
    //public static Matrix33F GetValueMatrix33F(this EffectParameter parameter)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="Matrix33F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="Matrix33F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix33F"/>.
    ///// </exception>
    //public static Matrix33F[] GetValueMatrix33FArray(this EffectParameter parameter, int count)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="Matrix44F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="Matrix44F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix44F"/>.
    ///// </exception>
    //public static Matrix44F GetValueMatrix44F(this EffectParameter parameter)
    //{
    //  return (Matrix44F)parameter.GetValueMatrix();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="Matrix44F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="Matrix44F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix44F"/>.
    ///// </exception>
    //public static Matrix44F[] GetValueMatrix44FArray(this EffectParameter parameter, int count)
    //{
    //  Matrix[] value = parameter.GetValueMatrixArray(count);

    //  var arrayLength = value.Length;
    //  Matrix44F[] convertedValue = new Matrix44F[arrayLength];
    //  for (int i = 0; i < arrayLength; i++)
    //    convertedValue[i] = (Matrix44F)value[i];

    //  return convertedValue;
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="QuaternionF"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="QuaternionF"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="QuaternionF"/>.
    ///// </exception>
    //public static QuaternionF GetValueQuaternionF(this EffectParameter parameter)
    //{
    //  return (QuaternionF)parameter.GetValueQuaternion();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="QuaternionF"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="QuaternionF"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="QuaternionF"/>.
    ///// </exception>
    //public static QuaternionF[] GetValueQuaternionFArray(this EffectParameter parameter, int count)
    //{
    //  Quaternion[] value = parameter.GetValueQuaternionArray(count);

    //  var arrayLength = value.Length;
    //  QuaternionF[] convertedValue = new QuaternionF[arrayLength];
    //  for (int i = 0; i < arrayLength; i++)
    //    convertedValue[i] = (QuaternionF)value[i];

    //  return convertedValue;
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="Vector2F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="Vector2F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector2F"/>.
    ///// </exception>
    //public static Vector2F GetValueVector2F(this EffectParameter parameter)
    //{
    //  return (Vector2F)parameter.GetValueVector2();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="Vector2F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="Vector2F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector2F"/>.
    ///// </exception>
    //public static Vector2F[] GetValueVector2FArray(this EffectParameter parameter, int count)
    //{
    //  Vector2[] value = parameter.GetValueVector2Array(count);

    //  var arrayLength = value.Length;
    //  Vector2F[] convertedValue = new Vector2F[arrayLength];
    //  for (int i = 0; i < arrayLength; i++)
    //    convertedValue[i] = (Vector2F)value[i];

    //  return convertedValue;
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="Vector3F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="Vector3F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    ///// </exception>
    //public static Vector3F GetValueVector3F(this EffectParameter parameter)
    //{
    //  return (Vector3F)parameter.GetValueVector3();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="Vector3F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="Vector3F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    ///// </exception>
    //public static Vector3F[] GetValueVector3FArray(this EffectParameter parameter, int count)
    //{
    //  Vector3[] value = parameter.GetValueVector3Array(count);

    //  var arrayLength = value.Length;
    //  Vector3F[] convertedValue = new Vector3F[arrayLength];
    //  for (int i = 0; i < arrayLength; i++)
    //    convertedValue[i] = (Vector3F)value[i];

    //  return convertedValue;
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as <see cref="Vector4F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <returns>
    ///// The value of the effect parameter as <see cref="Vector4F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    ///// </exception>
    //public static Vector4F GetValueVector4F(this EffectParameter parameter)
    //{
    //  return (Vector4F)parameter.GetValueVector4();
    //}


    ///// <summary>
    ///// Gets the value of the effect parameter as an array of <see cref="Vector4F"/>. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="count">The number of elements in the array.</param>
    ///// <returns>
    ///// The value of the effect parameter as an array of <see cref="Vector4F"/>.
    ///// </returns>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    ///// </exception>
    //public static Vector4F[] GetValueVector4FArray(this EffectParameter parameter, int count)
    //{
    //  Vector4[] value = parameter.GetValueVector4Array(count);

    //  var arrayLength = value.Length;
    //  Vector4F[] convertedValue = new Vector4F[arrayLength];
    //  for (int i = 0; i < arrayLength; i++)
    //    convertedValue[i] = (Vector4F)value[i];

    //  return convertedValue;
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix22F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Matrix22F value)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix22F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Matrix22F[] value)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix33F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Matrix33F value)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix33F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Matrix33F[] value)
    //{
    //  throw new NotImplementedException();
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix44F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Matrix44F value)
    //{
    //  parameter.SetValue((Matrix)value);
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Matrix44F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Matrix44F[] value)
    //{
    //  if (value != null)
    //  {
    //    var arrayLength = value.Length;
    //    Matrix[] convertedValue = new Matrix[arrayLength];
    //    for (int i = 0; i < arrayLength; i++)
    //      convertedValue[i] = (Matrix)value[i];

    //    parameter.SetValue(convertedValue);
    //  }
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="QuaternionF"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, QuaternionF value)
    //{
    //  parameter.SetValue((Quaternion)value);
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="QuaternionF"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, QuaternionF[] value)
    //{
    //  if (value != null)
    //  {
    //    var arrayLength = value.Length;
    //    Quaternion[] convertedValue = new Quaternion[arrayLength];
    //    for (int i = 0; i < arrayLength; i++)
    //      convertedValue[i] = (Quaternion)value[i];

    //    parameter.SetValue(convertedValue);
    //  }
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector2F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Vector2F value)
    //{
    //  parameter.SetValue((Vector2)value);
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector2F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Vector2F[] value)
    //{
    //  if (value != null)
    //  {
    //    var arrayLength = value.Length;
    //    Vector2[] convertedValue = new Vector2[arrayLength];
    //    for (int i = 0; i < arrayLength; i++)
    //      convertedValue[i] = (Vector2)value[i];

    //    parameter.SetValue(convertedValue);
    //  }
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Vector3F value)
    //{
    //  parameter.SetValue((Vector3)value);
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Vector3F[] value)
    //{
    //  if (value != null)
    //  {
    //    var arrayLength = value.Length;
    //    Vector3[] convertedValue = new Vector3[arrayLength];
    //    for (int i = 0; i < arrayLength; i++)
    //      convertedValue[i] = (Vector3)value[i];

    //    parameter.SetValue(convertedValue);
    //  }
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Vector4F value)
    //{
    //  parameter.SetValue((Vector4)value);
    //}


    ///// <summary>
    ///// Sets the value of the effect parameter. 
    ///// </summary>
    ///// <param name="parameter">The effect parameter.</param>
    ///// <param name="value">The value to assign to the effect parameter.</param>
    ///// <exception cref="InvalidCastException">
    ///// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    ///// </exception>
    //public static void SetValue(this EffectParameter parameter, Vector4F[] value)
    //{
    //  if (value != null)
    //  {
    //    var arrayLength = value.Length;
    //    Vector4[] convertedValue = new Vector4[arrayLength];
    //    for (int i = 0; i < arrayLength; i++)
    //      convertedValue[i] = (Vector4)value[i];

    //    parameter.SetValue(convertedValue);
    //  }
    //}    
    #endregion


    #region ----- EffectParameter extensions for special types -----

    /// <summary>
    /// Gets the value of the effect parameter as <see cref="Color"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as <see cref="Color"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Color"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Color GetColor(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
        return new Color(parameter.GetValueVector4());
      else
        return new Color(parameter.GetValueVector3());
    }


    /// <overloads>
    /// <summary>
    /// Sets the value of an effect parameter that represents a color.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the value of the effect parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="color">The color given as <see cref="Color"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Color"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetColor(this EffectParameter parameter, Color color)
    {
      if (parameter.ColumnCount == 4)
        parameter.SetValue(color.ToVector4());
      else
        parameter.SetValue(color.ToVector3());
    }


    /// <summary>
    /// Gets the value of the effect parameter as a RGB color represented as
    /// <see cref="Vector3"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a RGB color represented as
    /// <see cref="Vector3"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector3 GetColorVector3(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        return new Vector3(value.X, value.Y, value.Z);
      }
      else
      {
        return parameter.GetValueVector3();
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a RGB color.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="color">The color given as <see cref="Vector3"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetColor(this EffectParameter parameter, Vector3 color)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = new Vector4(color, 1);
        parameter.SetValue(value);
      }
      else
      {
        parameter.SetValue(color);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a RGB color represented as
    /// <see cref="Vector3F"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a RGB color represented as
    /// <see cref="Vector3F"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector3F GetColorVector3F(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        return new Vector3F(value.X, value.Y, value.Z);
      }
      else
      {
        return (Vector3F)parameter.GetValueVector3();
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a RGB color.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="color">The color given as <see cref="Vector3F"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetColor(this EffectParameter parameter, Vector3F color)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = new Vector4(color.X, color.Y, color.Z, 1);
        parameter.SetValue(value);
      }
      else
      {
        parameter.SetValue((Vector3)color);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a RGBA color represented as
    /// <see cref="Vector4"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a RGBA color represented as
    /// <see cref="Vector4"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector4 GetColorVector4(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        return parameter.GetValueVector4();
      }
      else
      {
        Vector3 value = parameter.GetValueVector3();
        return new Vector4(value, 1);
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a RGBA color.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="color">The color given as <see cref="Vector4"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetColor(this EffectParameter parameter, Vector4 color)
    {
      if (parameter.ColumnCount == 4)
      {
        parameter.SetValue(color);
      }
      else
      {
        Vector3 value = new Vector3(color.X, color.Y, color.Z);
        parameter.SetValue(value);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a RGBA color represented as
    /// <see cref="Vector4F"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a RGBA color represented as
    /// <see cref="Vector4F"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector4F GetColorVector4F(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        return (Vector4F)parameter.GetValueVector4();
      }
      else
      {
        Vector3 value = parameter.GetValueVector3();
        return new Vector4F(value.X, value.Y, value.Z, 1);
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a RGBA color.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="color">The color given as <see cref="Vector4F"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetColor(this EffectParameter parameter, Vector4F color)
    {
      if (parameter.ColumnCount == 4)
      {
        parameter.SetValue((Vector4)color);
      }
      else
      {
        Vector3 value = new Vector3(color.X, color.Y, color.Z);
        parameter.SetValue(value);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as position vector represented as 
    /// <see cref="Vector3"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as position vector represented as 
    /// <see cref="Vector3"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector3 GetPositionVector3(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckPositionVector(parameter, value);
        return new Vector3(value.X, value.Y, value.Z);
      }
      else
      {
        return parameter.GetValueVector3();
      }
    }


    /// <overloads>
    /// <summary>
    /// Sets the value of an effect parameter that represents a position.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the value of the effect parameter as a position vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="position">The position.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Color"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetPosition(this EffectParameter parameter, Vector3 position)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = new Vector4(position, 1);
        parameter.SetValue(value);
      }
      else
      {
        parameter.SetValue(position);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a position vector represented as
    /// <see cref="Vector3F"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a position vector represented as
    /// <see cref="Vector3F"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector3F GetPositionVector3F(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckPositionVector(parameter, value);
        return new Vector3F(value.X, value.Y, value.Z);
      }
      else
      {
        return (Vector3F)parameter.GetValueVector3();
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a position vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="position">The position given as <see cref="Vector3F"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetPosition(this EffectParameter parameter, Vector3F position)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = new Vector4(position.X, position.Y, position.Z, 1);
        parameter.SetValue(value);
      }
      else
      {
        parameter.SetValue((Vector3)position);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a position vector represented as
    /// <see cref="Vector4"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a position vector represented as
    /// <see cref="Vector4"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector4 GetPositionVector4(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckPositionVector(parameter, value);
        return value;
      }
      else
      {
        Vector3 value = parameter.GetValueVector3();
        return new Vector4(value, 1);
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a position vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="position">The position given as <see cref="Vector4"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetPosition(this EffectParameter parameter, Vector4 position)
    {
      CheckPositionVector(parameter, position);
      if (parameter.ColumnCount == 4)
      {
        parameter.SetValue(position);
      }
      else
      {
        Vector3 value = new Vector3(position.X, position.Y, position.Z);
        parameter.SetValue(value);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a position vector represented as
    /// <see cref="Vector4F"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a position vector represented as
    /// <see cref="Vector4F"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector4F GetPositionVector4F(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckPositionVector(parameter, value);
        return (Vector4F)value;
      }
      else
      {
        Vector3 value = parameter.GetValueVector3();
        return new Vector4F(value.X, value.Y, value.Z, 1);
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a position vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="position">The position given as <see cref="Vector4F"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetPosition(this EffectParameter parameter, Vector4F position)
    {
      CheckPositionVector(parameter, (Vector4)position);
      if (parameter.ColumnCount == 4)
      {
        parameter.SetValue((Vector4)position);
      }
      else
      {
        Vector3 value = new Vector3(position.X, position.Y, position.Z);
        parameter.SetValue(value);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as direction vector represented as 
    /// <see cref="Vector3"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as direction vector represented as 
    /// <see cref="Vector3"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector3 GetDirectionVector3(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckDirectionVector(parameter, value);
        return new Vector3(value.X, value.Y, value.Z);
      }
      else
      {
        return parameter.GetValueVector3();
      }
    }


    /// <overloads>
    /// <summary>
    /// Sets the value of an effect parameter that represents a direction.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the value of the effect parameter as a direction vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="direction">The direction.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Color"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetDirection(this EffectParameter parameter, Vector3 direction)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = new Vector4(direction, 0);
        parameter.SetValue(value);
      }
      else
      {
        parameter.SetValue(direction);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a direction vector represented as
    /// <see cref="Vector3F"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a direction vector represented as
    /// <see cref="Vector3F"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector3F GetDirectionVector3F(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckDirectionVector(parameter, value);
        return new Vector3F(value.X, value.Y, value.Z);
      }
      else
      {
        return (Vector3F)parameter.GetValueVector3();
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a direction vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="direction">The direction given as <see cref="Vector3F"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector3F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetDirection(this EffectParameter parameter, Vector3F direction)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = new Vector4(direction.X, direction.Y, direction.Z, 0);
        parameter.SetValue(value);
      }
      else
      {
        parameter.SetValue((Vector3)direction);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a direction vector represented as
    /// <see cref="Vector4"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a direction vector represented as
    /// <see cref="Vector4"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector4 GetDirectionVector4(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckDirectionVector(parameter, value);
        return value;
      }
      else
      {
        Vector3 value = parameter.GetValueVector3();
        return new Vector4(value, 0);
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a direction vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="direction">The direction given as <see cref="Vector4"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetDirection(this EffectParameter parameter, Vector4 direction)
    {
      CheckDirectionVector(parameter, direction);
      if (parameter.ColumnCount == 4)
      {
        parameter.SetValue(direction);
      }
      else
      {
        Vector3 value = new Vector3(direction.X, direction.Y, direction.Z);
        parameter.SetValue(value);
      }
    }


    /// <summary>
    /// Gets the value of the effect parameter as a direction vector represented as
    /// <see cref="Vector4F"/>. 
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The value of the effect parameter as a direction vector represented as
    /// <see cref="Vector4F"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static Vector4F GetDirectionVector4F(this EffectParameter parameter)
    {
      if (parameter.ColumnCount == 4)
      {
        Vector4 value = parameter.GetValueVector4();
        CheckDirectionVector(parameter, value);
        return (Vector4F)value;
      }
      else
      {
        Vector3 value = parameter.GetValueVector3();
        return new Vector4F(value.X, value.Y, value.Z, 0);
      }
    }


    /// <summary>
    /// Sets the value of the effect parameter to a direction vector.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="direction">The direction given as <see cref="Vector4F"/>.</param>
    /// <exception cref="InvalidCastException">
    /// Unable to cast this effect parameter to <see cref="Vector4F"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void SetDirection(this EffectParameter parameter, Vector4F direction)
    {
      CheckDirectionVector(parameter, (Vector4)direction);
      if (parameter.ColumnCount == 4)
      {
        parameter.SetValue((Vector4)direction);
      }
      else
      {
        Vector3 value = new Vector3(direction.X, direction.Y, direction.Z);
        parameter.SetValue(value);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [Conditional("DEBUG")]
    private static void CheckPositionVector(EffectParameter parameter, Vector4 value)
    {
      if (value.W == 0)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "EffectParameter \"{0}\" is not a valid position vector. the w-component must not be 0 (actual value = {1}).",
          parameter.Name,
          value.W);

        throw new InvalidCastException(message);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [Conditional("DEBUG")]
    private static void CheckDirectionVector(EffectParameter parameter, Vector4 value)
    {
      if (value.W != 0)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "EffectParameter \"{0}\" is not a valid direction vector. The w-component must be 0 (actual value = {1}).",
          parameter.Name,
          value.W);

        throw new InvalidCastException(message);
      }
    }
    #endregion
  }
}
