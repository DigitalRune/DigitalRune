// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  /// <summary>
  /// Defines an object that has a <see cref="Shape"/> and a <see cref="Pose"/> (position and 
  /// orientation). (Default implementation of <see cref="IGeometricObject"/>.)
  /// </summary>
  /// <inheritdoc cref="IGeometricObject"/>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [DebuggerDisplay("{GetType().Name,nq}(Shape = {Shape})")]
  public class GeometricObject : IGeometricObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the axis-aligned bounding box (AABB).
    /// </summary>
    /// <value>The axis-aligned bounding box (AABB).</value>
    public Aabb Aabb
    {
      get
      {
        if (_aabbIsValid == false)
        {
          _aabb = Shape.GetAabb(Scale, Pose);
          _aabbIsValid = true;
        }
        return _aabb;
      }
    }
    private Aabb _aabb;
    private bool _aabbIsValid;


    /// <summary>
    /// Gets or sets the pose (position and orientation).
    /// </summary>
    /// <inheritdoc/>
    public Pose Pose
    {
      get { return _pose; }
      set
      {
        if (_pose != value)
        {
          _pose = value;
          OnPoseChanged(EventArgs.Empty);
        }
      }
    }
    private Pose _pose;


    /// <summary>
    /// Gets or sets the shape.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Shape Shape
    {
      get { return _shape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_shape != null)
          _shape.Changed -= OnShapeChanged;

        _shape = value;
        _shape.Changed += OnShapeChanged;
        OnShapeChanged(ShapeChangedEventArgs.Empty);
      }
    }
    private Shape _shape;


    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    /// <value>
    /// The scale factors for the dimensions x, y and z. The default value is (1, 1, 1), which means
    /// "no scaling".
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is a scale factor that scales the <see cref="Shape"/> of this geometric object.
    /// The scale can even be negative to mirror an object.
    /// </para>
    /// <para>
    /// Changing this value does not actually change any values in the <see cref="Shape"/> instance.
    /// Collision algorithms and anyone who uses this geometric object must use the shape and apply
    /// the scale factor as appropriate. The scale is automatically applied in the property
    /// <see cref="Aabb"/>.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="ShapeChanged"/> event.
    /// </para>
    /// </remarks>
    public Vector3F Scale
    {
      get { return _scale; }
      set
      {
        if (_scale != value)
        {
          _scale = value;
          OnShapeChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _scale;


    /// <inheritdoc/>
    public event EventHandler<EventArgs> PoseChanged;


    /// <inheritdoc/>
    public event EventHandler<ShapeChangedEventArgs> ShapeChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class.
    /// </summary>
    public GeometricObject()
    {
      _shape = Shape.Empty;
      _scale = Vector3F.One;
      _pose = Pose.Identity;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class with a shape.
    /// </summary>
    /// <param name="shape">
    /// The shape (must not be <see langword="null"/>). See property <see cref="Shape"/> for more 
    /// details.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public GeometricObject(Shape shape)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");

      _shape = shape;
      _shape.Changed += OnShapeChanged;
      _scale = Vector3F.One;
      _pose = Pose.Identity;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class with shape and scale.
    /// </summary>
    /// <param name="shape">
    /// The shape (must not be <see langword="null"/>). See property <see cref="Shape"/> for more
    /// details.
    /// </param>
    /// <param name="scale">The scale.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public GeometricObject(Shape shape, Vector3F scale)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");

      _shape = shape;
      _shape.Changed += OnShapeChanged;
      _scale = scale;
      _pose = Pose.Identity;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class with shape and pose
    /// (position and orientation).
    /// </summary>
    /// <param name="shape">
    /// The shape (must not be <see langword="null"/>). See property <see cref="Shape"/> for more 
    /// details.
    /// </param>
    /// <param name="pose">The pose (position and orientation).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public GeometricObject(Shape shape, Pose pose)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");

      _shape = shape;
      _shape.Changed += OnShapeChanged;
      _scale = Vector3F.One;
      _pose = pose;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class with shape, scale and 
    /// pose (position and orientation).
    /// </summary>
    /// <param name="shape">
    /// The shape (must not be <see langword="null"/>). See property <see cref="Shape"/> for more
    /// details.
    /// </param>
    /// <param name="scale">The scale.</param>
    /// <param name="pose">The pose (position and orientation).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public GeometricObject(Shape shape, Vector3F scale, Pose pose)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");

      _shape = shape;
      _shape.Changed += OnShapeChanged;
      _scale = scale;
      _pose = pose;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    IGeometricObject IGeometricObject.Clone()
    {
      return Clone();
    }


    /// <summary>
    /// Creates a new <see cref="GeometricObject"/> that is a clone (deep copy) of the current 
    /// instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="GeometricObject"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="GeometricObject"/> derived class and <see cref="CloneCore"/> to create a copy of 
    /// the current instance. Classes that derive from <see cref="GeometricObject"/> need to 
    /// implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public GeometricObject Clone()
    {
      GeometricObject clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GeometricObject"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a protected method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone shape. A class derived from <see cref="GeometricObject"/> does not implement 
    /// <see cref="CreateInstanceCore"/>."
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private GeometricObject CreateInstance()
    {
      GeometricObject newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone geometric object. The derived class {0} does not implement CreateInstanceCore().",
          GetType());

        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="GeometricObject"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="GeometricObject"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="GeometricObject"/> derived class must
    /// implement this method. A typical implementation is to simply call the default constructor
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual GeometricObject CreateInstanceCore()
    {
      return new GeometricObject();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="GeometricObject"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="GeometricObject"/> derived class must
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(GeometricObject source)
    {
      Pose = source.Pose;
      Shape = source.Shape.Clone();
      Scale = source.Scale;
    }
    #endregion


    /// <summary>
    /// Called when the <see cref="Shape"/> property has changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      OnShapeChanged(eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="PoseChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPoseChanged(EventArgs)"/> 
    /// in a derived class, be sure to call the base class's <see cref="OnPoseChanged(EventArgs)"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnPoseChanged(EventArgs eventArgs)
    {
      _aabbIsValid = false;

      var handler = PoseChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="ShapeChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="ShapeChangedEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding 
    /// <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> in a derived class, be sure to call the
    /// base class's <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnShapeChanged(ShapeChangedEventArgs eventArgs)
    {
      _aabbIsValid = false;

      var handler = ShapeChanged;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
