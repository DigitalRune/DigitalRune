// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  partial class EffectBinding
  {
    // ReSharper disable UnusedParameter.Local
    private void CheckHint(EffectParameterBinding binding)
    {
      if ((Hints & binding.Description.Hint) == 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Cannot set binding for effect parameter. The effect binding does not supported parameter that have the sort hint \"{0}\".",
          binding.Description.Hint);
        throw new ArgumentException(message);
      }
    }
    // ReSharper restore UnusedParameter.Local


    /// <overloads>
    /// <summary>
    /// Sets a parameter binding for the specified effect parameter.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets a <see cref="ConstParameterBinding{T}"/> for the effect parameter with the specified 
    /// name.
    /// </summary>
    /// <typeparam name="T">The value type. See <see cref="EffectParameterBinding{T}"/>.</typeparam>
    /// <param name="name">
    /// The name of the effect parameter to which the binding is applied.
    /// </param>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ConstParameterBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for the effect parameter already exists, then the
    /// existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// <see cref="Effect"/> does not contain an <see cref="EffectParameter"/> with the given name.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public ConstParameterBinding<T> Set<T>(string name, T value)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Effect parameter name is empty.", "name");

      EffectParameter parameter = Effect.Parameters[name];
      if (parameter == null)
      {
        string message = String.Format(CultureInfo.InvariantCulture, "Effect parameter \"{0}\" not found.", name);
        throw new KeyNotFoundException(message);
      }

      return Set(parameter, value);
    }


    /// <summary>
    /// Sets a <see cref="ConstParameterArrayBinding{T}"/> for the effect parameter with the 
    /// specified name.
    /// </summary>
    /// <typeparam name="T">
    /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
    /// </typeparam>
    /// <param name="name">
    /// The name of the effect parameter to which the binding is applied.
    /// </param>
    /// <param name="values">The array of values.</param>
    /// <returns>The <see cref="ConstParameterArrayBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for the effect parameter already exists, then the
    /// existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// <see cref="Effect"/> does not contain an <see cref="EffectParameter"/> with the given name.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public ConstParameterArrayBinding<T> Set<T>(string name, T[] values)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Effect parameter name is empty.", "name");

      EffectParameter parameter = Effect.Parameters[name];
      if (parameter == null)
      {
        string message = String.Format(CultureInfo.InvariantCulture, "Effect parameter \"{0}\" not found.", name);
        throw new KeyNotFoundException(message);
      }

      return Set(parameter, values);
    }


    /// <summary>
    /// Sets a <see cref="DelegateParameterBinding{T}"/> for the effect parameter with the specified
    /// name.
    /// </summary>
    /// <typeparam name="T">The value type. See <see cref="EffectParameterBinding{T}"/>.</typeparam>
    /// <param name="name">
    /// The name of the effect parameter to which the binding is applied.
    /// </param>
    /// <param name="computeParameter">The callback function that computes the value.</param>
    /// <returns>The <see cref="DelegateParameterBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for the effect parameter already exists, then the
    /// existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// <see cref="Effect"/> does not contain an <see cref="EffectParameter"/> with the given name.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegateParameterBinding<T> Set<T>(string name, Func<DelegateParameterBinding<T>, RenderContext, T> computeParameter)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Effect parameter name is empty.", "name");

      EffectParameter parameter = Effect.Parameters[name];
      if (parameter == null)
      {
        string message = String.Format(CultureInfo.InvariantCulture, "Effect parameter \"{0}\" not found.", name);
        throw new KeyNotFoundException(message);
      }

      return Set(parameter, computeParameter);
    }


    /// <summary>
    /// Sets a <see cref="DelegateParameterArrayBinding{T}"/> for the effect parameter with the 
    /// specified name.
    /// </summary>
    /// <typeparam name="T">
    /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
    /// </typeparam>
    /// <param name="name">
    /// The name of the effect parameter to which the binding is applied.
    /// </param>
    /// <param name="computeParameter">
    /// The callback function that computes the parameter values.
    /// </param>
    /// <returns>The <see cref="DelegateParameterArrayBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for the effect parameter already exists, then the
    /// existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// <see cref="Effect"/> does not contain an <see cref="EffectParameter"/> with the given name.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegateParameterArrayBinding<T> Set<T>(string name, Action<DelegateParameterArrayBinding<T>, RenderContext, T[]> computeParameter)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Effect parameter name is empty.", "name");

      EffectParameter parameter = Effect.Parameters[name];
      if (parameter == null)
      {
        string message = String.Format(CultureInfo.InvariantCulture, "Effect parameter \"{0}\" not found.", name);
        throw new KeyNotFoundException(message);
      }

      return Set(parameter, computeParameter);
    }


    /// <summary>
    /// Sets a <see cref="ConstParameterBinding{T}"/> for the specified effect parameter.
    /// </summary>
    /// <typeparam name="T">The value type. See <see cref="EffectParameterBinding{T}"/>.</typeparam>
    /// <param name="parameter">
    /// The effect parameter to which the binding is applied.
    /// </param>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ConstParameterBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for <paramref name="parameter"/> already exists,
    /// then the existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public ConstParameterBinding<T> Set<T>(EffectParameter parameter, T value)
    {
      ConstParameterBinding<T> binding;
      int index = ParameterBindings.IndexOf(parameter);
      if (index >= 0)
      {
        // An effect parameter binding already exists.
        binding = ParameterBindings[index] as ConstParameterBinding<T>;
        if (binding != null)
        {
          // Update existing binding.
          binding.Value = value;
        }
        else
        {
          // Replace existing binding.
          binding = new ConstParameterBinding<T>(Effect, parameter, value);
          ParameterBindings[index] = binding;
        }
      }
      else
      {
        // Create a new binding.
        binding = new ConstParameterBinding<T>(Effect, parameter, value);
        CheckHint(binding);
        ParameterBindings.Add(binding);
      }

      return binding;
    }


    /// <summary>
    /// Sets a <see cref="ConstParameterArrayBinding{T}"/> for the specified effect parameter.
    /// </summary>
    /// <typeparam name="T">
    /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
    /// </typeparam>
    /// <param name="parameter">
    /// The effect parameter to which the binding is applied.
    /// </param>
    /// <param name="values">The array of values.</param>
    /// <returns>The <see cref="ConstParameterArrayBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for <paramref name="parameter"/> already exists,
    /// then the existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public ConstParameterArrayBinding<T> Set<T>(EffectParameter parameter, T[] values)
    {
      ConstParameterArrayBinding<T> binding;
      int index = ParameterBindings.IndexOf(parameter);
      if (index >= 0)
      {
        // An effect parameter binding already exists.
        binding = ParameterBindings[index] as ConstParameterArrayBinding<T>;
        if (binding != null)
        {
          // Update existing binding.
          binding.Values = values;
        }
        else
        {
          // Replace existing binding.
          binding = new ConstParameterArrayBinding<T>(Effect, parameter, values);
          ParameterBindings[index] = binding;
        }
      }
      else
      {
        // Create a new binding.
        binding = new ConstParameterArrayBinding<T>(Effect, parameter, values);
        CheckHint(binding);
        ParameterBindings.Add(binding);
      }

      return binding;
    }


    /// <summary>
    /// Sets a <see cref="DelegateParameterBinding{T}"/> for the specified effect parameter.
    /// </summary>
    /// <typeparam name="T">The value type. See <see cref="EffectParameterBinding{T}"/>.</typeparam>
    /// <param name="parameter">
    /// The effect parameter to which the binding is applied.
    /// </param>
    /// <param name="computeParameter">The callback function that computes the value.</param>
    /// <returns>The <see cref="DelegateParameterBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for <paramref name="parameter"/> already exists,
    /// then the existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegateParameterBinding<T> Set<T>(EffectParameter parameter, Func<DelegateParameterBinding<T>, RenderContext, T> computeParameter)
    {
      DelegateParameterBinding<T> binding;
      int index = ParameterBindings.IndexOf(parameter);
      if (index >= 0)
      {
        // An effect parameter binding already exists.
        binding = ParameterBindings[index] as DelegateParameterBinding<T>;
        if (binding != null)
        {
          // Update existing binding.
          binding.ComputeParameter = computeParameter;
        }
        else
        {
          // Replace existing binding.
          binding = new DelegateParameterBinding<T>(Effect, parameter, computeParameter);
          ParameterBindings[index] = binding;
        }
      }
      else
      {
        // Create a new binding.
        binding = new DelegateParameterBinding<T>(Effect, parameter, computeParameter);
        CheckHint(binding);
        ParameterBindings.Add(binding);
      }

      return binding;
    }


    /// <summary>
    /// Sets a <see cref="DelegateParameterArrayBinding{T}"/> for the specified effect parameter.
    /// </summary>
    /// <typeparam name="T">
    /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
    /// </typeparam>
    /// <param name="parameter">The effect parameter to which the binding is applied.</param>
    /// <param name="computeParameter">
    /// The callback function that computes the parameter values.
    /// </param>
    /// <returns>The <see cref="DelegateParameterArrayBinding{T}"/> that has been set.</returns>
    /// <remarks>
    /// If an appropriate effect parameter binding for <paramref name="parameter"/> already exists,
    /// then the existing binding is updated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegateParameterArrayBinding<T> Set<T>(EffectParameter parameter, Action<DelegateParameterArrayBinding<T>, RenderContext, T[]> computeParameter)
    {
      DelegateParameterArrayBinding<T> binding;
      int index = ParameterBindings.IndexOf(parameter);
      if (index >= 0)
      {
        // An effect parameter binding already exists.
        binding = ParameterBindings[index] as DelegateParameterArrayBinding<T>;
        if (binding != null)
        {
          // Update existing binding.
          binding.ComputeParameter = computeParameter;
        }
        else
        {
          // Replace existing binding.
          binding = new DelegateParameterArrayBinding<T>(Effect, parameter, computeParameter);
          ParameterBindings[index] = binding;
        }
      }
      else
      {
        // Create a new binding.
        binding = new DelegateParameterArrayBinding<T>(Effect, parameter, computeParameter);
        CheckHint(binding);
        ParameterBindings.Add(binding);
      }

      return binding;
    }
  }
}
