// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  /// <summary>
  /// Defines an object that has a <see cref="Shape"/> and a <see cref="Pose"/> (position and 
  /// orientation).
  /// </summary>
  /// <remarks>
  /// <para>
  /// Classes that need to implement <see cref="IGeometricObject"/> can derive from 
  /// <see cref="GeometricObject"/>.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> An <see cref="IGeometricObject"/> instance registers event 
  /// handlers for the <see cref="Shapes.Shape.Changed"/> event of the contained 
  /// <see cref="Shape"/>. Therefore, a <see cref="Shapes.Shape"/> will have an indirect reference
  /// to the <see cref="IGeometricObject"/>. This is no problem if the geometric object exclusively
  /// owns the shape. However, this could lead to problems ("life extension bugs" a.k.a. "memory
  /// leaks") when multiple geometric objects share the same shape: One geometric object is no
  /// longer used but it cannot be collected by the garbage collector because the shape still holds
  /// a reference to the object.
  /// </para>
  /// <para>
  /// Therefore, when <see cref="Shapes.Shape"/>s are shared between multiple 
  /// <see cref="IGeometricObject"/>s: Always set the property <see cref="Shape"/> to 
  /// <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> when the <see cref="IGeometricObject"/> 
  /// is no longer used. <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> is a special 
  /// immutable shape that never raises any <see cref="Shapes.Shape.Changed"/> events. Setting 
  /// <see cref="Shape"/> to <see cref="Shapes.Shape.Empty"/> ensures that the internal event 
  /// handlers are unregistered and the object can be garbage-collected properly.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> Geometric objects are cloneable. The method <see cref="Clone"/> 
  /// creates a deep copy of the geometric object - except when documented otherwise (see 
  /// description of implementing classes).
  /// </para>
  /// <para>
  /// <strong>Notes to Inheritors:</strong> The support for cloning of geometric objects is built-in
  /// in DigitalRune Geometry because several applications built upon the library rely on the 
  /// cloning mechanism. The DigitalRune Geometry library internally does not use the cloning 
  /// mechanism. So, if you need to write your own type that implements 
  /// <see cref="IGeometricObject"/> and your own application does not require to clone geometric 
  /// objects, you can simply omit the implementation of <see cref="Clone"/> - just throw a 
  /// <see cref="NotImplementedException"/>.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following is basically the implementation of <see cref="GeometricObject"/>. The source 
  /// code is shown here to illustrate how the interface <see cref="IGeometricObject"/> should be 
  /// implemented.
  /// <code lang="csharp">
  /// <![CDATA[
  /// using System;
  /// using DigitalRune.Geometry.Shapes;
  /// using DigitalRune.Mathematics.Algebra;
  /// 
  /// 
  /// namespace DigitalRune.Geometry
  /// {
  ///   [Serializable]
  ///   public class GeometricObject : IGeometricObject
  ///   {
  ///     //--------------------------------------------------------------
  ///     #region Properties & Events
  ///     //--------------------------------------------------------------
  /// 
  ///     public Aabb Aabb
  ///     {
  ///       get
  ///       {
  ///         if (_aabbIsValid == false)
  ///         {
  ///           _aabb = Shape.GetAabb(Scale, Pose);
  ///           _aabbIsValid = true;
  ///         }
  /// 
  ///         return _aabb;
  ///       }
  ///     }
  ///     private Aabb _aabb;
  ///     private bool _aabbIsValid;
  /// 
  /// 
  ///     public Pose Pose
  ///     {
  ///       get 
  ///       { 
  ///         return _pose; 
  ///       }
  ///       set
  ///       {
  ///         if (_pose != value)
  ///         {
  ///           _pose = value;
  ///           OnPoseChanged(EventArgs.Empty);
  ///         }
  ///       }
  ///     }
  ///     private Pose _pose;
  /// 
  /// 
  ///     public Shape Shape
  ///     {
  ///       get 
  ///       {   
  ///         return _shape; 
  ///       }
  ///       set
  ///       {
  ///         if (value == null)
  ///           throw new ArgumentNullException("value");
  /// 
  ///         if (_shape != null)
  ///           _shape.Changed -= OnShapeChanged;
  /// 
  ///         _shape = value;
  ///         _shape.Changed += OnShapeChanged;
  ///         OnShapeChanged(ShapeChangedEventArgs.Empty);
  ///       }
  ///     }
  ///     private Shape _shape;
  /// 
  /// 
  ///     public Vector3F Scale
  ///     {
  ///       get 
  ///       {
  ///         return _scale; 
  ///       }
  ///       set
  ///       {
  ///         if (_scale != value)
  ///         {
  ///           _scale = value;
  ///           OnShapeChanged(ShapeChangedEventArgs.Empty);
  ///         }
  ///       }
  ///     }
  ///     private Vector3F _scale;
  /// 
  /// 
  ///     public event EventHandler<EventArgs> PoseChanged;
  /// 
  /// 
  ///     public event EventHandler<ShapeChangedEventArgs> ShapeChanged;
  ///     #endregion
  /// 
  /// 
  ///     //--------------------------------------------------------------
  ///     #region Creation & Cleanup
  ///     //--------------------------------------------------------------
  /// 
  ///     public GeometricObject()
  ///     {
  ///       _shape = Shape.Empty;
  ///       _scale = Vector3F.One;
  ///       _pose = Pose.Identity;
  ///     }
  ///     #endregion
  /// 
  /// 
  ///     //--------------------------------------------------------------
  ///     #region Methods
  ///     //--------------------------------------------------------------
  /// 
  ///     IGeometricObject IGeometricObject.Clone()
  ///     {
  ///       return Clone();
  ///     } 
  /// 
  /// 
  ///     public GeometricObject Clone()
  ///     {
  ///       GeometricObject clone = CreateInstance();
  ///       clone.CloneCore(this);
  ///       return clone;
  ///     }
  /// 
  /// 
  ///     private GeometricObject CreateInstance()
  ///     {
  ///       GeometricObject newInstance = CreateInstanceCore();
  ///       if (GetType() != newInstance.GetType())
  ///       {
  ///         string message = String.Format(
  ///           "Cannot clone shape. The derived class {0} does not implement CreateInstanceCore().",
  ///           GetType());
  ///         throw new InvalidOperationException(message);
  ///       }
  /// 
  ///       return newInstance;
  ///     }
  /// 
  /// 
  ///     protected virtual GeometricObject CreateInstanceCore()
  ///     {
  ///       return new GeometricObject();
  ///     }
  /// 
  /// 
  ///     protected virtual void CloneCore(GeometricObject sourceShape)
  ///     {
  ///       Pose = sourceShape.Pose;
  ///       Shape = sourceShape.Shape.Clone();
  ///       Scale = sourceShape.Scale;
  ///     }
  /// 
  /// 
  ///     private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
  ///     {
  ///       OnShapeChanged(eventArgs);
  ///     }
  /// 
  /// 
  ///     protected virtual void OnPoseChanged(EventArgs eventArgs)
  ///     {
  ///       _aabbIsValid = false;
  /// 
  ///       var handler = PoseChanged;
  /// 
  ///       if (handler != null)
  ///         handler(this, eventArgs);
  ///     }
  /// 
  /// 
  ///     protected virtual void OnShapeChanged(ShapeChangedEventArgs eventArgs)
  ///     {
  ///       _aabbIsValid = false;
  /// 
  ///       var handler = ShapeChanged;
  /// 
  ///       if (handler != null)
  ///         handler(this, eventArgs);
  ///     }
  ///     #endregion
  ///   }
  /// }
  /// ]]>
  /// </code>
  /// </example>
  public interface IGeometricObject 
  {
    /// <summary>
    /// Gets the axis-aligned bounding box (AABB).
    /// </summary>
    /// <value>The axis-aligned bounding box (AABB).</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    Aabb Aabb { get; }


    /// <summary>
    /// Gets the pose (position and orientation).
    /// </summary>
    /// <value>The pose (position and orientation).</value>
    /// <remarks>
    /// <para>
    /// Changing this property raises the <see cref="PoseChanged"/> event.
    /// </para>
    /// </remarks>
    Pose Pose { get; }


    /// <summary>
    /// Gets the shape.
    /// </summary>
    /// <value>
    /// The shape. The shape must not be <see langword="null"/>, but it can be of type 
    /// <see cref="EmptyShape"/> (see <see cref="Shapes.Shape.Empty"/>).
    /// </value>
    /// <remarks>
    /// <para>
    /// Changing this property raises the <see cref="ShapeChanged"/> event.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> An <see cref="IGeometricObject"/> instance registers event 
    /// handlers for the <see cref="Shapes.Shape.Changed"/> event of the contained 
    /// <see cref="Shape"/>. Therefore, a <see cref="Shapes.Shape"/> will have an indirect
    /// reference to the <see cref="IGeometricObject"/>. This is no problem if the geometric object
    /// exclusively owns the shape. However, this could lead to problems ("life extension bugs"
    /// a.k.a. "memory leaks") when multiple geometric objects share the same shape: One geometric
    /// object is no longer used but it cannot be collected by the garbage collector because the
    /// shape still holds a reference to the object.
    /// </para>
    /// <para>
    /// Therefore, when <see cref="Shapes.Shape"/>s are shared between multiple 
    /// <see cref="IGeometricObject"/>s: Always set the property <see cref="Shape"/> to 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> when the <see cref="IGeometricObject"/> 
    /// is no longer used. <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> is a special 
    /// immutable shape that never raises any <see cref="Shapes.Shape.Changed"/> events. Setting 
    /// <see cref="Shape"/> to <see cref="Shapes.Shape.Empty"/> ensures that the internal event 
    /// handlers are unregistered and the object can be garbage-collected properly.
    /// </para>
    /// </remarks>
    Shape Shape { get; }


    /// <summary>
    /// Gets the scale.
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
    Vector3F Scale { get; }


    /// <summary>
    /// Occurs when the <see cref="Pose"/> was changed.
    /// </summary>
    event EventHandler<EventArgs> PoseChanged;


    /// <summary>
    /// Occurs when the <see cref="Shape"/> or <see cref="Scale"/> was changed.
    /// </summary>
    event EventHandler<ShapeChangedEventArgs> ShapeChanged;


    /// <summary>
    /// Creates a new <see cref="IGeometricObject"/> that is a clone (deep copy) of the current
    /// instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="IGeometricObject"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    IGeometricObject Clone();
  }
}
