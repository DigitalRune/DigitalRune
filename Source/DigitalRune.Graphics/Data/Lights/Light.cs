// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the properties of a light source.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Light"/> is the base class for all light sources: see <see cref="AmbientLight"/>,
  /// <see cref="DirectionalLight"/>, <see cref="PointLight"/>, <see cref="Spotlight"/>, and
  /// <see cref="ProjectorLight"/>. A <see cref="Light"/> defines the properties of a light source,
  /// like color, intensity, extent, etc. However, it does not define the position of a light 
  /// source, or its direction. Position and orientation are defined by creating a 
  /// <see cref="LightNode"/> and adding it to a <see cref="IScene"/>. Multiple 
  /// <see cref="LightNode"/>s can share the same <see cref="Light"/> object.
  /// </para>
  /// <para>
  /// Each light has a <see cref="Shape"/> - a 3D shape that defines the space that is lit by the 
  /// light source. See <see cref="Shape"/> for more information.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="Light"/>s are cloneable. <see cref="Clone"/> creates a deep copy of the current 
  /// light source - unless documented otherwise (see derived classes). Most properties including 
  /// the <see cref="Shape"/> are duplicated.
  /// </para>
  /// </remarks>
  public abstract class Light : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the light.
    /// </summary>
    /// <value>The name of the light.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets (or sets) the shape of the light volume.
    /// </summary>
    /// <value>
    /// A <see cref="Geometry.Shapes.Shape"/> that describes the light volume (the area that is hit 
    /// by the light).
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="Shape"/> defines the space that is lit by the light source. The shape depends
    /// on the type of light (see derived classes). For example, the light volume of a 
    /// <see cref="PointLight"/> is a <see cref="SphereShape"/> where the radius of the sphere 
    /// matches the range of the light. The light volume is used by the <see cref="Scene"/> to 
    /// determine which objects are lit by a certain light source. 
    /// </para>
    /// <para>
    /// This shape is used for culling using bounding shapes. It is not used to clip the lit area: 
    /// If the bounding shape of a mesh touches this shape, then the whole mesh is lit - not only
    /// the overlapping part! The <see cref="LightNode.Clip"/> property of a <see cref="LightNode"/>
    /// can be used to define a clipping volume.
    /// </para>
    /// <para>
    /// Some light classes may allow to change the shape. But the shape should not be replaced while
    /// the <see cref="Light"/> is in use, i.e. referenced by a 
    /// <see cref="LightNode"/>. For example, if the bounding shape is a <see cref="SphereShape"/>,
    /// the radius of the sphere can be changed at any time. But it is not allowed to replace the 
    /// <see cref="SphereShape"/> with a <see cref="BoxShape"/> as long as the light is used in a
    /// scene. Replacing the bounding shape will not raise any exceptions, but the light node may
    /// not use the new shape, hence it may not be rendered as desired.
    /// </para>
    /// </remarks>
    public Shape Shape { get; protected set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Light"/> class.
    /// </summary>
    protected Light()
    {
      // Set a default shape.
      Shape = Shape.Infinite;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Light"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Light"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="Light"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Light"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Light"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Light Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Light"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="Light"/> method, which this 
    /// method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Light"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Light CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Light. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="Light"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Light"/> derived class must implement this method. A typical implementation is to
    /// simply call the default constructor and return the result. 
    /// </para>
    /// </remarks>
    protected abstract Light CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="Light"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Light"/> derived class must implement
    /// this method. A typical implementation is to call <c>base.CloneCore(this)</c> to copy all 
    /// properties of the base class and then copy all properties of the derived class.
    /// </para>
    /// <para>
    /// Note that the base class <see cref="Light"/> does not copy or clone the <see cref="Shape"/>
    /// property. The derived class needs to take care of the <see cref="Shape"/> property.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Light source)
    {
      Name = source.Name;

      // Shape does not need to be cloned. It is managed automatically in derived classes.
    }
    #endregion


    /// <summary>
    /// Gets the (approximated) light intensity at the given distance.
    /// </summary>
    /// <param name="distance">The distance from the light.</param>
    /// <returns>
    /// A value representing the (red, green and blue) light intensity at the specified distance. 
    /// </returns>
    public abstract Vector3F GetIntensity(float distance);
    #endregion
  }
}
