// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Manages a collection of <see cref="IPresentationTarget"/>s.
  /// </summary>
  public class PresentationTargetCollection : NotifyingCollection<IPresentationTarget>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PresentationTargetCollection"/> class.
    /// </summary>
    public PresentationTargetCollection() : base(false, false)
    {
    }
  }
}
