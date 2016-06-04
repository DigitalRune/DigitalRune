// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Provides arguments for the <see cref="State.Enter"/>, <see cref="State.Update"/> and
  /// <see cref="State.Exit"/> events of a <see cref="State"/>.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public class StateEventArgs : EventArgs
  {
    /// <summary>
    /// Represents an event with no event data.
    /// </summary>
    /// <remarks>
    /// The value of <see cref="Empty"/> is a read-only instance of <see cref="StateEventArgs"/> 
    /// equivalent to the result of calling the <see cref="StateEventArgs"/> constructor.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public new static readonly StateEventArgs Empty = new StateEventArgs();


    /// <summary>
    /// Gets the size of the current time step.
    /// </summary>
    /// <value>The size of the current time step.</value>
    public TimeSpan DeltaTime { get; internal set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="StateEventArgs"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="StateEventArgs"/> class.
    /// </summary>
    public StateEventArgs()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StateEventArgs"/> class for the given time
    /// step.
    /// </summary>
    /// <param name="deltaTime">The size of the current time step.</param>
    public StateEventArgs(TimeSpan deltaTime)
    {
      DeltaTime = deltaTime;
    }
  }
}
