// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Identifies a player.
  /// </summary>
  /// <remarks>
  /// See description of <see cref="IInputService"/> (see section "Logical Players and Game 
  /// Controllers") to find out more.
  /// </remarks>
  public enum LogicalPlayerIndex
  {
    /// <summary>
    /// Any logical player. Use this value to check input on any game controller. (Note: Only game
    /// controllers that have been assigned to players are checked. Game controllers that are not
    /// assigned to a player are ignored.)
    /// </summary>
    Any = -1,

    /// <summary>The first logical player.</summary>
    One = 0,

    /// <summary>The second logical player.</summary>
    Two,

    /// <summary>The third logical player.</summary>
    Three,

    /// <summary>The fourth logical player.</summary>
    Four,
  }
}
