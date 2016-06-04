// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Provides arguments for <see cref="UIControl.InputProcessing"/> and
  /// <see cref="UIControl.InputProcessed"/> events.
  /// </summary>
  public class InputEventArgs : EventArgs
  {
    /// <summary>
    /// Gets or sets the input context.
    /// </summary>
    /// <value>The input context.</value>
    public InputContext Context { get; set; }
  }
}
