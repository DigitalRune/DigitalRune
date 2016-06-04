// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Represents a collection of <see cref="BoneMapper"/> instances.
  /// </summary>
  public class BoneMapperCollection : NotifyingCollection<BoneMapper>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BoneMapperCollection"/> class.
    /// </summary>
    public BoneMapperCollection()
      : base(false, true)
    {
    }
  }
}
