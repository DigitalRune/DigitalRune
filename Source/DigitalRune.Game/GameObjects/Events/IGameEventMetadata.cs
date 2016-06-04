// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  /// <summary>
  /// Identifies and describes a game object event.
  /// </summary>
  public interface IGameEventMetadata : INamedObject
  {
    /// <summary>
    /// Gets the unique ID.
    /// </summary>
    /// <value>The unique ID.</value>
    int Id { get; }


    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    /// <value>The category.</value>
    /// <remarks>
    /// This can be any string, like "Appearance", "Behavior", that can be used to group events in a
    /// game editor. See <see cref="GamePropertyCategories"/> for a list of default categories.
    /// </remarks>
    /// <seealso cref="GamePropertyCategories"/>
    string Category { get; set; }


    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    /// <remarks>
    /// This can be any string that describes the event for users of a game editor.
    /// </remarks>
    string Description { get; set; }


    /// <summary>
    /// Gets the default event arguments.
    /// </summary>
    /// <value>The default event arguments.</value>
    /// <remarks>
    /// If the event is raised without user-defined event arguments, the default arguments are
    /// passed to the event handlers.
    /// </remarks>
    EventArgs DefaultEventArgs { get; }
  }
}
