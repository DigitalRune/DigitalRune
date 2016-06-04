// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;


namespace DigitalRune.Game.Input
{
  partial class InputManager
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public TouchCollection TouchCollection
    {
      get { return _touchCollection; }
    }
    private TouchCollection _touchCollection;


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<GestureSample> Gestures
    {
      get { return _gestures; }
    }
    private readonly List<GestureSample> _gestures;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Signature should be consistent with other Update methods.")]
    private void UpdateTouch(TimeSpan deltaTime)
    {
      // Touch input
      _touchCollection = TouchPanel.GetState();

      // Touch gestures
      _gestures.Clear();
      while (TouchPanel.IsGestureAvailable)
        _gestures.Add(TouchPanel.ReadGesture());
    }
    #endregion
  }
}
#endif
