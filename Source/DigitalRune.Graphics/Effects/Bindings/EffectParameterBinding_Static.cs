// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  partial class EffectParameterBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// Provides delegates that check whether the effect parameter binding has the correct type.
    /// </summary>
    internal static readonly Dictionary<Type, Func<EffectParameter, bool>> ValidateTypeMethods = new Dictionary<Type, Func<EffectParameter, bool>>
    {
      { typeof(bool),         ValidateBoolean     },
      { typeof(int),          ValidateInt32       },
      { typeof(Matrix),       ValidateMatrix      },
      // Not implemented yet:
      //{ typeof(Matrix22F),  },
      //{ typeof(Matrix33F),  },
      { typeof(Matrix44F),    ValidateMatrix      },
      { typeof(Quaternion),   ValidateVector4     },
      { typeof(QuaternionF),  ValidateVector4     },
      { typeof(float),        ValidateSingle      },
#if !MONOGAME
      { typeof(string),       ValidateString      },
#endif
      { typeof(Texture),      ValidateTexture     },
      { typeof(Texture2D),    ValidateTexture2D   },
      { typeof(Texture3D),    ValidateTexture3D   },
      { typeof(TextureCube),  ValidateTextureCube },
      { typeof(Vector2),      ValidateVector2     },
      { typeof(Vector3),      ValidateVector3     },
      { typeof(Vector4),      ValidateVector4     },
      { typeof(Vector2F),     ValidateVector2     },
      { typeof(Vector3F),     ValidateVector3     },
      { typeof(Vector4F),     ValidateVector4     },
    };


    /// <summary>
    /// Provides delegates that set parameter values for all supported value types.
    /// </summary>
    internal static readonly Dictionary<Type, Delegate> SetValueMethods = new Dictionary<Type, Delegate>
    {
      { typeof(bool),        (Action<EffectParameter, bool>)       ((parameter, value) => parameter.SetValue(value))             },
      { typeof(int),         (Action<EffectParameter, int>)        ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Matrix),      (Action<EffectParameter, Matrix>)     ((parameter, value) => parameter.SetValue(value))             },
      // Not implemented yet:
      //{ typeof(Matrix22F),   (Action<EffectParameter, Matrix22F>)  ((parameter, value) => parameter.SetValue(value))             },
      //{ typeof(Matrix33F),   (Action<EffectParameter, Matrix33F>)  ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Matrix44F),   (Action<EffectParameter, Matrix44F>)  ((parameter, value) => parameter.SetValue((Matrix)value))     },
      { typeof(Quaternion),  (Action<EffectParameter, Quaternion>) ((parameter, value) => parameter.SetValue(value))             },
      { typeof(QuaternionF), (Action<EffectParameter, QuaternionF>)((parameter, value) => parameter.SetValue((Quaternion)value)) },
      { typeof(float),       (Action<EffectParameter, float>)      ((parameter, value) => parameter.SetValue(value))             },
#if !MONOGAME
      { typeof(string),      (Action<EffectParameter, string>)     ((parameter, value) => parameter.SetValue(value))             },
#endif
      { typeof(Texture),     (Action<EffectParameter, Texture>)    ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Texture2D),   (Action<EffectParameter, Texture2D>)  ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Texture3D),   (Action<EffectParameter, Texture3D>)  ((parameter, value) => parameter.SetValue(value))             },
      { typeof(TextureCube), (Action<EffectParameter, TextureCube>)((parameter, value) => parameter.SetValue(value))             },
      { typeof(Vector2),     (Action<EffectParameter, Vector2>)    ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Vector3),     (Action<EffectParameter, Vector3>)    ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Vector4),     (Action<EffectParameter, Vector4>)    ((parameter, value) => parameter.SetValue(value))             },
      { typeof(Vector2F),    (Action<EffectParameter, Vector2F>)   ((parameter, value) => parameter.SetValue((Vector2)value))    },
      { typeof(Vector3F),    (Action<EffectParameter, Vector3F>)   ((parameter, value) => parameter.SetValue((Vector3)value))    },
      { typeof(Vector4F),    (Action<EffectParameter, Vector4F>)   ((parameter, value) => parameter.SetValue((Vector4)value))    },
    };


    /// <summary>
    /// Provides delegates that set parameter values for arrays of all supported value types.
    /// </summary>
    internal static readonly Dictionary<Type, Delegate> SetValueArrayMethods = new Dictionary<Type, Delegate>
    {
      // DigitalRune data types are not supported at the moment because we have to copy the
      // array each time (slow + garbage).
#if !MONOGAME
      { typeof(bool),        (Action<EffectParameter, bool[]>)       ((parameter, value) => parameter.SetValue(value)) },
      { typeof(int),         (Action<EffectParameter, int[]>)        ((parameter, value) => parameter.SetValue(value)) },
#endif
      { typeof(Matrix),      (Action<EffectParameter, Matrix[]>)     ((parameter, value) => parameter.SetValue(value)) },
#if !MONOGAME
      { typeof(Quaternion),  (Action<EffectParameter, Quaternion[]>) ((parameter, value) => parameter.SetValue(value)) },
#endif
      //{ typeof(QuaternionF), (Action<EffectParameter, QuaternionF[]>)((parameter, value) => parameter.SetValue(value)) },
      { typeof(float),       (Action<EffectParameter, float[]>)      ((parameter, value) => parameter.SetValue(value)) },
      { typeof(Vector2),     (Action<EffectParameter, Vector2[]>)    ((parameter, value) => parameter.SetValue(value)) },
      { typeof(Vector3),     (Action<EffectParameter, Vector3[]>)    ((parameter, value) => parameter.SetValue(value)) },
      { typeof(Vector4),     (Action<EffectParameter, Vector4[]>)    ((parameter, value) => parameter.SetValue(value)) },
      //{ typeof(Vector2F),    (Action<EffectParameter, Vector2F[]>)   ((parameter, value) => parameter.SetValue(value)) },
      //{ typeof(Vector3F),    (Action<EffectParameter, Vector3F[]>)   ((parameter, value) => parameter.SetValue(value)) },
      //{ typeof(Vector4F),    (Action<EffectParameter, Vector4F[]>)   ((parameter, value) => parameter.SetValue(value)) },

// Following methods cause garbage!
////#if !XBOX
//      // The following types caused a MissingMethodException at runtime (during jitting) on Xbox 360 in XNA 3.1.
//      { typeof(Matrix22F), (Action<EffectParameter, Matrix22F[]>)((parameter, value) => parameter.SetValue(value)) },
//      { typeof(Matrix33F), (Action<EffectParameter, Matrix33F[]>)((parameter, value) => parameter.SetValue(value)) },
//      { typeof(Matrix44F), (Action<EffectParameter, Matrix44F[]>)((parameter, value) => parameter.SetValue(value)) },
////#endif
    };
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal static void ThrowIfArray(Effect effect, EffectParameterDescription description)
    {
      if (description.Parameter.Elements.Count > 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Effect parameter \"{0}\" is an array of elements. " +
          "Use EffectParameterArrayBinding<T> instead of EffectParameterBinding<T>!\n\n{1}",
          description.Parameter.Name,
          GetEffectParameterInfo(description));

        throw new EffectBindingException(message, effect, description.Parameter);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal static void ThrowIfNotArray(Effect effect, EffectParameterDescription description)
    {
      if (description.Parameter.Elements.Count == 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Effect parameter \"{0}\" is not an array of elements. " +
          "Use EffectParameterBinding<T> instead of EffectParameterArrayBinding<T>!\n\n{1}",
          description.Parameter.Name,
          GetEffectParameterInfo(description));

        throw new EffectBindingException(message, effect, description.Parameter);
      }
    }


    /// <summary>
    /// Throws the type of if invalid.
    /// </summary>
    /// <typeparam name="T">The value type of the effect parameter binding.</typeparam>
    /// <param name="effect">The effect.</param>
    /// <param name="description">The effect parameter description.</param>
    /// <param name="validateType">Type of the validate.</param>
    /// <param name="numberOfElements">The number of elements.</param>
    /// <exception cref="DigitalRune.Graphics.Effects.EffectBindingException">
    /// </exception>
    internal static void ThrowIfInvalidType<T>(Effect effect, EffectParameterDescription description, Func<EffectParameter, bool> validateType, int numberOfElements)
    {
      var parameter = description.Parameter;
      if (!validateType(parameter))
      {
        var message = new StringBuilder();
        message.AppendFormat(
          CultureInfo.InvariantCulture,
          "Binding for effect parameter \"{0}\" has wrong type.\n" +
          "Type of effect parameter binding: {1}",
          parameter.Name,
          typeof(T).Name);

        string allowedTypes = GetEffectParameterType(parameter);
        if (allowedTypes != null)
        {
          message.AppendLine();
          message.AppendFormat(
            CultureInfo.InvariantCulture,
            "Allowed types: {0}\n\n",
            allowedTypes);
        }

        string parameterInfo = GetEffectParameterInfo(description);
        message.Append(parameterInfo);

        throw new EffectBindingException(message.ToString(), effect, parameter);
      }

      // Smaller arrays are ok, for example: Setting 58 skinning matrices out of max 72.
      // Bigger arrays are not allowed.
      if (numberOfElements > parameter.Elements.Count)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Length of the array ({0}) is greater than the number of elements of the effect parameter \"{1}\".\n\n{2}",
          numberOfElements,
          parameter.Name,
          GetEffectParameterInfo(description));
        throw new EffectBindingException(message, effect, parameter);
      }
    }


    private static string GetEffectParameterType(EffectParameter parameter)
    {
      switch (parameter.ParameterType)
      {
        case EffectParameterType.Bool:
          return "bool";
        case EffectParameterType.Int32:
          return "int";
        case EffectParameterType.Single:
          switch (parameter.ParameterClass)
          {
            case EffectParameterClass.Scalar:
              return "float";
            case EffectParameterClass.Vector:
              if (parameter.ColumnCount == 2 && parameter.RowCount == 1)
                return "Vector2, Vector2F";
              if (parameter.ColumnCount == 3 && parameter.RowCount == 1)
                return "Vector3, Vector3F";
              if (parameter.ColumnCount == 4 && parameter.RowCount == 1)
                return "Vector4, Vector4F, Quaternion, Quaternion4F";
              break;
            case EffectParameterClass.Matrix:
              return "Matrix, Matrix44F";
          }
          break;
        case EffectParameterType.String:
          return "string";
        case EffectParameterType.Texture:
          return "Texture, Texture2D, Texture3D, TextureCube";
        case EffectParameterType.Texture1D:
        case EffectParameterType.Texture2D:
          return "Texture, Texture2D";
        case EffectParameterType.Texture3D:
          return "Texture, Texture3D";
        case EffectParameterType.TextureCube:
          return "Texture, TextureCube";
        case EffectParameterType.Void:
          break;
      }

      return null;
    }


    private static string GetEffectParameterInfo(EffectParameterDescription description)
    {
      var parameter = description.Parameter;
      return string.Format(
        CultureInfo.InvariantCulture,
        "EffectParameter (XNA):\n" +
        "  Name = {0}\n" +
        "  Semantic = {1}\n" +
        "  ParameterClass = {2}\n" +
        "  ParameterType = {3}\n" +
        "  RowCount = {4}\n" +
        "  ColumnCount = {5}\n" +
        "  Elements.Count = {6}\n" +
        "\n" +
        "EffectParameterDescription (DigitalRune Graphics):\n" +
        "  Semantic = {7}\n" +
        "  Index = {8}\n" +
        "  Hint = {9}",
        parameter.Name,
        parameter.Semantic,
        parameter.ParameterClass,
        parameter.ParameterType,
        parameter.RowCount,
        parameter.ColumnCount,
        parameter.Elements.Count,
        description.Semantic,
        description.Index,
        description.Hint);
    }


    private static bool ValidateBoolean(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Scalar
             && parameter.ParameterType == EffectParameterType.Bool;
    }


    private static bool ValidateInt32(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Scalar
             && parameter.ParameterType == EffectParameterType.Int32;
    }


    private static bool ValidateSingle(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Scalar
             && parameter.ParameterType == EffectParameterType.Single;
    }


    private static bool ValidateMatrix(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Matrix
             && parameter.ParameterType == EffectParameterType.Single;
    }


#if !MONOGAME
    private static bool ValidateString(EffectParameter parameter)
    {
      return parameter.ParameterType == EffectParameterType.String;
    }
#endif


    private static bool ValidateTexture(EffectParameter parameter)
    {
      var parameterType = parameter.ParameterType;
      return parameterType == EffectParameterType.Texture
             || parameterType == EffectParameterType.Texture1D
             || parameterType == EffectParameterType.Texture2D
             || parameterType == EffectParameterType.Texture3D
             || parameterType == EffectParameterType.TextureCube;
    }


    private static bool ValidateTexture2D(EffectParameter parameter)
    {
      var parameterType = parameter.ParameterType;
      return parameterType == EffectParameterType.Texture
             || parameterType == EffectParameterType.Texture1D
             || parameterType == EffectParameterType.Texture2D;
    }


    private static bool ValidateTexture3D(EffectParameter parameter)
    {
      var parameterType = parameter.ParameterType;
      return parameterType == EffectParameterType.Texture
             || parameterType == EffectParameterType.Texture3D;
    }


    private static bool ValidateTextureCube(EffectParameter parameter)
    {
      var parameterType = parameter.ParameterType;
      return parameterType == EffectParameterType.Texture
             || parameterType == EffectParameterType.TextureCube;
    }


    private static bool ValidateVector2(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Vector
             && parameter.ParameterType == EffectParameterType.Single
             && parameter.ColumnCount == 2
             && parameter.RowCount == 1;
    }


    private static bool ValidateVector3(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Vector
             && parameter.ParameterType == EffectParameterType.Single
             && parameter.ColumnCount == 3
             && parameter.RowCount == 1;
    }


    private static bool ValidateVector4(EffectParameter parameter)
    {
      return parameter.ParameterClass == EffectParameterClass.Vector
             && parameter.ParameterType == EffectParameterType.Single
             && parameter.ColumnCount == 4
             && parameter.RowCount == 1;
    }
    #endregion
    
  }
}
