// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents view-dependent LOD data which is stored per camera.
  /// </summary>
  internal class LodData : IDisposable
  {
    public LodSelection Selection;  // The current LOD or LOD transition.
    public int Frame;               // The frame number in which the LOD was selected.


    public LodData()
    {
      Selection = new LodSelection();
      Frame = -1;
    }


    public void Dispose()
    {
      Selection = new LodSelection();
      Frame = -1;
    }
  }
}
