// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Defines how the property influences layout and rendering.
  /// </summary>
  [Flags]
  public enum UIPropertyOptions
  {
    /// <summary>
    /// No options are specified; the property does not influence the UI.
    /// </summary>
    None = 0,

    /// <summary>
    /// The measure pass of layout compositions is affected by value changes to this property. 
    /// </summary>
    AffectsMeasure = 1,

    /// <summary>
    /// The arrange pass of layout compositions is affected by value changes to this property. 
    /// </summary>
    AffectsArrange = 2,

    //AffectsParentMeasure,
    //AffectsParentArrange,

    /// <summary>
    /// Some aspect of rendering or layout composition (other than measure or arrange) is affected 
    /// by value changes to this property.
    /// </summary>
    AffectsRender = 4,
  }
}
