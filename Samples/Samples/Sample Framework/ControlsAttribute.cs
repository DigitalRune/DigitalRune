using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DigitalRune.Game;
using Microsoft.Xna.Framework;


namespace Samples
{
  /// <summary>
  /// Describes the controls (e.g. keyboard keys) used by an XNA <see cref="GameComponent"/> or a
  /// DigitalRune <see cref="GameObject"/>.
  /// </summary>
  /// <remarks>
  /// The description will be printed in the Help window.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public class ControlsAttribute : Attribute
  {
    /// <summary>
    /// Gets the description of the controls.
    /// </summary>
    /// <value>The control description.</value>
    public string Description { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ControlsAttribute" /> class.
    /// </summary>
    /// <param name="description">The description.</param>
    public ControlsAttribute(string description)
    {
      if (description == null)
        throw new ArgumentNullException("description");
      
      Description = description;
    }


    /// <summary>
    /// Gets the controls attribute for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The controls attributes.</returns>
    public static IEnumerable<ControlsAttribute> GetControlsAttribute(Type type)
    {
#if NETFX_CORE
      return type.GetTypeInfo().GetCustomAttributes<ControlsAttribute>();
#else
      return type.GetCustomAttributes(true).OfType<ControlsAttribute>();
#endif
    }
  }
}
