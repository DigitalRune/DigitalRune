// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  // Interface for all GameEventData<T> classes.
  internal interface IGameEventData
  {
    // Factory method to create GameEventData<T> instances.
    IGameEvent CreateGameEvent(GameObject owner, int eventId);
  }
}
