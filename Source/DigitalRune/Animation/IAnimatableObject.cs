// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Represents an object that can be animated.
  /// </summary>
  /// <remarks>
  /// An <see cref="IAnimatableObject"/> is an object that can be animated. It has properties that 
  /// can be controlled by animations. 
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Animatable")]
  public interface IAnimatableObject : INamedObject
  {
    /// <summary>
    /// Gets either the properties which are currently animated, or all properties which can be 
    /// animated. (See remarks.)
    /// </summary>
    /// <returns>
    /// The properties which are currently animated, or the all properties which can be animated.
    /// (See remarks.)
    /// </returns>
    /// <remarks>
    /// This method is required by the animation system to stop all animations running on this 
    /// object. The type that implements this method can either:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Variant #1: Return only the properties which are currently being animated.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Variant #2: Return all properties which can be animated - independent of whether they are 
    /// currently being animated or not.
    /// </description>
    /// </item>
    /// </list>
    /// The first implementation (Variant #1) is preferred by the animation system, but in some 
    /// cases it is not easily possible to determine which properties are currently being animated. 
    /// In this case the <see cref="IAnimatableObject"/> may simple return all properties (Variant 
    /// #2).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    IEnumerable<IAnimatableProperty> GetAnimatedProperties();


    /// <summary>
    /// Gets the property with given name and type which can be animated.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// The <see cref="IAnimatableProperty"/> that has the given name and type; otherwise, 
    /// <see langword="null"/> if the object does not have an animatable property with this name or 
    /// type.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Animatable")]
    IAnimatableProperty<T> GetAnimatableProperty<T>(string name);
  }
}
