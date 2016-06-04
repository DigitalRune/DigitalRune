// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Occurs if an effect binding fails.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public class EffectBindingException : GraphicsException
  {
    /// <summary>
    /// Gets the name of the effect.
    /// </summary>
    public string EffectName { get; private set; }


    /// <summary>
    /// Gets the name of the effect parameter.
    /// </summary>
    public string EffectParameterName { get; private set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class.
    /// </summary>
    public EffectBindingException()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class with a
    /// specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EffectBindingException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class with a
    /// specified error message and a reference to the inner exception that is the cause of this
    /// exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <see langword="null"/> if no
    /// inner exception is specified.
    /// </param>
    public EffectBindingException(string message, Exception innerException)
      : base(message, innerException)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class with a
    /// specified error message and additional effect information.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="effect">The effect. Can be <see langword="null"/>.</param>
    /// <param name="effectParameter">The effect parameter. Can be <see langword="null"/>.</param>
    public EffectBindingException(string message, Effect effect, EffectParameter effectParameter)
      : base(message)
    {
      EffectName = GetName(effect);
      EffectParameterName = GetName(effectParameter);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class with a
    /// specified error message, additional effect information and a reference to the inner
    /// exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="effect">The effect. Can be <see langword="null"/>.</param>
    /// <param name="effectParameter">The effect parameter. Can be <see langword="null"/>.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <see langword="null"/> if no
    /// inner exception is specified.
    /// </param>
    public EffectBindingException(string message, Effect effect, EffectParameter effectParameter, Exception innerException)
      : base(message, innerException)
    {
      EffectName = GetName(effect);
      EffectParameterName = GetName(effectParameter);
    }


    private static string GetName(Effect effect)
    {
      if (effect != null)
      {
        if (!string.IsNullOrEmpty(effect.Name))
          return effect.Name;

        var effectType = effect.GetType();
        if (effectType != typeof(Effect))
          return effectType.Name;
      }

      return null;
    }


    private static string GetName(EffectParameter parameter)
    {
      if (parameter != null)
        return parameter.Name;

      return null;
    }


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectBindingException"/> class with 
    /// serialized data.
    /// </summary>
    /// <param name="info">
    /// The <see cref="SerializationInfo"/> that holds the serialized object data about the 
    /// exception being thrown.
    /// </param>
    /// <param name="context">
    /// The <see cref="StreamingContext"/> that contains contextual information about the source or 
    /// destination.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="info"/> parameter is null.
    /// </exception>
    /// <exception cref="SerializationException">
    /// The class name is null or <see cref="Exception.HResult"/> is zero (0).
    /// </exception>
    protected EffectBindingException(SerializationInfo info, StreamingContext context) 
      : base(info, context)
    {
      EffectName = info.GetString("EffectName");
      EffectParameterName = info.GetString("EffectParameterName");
    }


    /// <summary>
    /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with
    /// information about the exception.
    /// </summary>
    /// <param name="info">
    /// The <see cref="SerializationInfo"/> that holds the serialized object data about the
    /// exception being thrown.
    /// </param>
    /// <param name="context">
    /// The <see cref="StreamingContext"/> that contains contextual information about the source or
    /// destination.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="info"/> parameter is <see langword="null"/>.
    /// </exception>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info");

      base.GetObjectData(info, context);
      info.AddValue("EffectName", EffectName);
      info.AddValue("EffectParameterName", EffectParameterName);
    }
#endif
  }
}
