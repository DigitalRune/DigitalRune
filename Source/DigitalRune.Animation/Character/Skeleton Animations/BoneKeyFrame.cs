// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  // Animated bones often only need Rotation, or Rotation + Translation. Therefore,
  // we have different bone key frame types to save memory.

  internal enum BoneKeyFrameType
  {
    R,
    RT,
    SRT
  }


  internal struct BoneKeyFrameR
  {
    public TimeSpan Time;
    public QuaternionF Rotation;
  }


  internal struct BoneKeyFrameRT
  {
    public TimeSpan Time;
    public QuaternionF Rotation;
    public Vector3F Translation;
  }


  internal struct BoneKeyFrameSRT
  {
    public TimeSpan Time;
    public SrtTransform Transform;
  }
}
