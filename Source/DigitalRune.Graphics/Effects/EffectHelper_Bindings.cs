// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  partial class EffectHelper
  {
    /// <summary>
    /// Creates the technique binding for the specified effect by calling the 
    /// <see cref="IEffectBinder"/>s.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <returns>
    /// The effect technique binding created by an <see cref="IEffectBinder"/>. If no
    /// <see cref="IEffectBinder"/> returned a binding then a default 
    /// <see cref="EffectTechniqueBinding"/> is returned.
    /// </returns>
    internal static EffectTechniqueBinding CreateTechniqueBinding(IGraphicsService graphicsService, Effect effect)
    {
      foreach (var binder in graphicsService.EffectBinders)
      {
        var binding = binder.GetBinding(effect);
        if (binding != null)
          return binding;
      }

      return EffectTechniqueBinding.Default;
    }


    /// <summary>
    /// Initializes the bindings of the effect by calling the <see cref="IEffectBinder"/>s.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effectEx">The effect wrapper.</param>
    /// <param name="opaqueData">The opaque data.</param>
    /// <param name="bindings">
    /// The collection that stores the resulting effect parameter bindings.
    /// </param>
    /// <remarks>
    /// If the <see cref="IEffectBinder"/>s do not return a binding, then this method creates
    /// default bindings using opaque data or the default values from the effect. 
    /// </remarks>
    internal static void InitializeParameterBindings(IGraphicsService graphicsService, EffectEx effectEx, IDictionary<string, object> opaqueData, EffectParameterBindingCollection bindings)
    {
      foreach (var parameter in effectEx.Resource.Parameters)
        SetBinding(graphicsService, effectEx, opaqueData, bindings, parameter);
    }


    private static void SetBinding(IGraphicsService graphicsService, EffectEx effectEx, IDictionary<string, object> opaqueData, EffectParameterBindingCollection bindings, EffectParameter parameter)
    {
      // Skip this parameter if we already have a binding. (Could be created in 
      // derived classes that override OnInitializeBindings()).
      if (bindings.Contains(parameter))
        return;

      // Get description.
      EffectParameterDescription description;
      effectEx.ParameterDescriptions.TryGet(parameter, out description);

      if (description != null)
      {
        // Check if parameter needs to be added to the bindings collection.
        if ((description.Hint & bindings.Hints) == 0)
          return;

        var effect = effectEx.Resource;

        // Try to get binding using the effect parameter binders.
        bool bindingCreated = SetAutomaticBinding(effect, parameter, graphicsService.EffectBinders, opaqueData, bindings);
        if (bindingCreated)
          return;

        // Look up effect parameter in opaque data.
        bindingCreated = SetOpaqueDataBinding(effect, parameter, opaqueData, bindings);
        if (bindingCreated)
          return;

        // Default: Use default value stored in .fx file.
        SetDefaultBinding(graphicsService, effectEx, parameter, description, bindings);
      }
      else
      {
        // Parameter has no description? This happens for structs and arrays of
        // structs. We bind the struct members directly.
        if (parameter.ParameterClass == EffectParameterClass.Struct)
        {
          if (parameter.Elements.Count > 0)
          {
            // Effect parameter is an array of structs. 
            foreach (EffectParameter element in parameter.Elements)
              foreach (EffectParameter member in element.StructureMembers)
                SetBinding(graphicsService, effectEx, opaqueData, bindings, member);
          }
          else
          {
            // Effect parameter is a struct. 
            foreach (EffectParameter member in parameter.StructureMembers)
              SetBinding(graphicsService, effectEx, opaqueData, bindings, member);
          }
        }
      }
    }


    private static bool SetAutomaticBinding(Effect effect, EffectParameter parameter, IList<IEffectBinder> binders, IDictionary<string, object> opaqueData, EffectParameterBindingCollection bindings)
    {
      Debug.Assert(!bindings.Contains(parameter), "Effect binding already contains a binding for the given effect parameter.");

      // Loop through all IEffectParameterBinders and try to setup a valid binding.
      int numberOfBinders = binders.Count;
      for (int i = 0; i < numberOfBinders; i++)
      {
        EffectParameterBinding binding = binders[i].GetBinding(effect, parameter, opaqueData);
        if (binding != null)
        {
          bindings.Add(binding);
          return true;
        }
      }

      return false;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Chosen to reduce nesting.")]
    private static bool SetOpaqueDataBinding(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData, EffectParameterBindingCollection bindings)
    {
      Debug.Assert(!bindings.Contains(parameter), "Effect binding already contains a binding for the given effect parameter.");

      if (opaqueData == null || opaqueData.Count == 0)
      {
        // No opaque data.
        return false;
      }

      if (parameter.ParameterClass == EffectParameterClass.Struct)
      {
        // Structs cannot be set from opaque data.
        return false;
      }

      // Get value from opaque data.
      object value;
      bool valueFound = opaqueData.TryGetValue(parameter.Name, out value);
      if (!valueFound)
        return false;

      EffectParameterBinding binding = null;
      if (parameter.Elements.Count == 0)
      {
        // ----- Parameter is not an array.
        if (parameter.ParameterClass == EffectParameterClass.Scalar)
        {
          // Scalar value bindings.
          if (parameter.ParameterType == EffectParameterType.Bool && ObjectHelper.IsConvertible(value))
            binding = new ConstParameterBinding<bool>(effect, parameter,  ObjectHelper.ConvertTo<bool>(value, CultureInfo.InvariantCulture));
          else if (parameter.ParameterType == EffectParameterType.Int32 && ObjectHelper.IsConvertible(value))
            binding = new ConstParameterBinding<int>(effect, parameter, ObjectHelper.ConvertTo<int>(value, CultureInfo.InvariantCulture));
          else if (parameter.ParameterType == EffectParameterType.Single && ObjectHelper.IsConvertible(value))
            binding = new ConstParameterBinding<float>(effect, parameter, ObjectHelper.ConvertTo<float>(value, CultureInfo.InvariantCulture));
        }
        else if (parameter.ParameterClass == EffectParameterClass.Vector && parameter.ParameterType == EffectParameterType.Single)
        {
          if (parameter.ColumnCount == 2 || parameter.RowCount == 2)
          {
            if (value is Vector2)
              binding = new ConstParameterBinding<Vector2>(effect, parameter, (Vector2)value);
            else if (value is Vector2F)
              binding = new ConstParameterBinding<Vector2F>(effect, parameter, (Vector2F)value);
          }
          else if (parameter.ColumnCount == 3 || parameter.RowCount == 3)
          {
            if (value is Vector3)
              binding = new ConstParameterBinding<Vector3>(effect, parameter, (Vector3)value);
            else if (value is Vector3F)
              binding = new ConstParameterBinding<Vector3F>(effect, parameter, (Vector3F)value);
          }
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
          {
            if (value is Vector4)
              binding = new ConstParameterBinding<Vector4>(effect, parameter, (Vector4)value);
            else if (value is Vector4F)
              binding = new ConstParameterBinding<Vector4F>(effect, parameter, (Vector4F)value);
          }
        }
        else if (parameter.ParameterClass == EffectParameterClass.Matrix && parameter.ParameterType == EffectParameterType.Single)
        {
          if (parameter.ColumnCount == 2 && parameter.RowCount == 2)
          {
            if (value is Matrix22F)
              binding = new ConstParameterBinding<Matrix22F>(effect, parameter, (Matrix22F)value);
          }
          else if (parameter.ColumnCount == 3 && parameter.RowCount == 3)
          {
            if (value is Matrix33F)
              binding = new ConstParameterBinding<Matrix33F>(effect, parameter, (Matrix33F)value);
          }
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
          {
            if (value is Matrix)
              binding = new ConstParameterBinding<Matrix>(effect, parameter, (Matrix)value);
            else if (value is Matrix44F)
              binding = new ConstParameterBinding<Matrix44F>(effect, parameter, (Matrix44F)value);
          }
        }
        else if (parameter.ParameterClass == EffectParameterClass.Object)
        {
          if (parameter.ParameterType == EffectParameterType.String && value is string)
            binding = new ConstParameterBinding<string>(effect, parameter, (string)value);
          else if (parameter.ParameterType == EffectParameterType.Texture && value is Texture)
            binding = new ConstParameterBinding<Texture>(effect, parameter, (Texture)value);
          else if (parameter.ParameterType == EffectParameterType.Texture1D && value is Texture)
            binding = null; // 1D textures are not supported in XNA.
          else if (parameter.ParameterType == EffectParameterType.Texture2D && value is Texture2D)
            binding = new ConstParameterBinding<Texture2D>(effect, parameter, (Texture2D)value);
          else if (parameter.ParameterType == EffectParameterType.Texture3D && value is Texture3D)
            binding = new ConstParameterBinding<Texture3D>(effect, parameter, (Texture3D)value);
          else if (parameter.ParameterType == EffectParameterType.TextureCube && value is TextureCube)
            binding = new ConstParameterBinding<TextureCube>(effect, parameter, (TextureCube)value);
        }
      }
      else
      {
        // ----- Parameter is array.
        // TODO: We could also check whether the length of the arrays match.
        if (parameter.ParameterClass == EffectParameterClass.Scalar)
        {
          // Scalar value bindings.
          if (parameter.ParameterType == EffectParameterType.Bool && IsArray<bool>(value))
            binding = new ConstParameterArrayBinding<bool>(effect, parameter, ToBooleanArray(value));
          else if (parameter.ParameterType == EffectParameterType.Int32 && IsArray<int>(value))
            binding = new ConstParameterArrayBinding<int>(effect, parameter, ToInt32Array(value));
          else if (parameter.ParameterType == EffectParameterType.Single && IsArray<float>(value))
            binding = new ConstParameterArrayBinding<float>(effect, parameter, ToSingleArray(value));
        }
        else if (parameter.ParameterClass == EffectParameterClass.Vector &&
                 parameter.ParameterType == EffectParameterType.Single)
        {
          if (parameter.ColumnCount == 2 || parameter.RowCount == 2)
          {
            if (value is Vector2[])
              binding = new ConstParameterArrayBinding<Vector2>(effect, parameter, (Vector2[])value);
            else if (value is Vector2F[])
              binding = new ConstParameterArrayBinding<Vector2F>(effect, parameter, (Vector2F[])value);
          }
          else if (parameter.ColumnCount == 3 || parameter.RowCount == 3)
          {
            if (value is Vector3[])
              binding = new ConstParameterArrayBinding<Vector3>(effect, parameter, (Vector3[])value);
            else if (value is Vector3F[])
              binding = new ConstParameterArrayBinding<Vector3F>(effect, parameter, (Vector3F[])value);
          }
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
          {
            if (value is Vector4[])
              binding = new ConstParameterArrayBinding<Vector4>(effect, parameter, (Vector4[])value);
            else if (value is Vector4F[])
              binding = new ConstParameterArrayBinding<Vector4F>(effect, parameter, (Vector4F[])value);
          }
        }
        else if (parameter.ParameterClass == EffectParameterClass.Matrix &&
                 parameter.ParameterType == EffectParameterType.Single)
        {
          //#if !XBOX
          // The type Matrix22F[], Matrix33F[], Matrix44F[] caused a MissingMethodException at runtime on Xbox 360 in XNA 3.1.
          // See also: EffectParameterBinding<T>.SetValueMethods!
          if (parameter.ColumnCount == 2 && parameter.RowCount == 2)
          {
            if (value is Matrix22F[])
              binding = new ConstParameterArrayBinding<Matrix22F>(effect, parameter, (Matrix22F[])value);
          }
          else if (parameter.ColumnCount == 3 && parameter.RowCount == 3)
          {
            if (value is Matrix33F[])
              binding = new ConstParameterArrayBinding<Matrix33F>(effect, parameter, (Matrix33F[])value);
          }
          else
            //#endif
            if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
            {
              if (value is Matrix[])
                binding = new ConstParameterArrayBinding<Matrix>(effect, parameter, (Matrix[])value);
              //#if !XBOX
              else if (value is Matrix44F[])
                binding = new ConstParameterArrayBinding<Matrix44F>(effect, parameter, (Matrix44F[])value);
              //#endif
            }
        }
        else if (parameter.ParameterClass == EffectParameterClass.Object)
        {
          if (parameter.ParameterType == EffectParameterType.String && value is string[])
            binding = new ConstParameterArrayBinding<string>(effect, parameter, (string[])value);

          // Note: Arrays of textures are not supported in DirectX 9.
        }
      }

      if (binding != null)
      {
        bindings.Add(binding);
        return true;
      }
      return false;
    }


    // Use the default values from Effect, as specified in the .fx file.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static void SetDefaultBinding(IGraphicsService graphicsService, EffectEx effectEx, EffectParameter parameter, EffectParameterDescription usage, EffectParameterBindingCollection bindings)
    {
      Debug.Assert(!bindings.Contains(parameter), "Effect binding already contains a binding for the given effect parameter.");

      if (parameter.ParameterClass == EffectParameterClass.Struct)
      {
        if (parameter.Elements.Count > 0)
        {
          // ----- Effect parameter is an array of structs. --> Recursively process elements of array.
          foreach (EffectParameter element in parameter.Elements)
            SetDefaultBinding(graphicsService, effectEx, element, usage, bindings);
        }
        else
        {
          // ----- Effect parameter is a struct. --> Recursively process members of struct.
          foreach (EffectParameter member in parameter.StructureMembers)
            SetDefaultBinding(graphicsService, effectEx, member, usage, bindings);
        }

        return;
      }

      // Set ConstParameterBinding using the default value stored in .fx file.
      var effect = effectEx.Resource;
      object originalValue;
      effectEx.OriginalParameterValues.TryGetValue(parameter, out originalValue);
      EffectParameterBinding binding = null;
      if (parameter.Elements.Count == 0)
      {
        // ----- Parameter is not an array.

        if (parameter.ParameterClass == EffectParameterClass.Scalar)
        {
          // Scalar values.
          if (parameter.ParameterType == EffectParameterType.Bool)
            binding = new ConstParameterBinding<bool>(effect, parameter, (bool)originalValue);
          else if (parameter.ParameterType == EffectParameterType.Int32)
            binding = new ConstParameterBinding<int>(effect, parameter, (int)originalValue);
          else if (parameter.ParameterType == EffectParameterType.Single)
            binding = new ConstParameterBinding<float>(effect, parameter, (float)originalValue);
        }
        else if (parameter.ParameterClass == EffectParameterClass.Vector
                 && parameter.ParameterType == EffectParameterType.Single)
        {
          // Vector values.
          if (parameter.ColumnCount == 2 || parameter.RowCount == 2)
            binding = new ConstParameterBinding<Vector2>(effect, parameter, (Vector2)originalValue);
          else if (parameter.ColumnCount == 3 || parameter.RowCount == 3)
            binding = new ConstParameterBinding<Vector3>(effect, parameter, (Vector3)originalValue);
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
            binding = new ConstParameterBinding<Vector4>(effect, parameter, (Vector4)originalValue);
        }
        else if (parameter.ParameterClass == EffectParameterClass.Matrix
                 && parameter.ParameterType == EffectParameterType.Single)
        {
          // Matrix value.
          binding = new ConstParameterBinding<Matrix>(effect, parameter, (Matrix)originalValue);
        }
        else if (parameter.ParameterClass == EffectParameterClass.Object)
        {
          // Object values.
          if (parameter.ParameterType == EffectParameterType.String)
          {
            binding = new ConstParameterBinding<string>(effect, parameter, (string)originalValue);
          }
          else if (parameter.ParameterType == EffectParameterType.Texture)
          {
            // A texture type but we are not sure which exact type. --> Try different types.
            try
            {
              binding = new ConstParameterBinding<Texture2D>(effect, parameter, graphicsService.GetDefaultTexture2DWhite());
            }
            catch (Exception)
            {
              try
              {
                binding = new ConstParameterBinding<Texture3D>(effect, parameter, graphicsService.GetDefaultTexture3DWhite());
              }
              catch (Exception)
              {
                try
                {
                  binding = new ConstParameterBinding<TextureCube>(effect, parameter, graphicsService.GetDefaultTextureCubeWhite());
                }
                catch (Exception)
                {
                  // Default value for a parameter of type Texture could not be read from Effect.
                }
              }
            }
          }
          else if (parameter.ParameterType == EffectParameterType.Texture1D)
          {
            // NOTE: 1D textures are not supported in XNA.
          }
          else if (parameter.ParameterType == EffectParameterType.Texture2D)
          {
            binding = new ConstParameterBinding<Texture2D>(effect, parameter, graphicsService.GetDefaultTexture2DWhite());
          }
          else if (parameter.ParameterType == EffectParameterType.Texture3D)
          {
            binding = new ConstParameterBinding<Texture3D>(effect, parameter, graphicsService.GetDefaultTexture3DWhite());
          }
          else if (parameter.ParameterType == EffectParameterType.TextureCube)
          {
            binding = new ConstParameterBinding<TextureCube>(effect, parameter, graphicsService.GetDefaultTextureCubeWhite());
          }
        }
      }
      else
      {
        // ----- Parameter is array.
        int length = parameter.Elements.Count;
        Debug.Assert(length > 0, "Effect parameter should be an array.");

        // Note: In XNA originalValue is valid. In MonoGame originalValue is null and we have to
        // create a new array!

        if (parameter.ParameterClass == EffectParameterClass.Scalar)
        {
          // Scalar value bindings.
          if (parameter.ParameterType == EffectParameterType.Bool)
            binding = new ConstParameterArrayBinding<bool>(effect, parameter, (bool[])originalValue ?? new bool[parameter.Elements.Count]);
          else if (parameter.ParameterType == EffectParameterType.Int32)
            binding = new ConstParameterArrayBinding<int>(effect, parameter, (int[])originalValue ?? new int[parameter.Elements.Count]);
          else if (parameter.ParameterType == EffectParameterType.Single)
            binding = new ConstParameterArrayBinding<float>(effect, parameter, (float[])originalValue ?? new float[parameter.Elements.Count]);
        }
        else if (parameter.ParameterClass == EffectParameterClass.Vector && parameter.ParameterType == EffectParameterType.Single)
        {
          if (parameter.ColumnCount == 2 || parameter.RowCount == 2)
            binding = new ConstParameterArrayBinding<Vector2>(effect, parameter, (Vector2[])originalValue ?? new Vector2[parameter.Elements.Count]);
          else if (parameter.ColumnCount == 3 || parameter.RowCount == 3)
            binding = new ConstParameterArrayBinding<Vector3>(effect, parameter, (Vector3[])originalValue ?? new Vector3[parameter.Elements.Count]);
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
            binding = new ConstParameterArrayBinding<Vector4>(effect, parameter, (Vector4[])originalValue ?? new Vector4[parameter.Elements.Count]);
        }
        else if (parameter.ParameterClass == EffectParameterClass.Matrix && parameter.ParameterType == EffectParameterType.Single)
        {
          binding = new ConstParameterArrayBinding<Matrix>(effect, parameter, (Matrix[])originalValue ?? new Matrix[parameter.Elements.Count]);
        }
        else if (parameter.ParameterClass == EffectParameterClass.Object)
        {
          // Note: Arrays of strings or textures are not supported in XNA.
        }
      }

      if (binding != null)
        bindings.Add(binding);
    }


    #region ----- Conversion Helpers -----

    /// <summary>
    /// Determines whether the specified value is array of the specified type.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <see langword="true"/> if the specified value is array of <typeparamref name="T"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsArray<T>(object value)
    {
      // Direct check: Should suffice in most cases.
      if (value is T[])
        return true;

      // Maybe can convert the elements to the desired type.
      var valueType = value.GetType();
      if (valueType.IsArray)
      {
        var elementType = valueType.GetElementType();
        if (ObjectHelper.IsConvertible(elementType))
          return true;
      }

      return false;
    }


    private static bool[] ToBooleanArray(object value)
    {
      Debug.Assert(IsArray<bool>(value), "Call IsArray<bool>(value) before calling ToArray<bool>(value).");

      var result = value as bool[];
      if (result != null)
        return result;

      var array = (Array)value;
      int numberOfElements = array.Length;
      result = new bool[numberOfElements];

      for (int i = 0; i < numberOfElements; i++)
        result[i] = ObjectHelper.ConvertTo<bool>(array.GetValue(i), CultureInfo.InvariantCulture);
      
      return result;
    }


    private static int[] ToInt32Array(object value)
    {
      Debug.Assert(IsArray<int>(value), "Call IsArray<int>(value) before calling ToArray<int>(value).");

      var result = value as int[];
      if (result != null)
        return result;

      var array = (Array)value;
      int numberOfElements = array.Length;
      result = new int[numberOfElements];

      for (int i = 0; i < numberOfElements; i++)
        result[i] = ObjectHelper.ConvertTo<int>(array.GetValue(i), CultureInfo.InvariantCulture);

      return result;
    }


    private static float[] ToSingleArray(object value)
    {
      Debug.Assert(IsArray<float>(value), "Call IsArray<float>(value) before calling ToArray<float>(value).");

      var result = value as float[];
      if (result != null)
        return result;

      var array = (Array)value;
      int numberOfElements = array.Length;
      result = new float[numberOfElements];
      for (int i = 0; i < numberOfElements; i++)
        result[i] = ObjectHelper.ConvertTo<float>(array.GetValue(i), CultureInfo.InvariantCulture);
      
      return result;
    }
    #endregion


    #region ----- Original Parameter Values -----

    /// <summary>
    /// Creates a dictionary of all effect parameter values.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>A dictionary of all effect parameter values.</returns>
    internal static Dictionary<EffectParameter, object> GetParameterValues(Effect effect)
    {
      var values = new Dictionary<EffectParameter, object>();
      foreach (var parameter in effect.Parameters)
        GetParameterValues(parameter, values);

      return values;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static void GetParameterValues(EffectParameter parameter, Dictionary<EffectParameter, object> values)
    {
      if (parameter.ParameterClass == EffectParameterClass.Struct)
      {
        if (parameter.Elements.Count > 0)
        {
          // ----- Effect parameter is an array of structs.
          foreach (EffectParameter element in parameter.Elements)
            GetParameterValues(element, values);
        }
        else
        {
          // ----- Effect parameter is a struct.
          foreach (EffectParameter member in parameter.StructureMembers)
            GetParameterValues(member, values);
        }

        return;
      }

      object value = null;
      if (parameter.Elements.Count == 0)
      {
        // ----- Parameter is not an array.
        if (parameter.ParameterClass == EffectParameterClass.Scalar)
        {
          // Scalar values.
          if (parameter.ParameterType == EffectParameterType.Bool)
            value = parameter.GetValueBoolean();
          else if (parameter.ParameterType == EffectParameterType.Int32)
            value = parameter.GetValueInt32();
          else if (parameter.ParameterType == EffectParameterType.Single)
            value = parameter.GetValueSingle();
        }
        else if (parameter.ParameterClass == EffectParameterClass.Vector
                 && parameter.ParameterType == EffectParameterType.Single)
        {
          // Vector values.
          if (parameter.ColumnCount == 2 || parameter.RowCount == 2)
            value = parameter.GetValueVector2();
          else if (parameter.ColumnCount == 3 || parameter.RowCount == 3)
            value = parameter.GetValueVector3();
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
            value = parameter.GetValueVector4();
        }
        else if (parameter.ParameterClass == EffectParameterClass.Matrix
                 && parameter.ParameterType == EffectParameterType.Single)
        {
          // Matrix value.
#if !MONOGAME
          value = parameter.GetValueMatrix();
#else
          // MonoGame throws exception if following condition is not met.
          if (parameter.RowCount == 4 || parameter.ColumnCount == 4)       
            value = parameter.GetValueMatrix();
#endif
        }
        else if (parameter.ParameterClass == EffectParameterClass.Object)
        {
          // Object values.
          if (parameter.ParameterType == EffectParameterType.String)
          {
            value = parameter.GetValueString();
          }
          else
          {
            // Effect parameter is texture. (Value is always null.)
          }
        }
      }
      else
      {
        // ----- Parameter is array.
        int length = parameter.Elements.Count;
        Debug.Assert(length > 0, "Effect parameter should be an array.");

        if (parameter.ParameterClass == EffectParameterClass.Scalar)
        {
#if !MONOGAME
          // Scalar value bindings.
          if (parameter.ParameterType == EffectParameterType.Bool)
            value = parameter.GetValueBooleanArray(length);
          else if (parameter.ParameterType == EffectParameterType.Int32)
            value = parameter.GetValueInt32Array(length);
          else if (parameter.ParameterType == EffectParameterType.Single)
            value = parameter.GetValueSingleArray(length);
#endif
        }
        else if (parameter.ParameterClass == EffectParameterClass.Vector && parameter.ParameterType == EffectParameterType.Single)
        {
#if !MONOGAME
          if (parameter.ColumnCount == 2 || parameter.RowCount == 2)
            value = parameter.GetValueVector2Array(length);
          else if (parameter.ColumnCount == 3 || parameter.RowCount == 3)
            value = parameter.GetValueVector3Array(length);
          else if (parameter.ColumnCount == 4 || parameter.RowCount == 4)
            value = parameter.GetValueVector4Array(length);
#endif
        }
        else if (parameter.ParameterClass == EffectParameterClass.Matrix && parameter.ParameterType == EffectParameterType.Single)
        {
#if !MONOGAME
          value = parameter.GetValueMatrixArray(length);
#endif
        }
        else if (parameter.ParameterClass == EffectParameterClass.Object)
        {
          // Note: Arrays of strings or textures are not supported in XNA.
        }
      }

      if (value != null)
        values.Add(parameter, value);
    }
    #endregion
  }
}
