// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines the value of an animation at a certain point in time.
  /// </summary>
  /// <typeparam name="T">The type of the value stored in the key frame.</typeparam>
  public interface IKeyFrame<T>
  {
    // Note: Type can be made covariant IKeyFrame<out T>. But the .NET Compact Framework does
    // currently not support C# 4.0 language features.


    /// <summary>
    /// Gets the time offset from the start of the animation to the key frame.
    /// </summary>
    /// <value>The time of the key frame.</value>
    TimeSpan Time { get; }


    /// <summary>
    /// Gets the animation value of the key frame.
    /// </summary>
    /// <value>The animation value of the key frame.</value>
    T Value { get; }
  }
}
