// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  /// <summary>
  /// Base interface for <see cref="GameProperty{T}"/>.
  /// </summary>
  public interface IGameProperty : INamedObject
  {
    /// <summary>
    /// Gets the game object that owns this property.
    /// </summary>
    /// <value>The <see cref="GameObject"/> that owns this property.</value>
    GameObject Owner { get; }


    /// <summary>
    /// Gets the property metadata.
    /// </summary>
    /// <value>The property metadata.</value>
    IGamePropertyMetadata Metadata { get; }


    /// <summary>
    /// Gets a value indicating whether this property has a local value.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this property is set to a local value; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If no value was set for a property, then the property has the same value as in the 
    /// <see cref="GameObject.Template"/> of the game object. If no template is set, then the
    /// property uses the <see cref="GamePropertyMetadata{T}.DefaultValue"/> defined in the 
    /// <see cref="GamePropertyMetadata{T}"/>. In these cases the property does not have a local
    /// value.
    /// </para>
    /// <para>
    /// If a value was set using a <strong>SetValue</strong> method of the <see cref="GameObject"/>,
    /// then the property has a local value - even if the new value is the same as the default 
    /// value!
    /// </para>
    /// <para>
    /// A local value can only be removed using the <see cref="Reset"/> method.
    /// </para>
    /// </remarks>
    bool HasLocalValue { get; }


    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    object Value { get; set; }

    
    /// <summary>
    /// Parses the specified string and updates the <see cref="GameProperty{T}.Value"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    void Parse(string value);


    /// <summary>
    /// Removes any local values and sets the property to its default value.
    /// </summary>
    /// <remarks>
    /// See description in <see cref="HasLocalValue"/>.
    /// </remarks>
    void Reset();
  }
}
