// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a camera that defines a view into the 3D scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class has a <see cref="Projection"/>, which defines the camera frustum and the projection
  /// transformation.
  /// </para>
  /// <para>
  /// The view transformation (position and orientation of the camera in the world) is defined by 
  /// creating a <see cref="CameraNode"/> in a <see cref="IScene"/>. Multiple 
  /// <see cref="CameraNode"/>s can share the same <see cref="Camera"/> object.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="Camera"/>s are cloneable. When <see cref="Clone()"/> is called all properties 
  /// including the <see cref="Projection"/> are duplicated (deep copy).
  /// </para>
  /// </remarks>
  /// <seealso cref="CameraNode"/>
  public class Camera : INamedObject
  {
    // Old comment:
    ///// <para>
    ///// A real world camera model consists of an image sensor (for example a film) and an apparatus
    ///// that defines how the incoming light reaches the image sensor. This apparatus consists for 
    ///// example of lenses, mirrors and prisms that define how the image is projected on the (usually)
    ///// planar image sensor. The image sensor defines how light information is converted into a 
    ///// picture.
    ///// </para>

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of this camera.
    /// </summary>
    /// <value>The name of this camera.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets the projection.
    /// </summary>
    /// <value>The projection. The default value is a standard perspective projection.</value>
    public Projection Projection { get; private set; }


    /// <summary>
    /// Gets or sets the projection transformation of the last frame.
    /// </summary>
    /// <value>
    /// The projection transformation of the last frame. Can be <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The property <see cref="LastProjection"/> is an optional property. It stores the projection
    /// transformation of the last frame that was rendered. This information is required for 
    /// temporal reprojection, which is used in temporal caching or camera motion blur.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> This property is not updated automatically! 
    /// <see cref="LastProjection"/> needs to be set by the application logic whenever the camera
    /// projection is changed.
    /// </para>
    /// </remarks>
    public Matrix44F? LastProjection { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera"/> class with a given projection.
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="projection"/> is <see langword="null"/>.
    /// </exception>
    public Camera(Projection projection)
    {
      if (projection == null)
        throw new ArgumentNullException("projection");

      Projection = projection;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Camera"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Camera"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="Camera"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Camera"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Camera"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Camera Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Camera"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method,
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Camera"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Camera CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Camera. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="Camera"/>
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Camera"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Camera"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual Camera CreateInstanceCore()
    {
      return new Camera(Projection.Clone());
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified 
    /// <see cref="Camera"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Camera"/> derived class must 
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to 
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Camera source)
    {
      Name = source.Name;
      LastProjection = source.LastProjection;
    }
    #endregion

    #endregion
  }
}
