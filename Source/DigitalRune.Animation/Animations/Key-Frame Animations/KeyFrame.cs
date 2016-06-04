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
  public class KeyFrame<T> : IKeyFrame<T>
  {
    /// <summary>
    /// Gets or sets the time offset from the start of the animation to this key frame.
    /// </summary>
    /// <value>The time value of the key frame.</value>
    public TimeSpan Time { get; set; }


    /// <summary>
    /// Gets or sets the animation value for this key frame.
    /// </summary>
    /// <value>The animation value of the key frame.</value>
    public T Value { get; set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyFrame{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyFrame{T}"/> class.
    /// </summary>
    public KeyFrame()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="KeyFrame{T}"/> class with the given time and
    /// value.
    /// </summary>
    /// <param name="time">The time value of the key frame.</param>
    /// <param name="value">The animation value of the key frame.</param>
    public KeyFrame(TimeSpan time, T value)
    {
      Time = time;
      Value = value;
    }
  }
}
