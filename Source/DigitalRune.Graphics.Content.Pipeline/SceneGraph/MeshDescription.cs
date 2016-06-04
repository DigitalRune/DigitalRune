// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Graphics.Content.Pipeline
{
  internal class MeshDescription
  {
    public string Name { get; set; }
    public bool GenerateTangentFrames { get; set; }
    public float MaxDistance { get; set; }
    public float LodDistance { get; set; }
    public List<SubmeshDescription> Submeshes { get; set; }


    public SubmeshDescription GetSubmeshDescription(int submeshIndex)
    {
      if (Submeshes == null || submeshIndex < 0 || submeshIndex >= Submeshes.Count)
        return null;

      return Submeshes[submeshIndex];
    }
  }
}
