// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  /// <summary>
  /// Identifies and describes a game object property.
  /// </summary>
  public interface IGamePropertyMetadata : INamedObject
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
    /// This can be any string, like "Appearance", "Behavior", that can be used to group properties
    /// in a game editor. See <see cref="GamePropertyCategories"/> for a list of default categories.
    /// </remarks>
    /// <seealso cref="GamePropertyCategories"/>
    string Category { get; set; }


    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    /// <remarks>
    /// This can be any string that describes the property for users of a game editor.
    /// </remarks>
    string Description { get; set; }


    /// <summary>
    /// Gets the default value.
    /// </summary>
    /// <value>The default value.</value>
    object DefaultValue { get; set; }
  }
}
