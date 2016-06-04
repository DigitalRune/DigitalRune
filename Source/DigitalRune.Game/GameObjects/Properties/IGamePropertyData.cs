// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  // Interface for all GamePropertyData<T> classes.
  internal interface IGamePropertyData
  {
    bool HasLocalValue { get; set; }


    // Factory method to create GamePropertyData<T> instances.
    IGameProperty CreateGameProperty(GameObject owner, int propertyId);
  }
}
