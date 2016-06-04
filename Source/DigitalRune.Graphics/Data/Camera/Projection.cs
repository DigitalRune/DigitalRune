// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a projection transformation (including its bounding shape).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The property <see cref="ViewVolume"/> defines the bounding shape of the projection which can
  /// be used for frustum culling. The shape is updated automatically when the properties of the
  /// projection change.
  /// </para>
  /// <para>
  /// <strong>Notes to Inheritors: </strong><br/>
  /// Derived classes must initialize <see cref="ViewVolume"/> and provide the implementation of the
  /// <see cref="Set"/> method. The base class caches a <see cref="Matrix44F"/> which describes the
  /// projection. Therefore, derived classes must call <see cref="Invalidate"/> if the projection is
  /// changed. The <see cref="Projection"/> base class will call <see cref="ComputeProjection"/> of
  /// derived classes to get and cache the new projection matrix when needed.
  /// </para>
  /// </remarks>
  public abstract class Projection
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Matrix44F _projection = Matrix44F.Identity;
    private bool _projectionNeedsToBeUpdated;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the minimum x-value of the view volume at the near view-plane.
    /// </summary>
    /// <value>The minimum x-value of the view volume at the near view-plane.</value>
    public float Left
    {
      get { return ViewVolume.Left; }
      set
      {
        if (ViewVolume.Left != value)
        {
          ViewVolume.Left = value;
          Invalidate();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximum x-value of the view volume at the near view-plane.
    /// </summary>
    /// <value>The maximum x-value of the view volume at the near view-plane.</value>
    public float Right
    {
      get { return ViewVolume.Right; }
      set
      {
        if (ViewVolume.Right != value)
        {
          ViewVolume.Right = value;
          Invalidate();
        }
      }
    }


    /// <summary>
    /// Gets or sets the minimum y-value of the view volume at the near view-plane.
    /// </summary>
    /// <value>The minimum y-value of the view volume at the near view-plane.</value>
    public float Bottom
    {
      get { return ViewVolume.Bottom; }
      set
      {
        if (ViewVolume.Bottom != value)
        {
          ViewVolume.Bottom = value;
          Invalidate();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximum y-value of the view volume at the near view-plane.
    /// </summary>
    /// <value>The maximum y-value of the view volume at the near view-plane.</value>
    public float Top
    {
      get { return ViewVolume.Top; }
      set
      {
        if (ViewVolume.Top != value)
        {
          ViewVolume.Top = value;
          Invalidate();
        }
      }
    }


    /// <summary>
    /// Gets or sets the distance to the near view plane. 
    /// </summary>
    /// <value>The distance to the near view plane.</value>
    public float Near
    {
      get { return ViewVolume.Near; }
      set
      {
        if (ViewVolume.Near != value)
        {
          ViewVolume.Near = value;
          Invalidate();
        }
      }
    }


    /// <summary>
    /// Gets or sets the distance to the far view plane. 
    /// </summary>
    /// <value>The distance to the far view plane.</value>
    public float Far
    {
      get { return ViewVolume.Far; }
      set
      {
        if (ViewVolume.Far != value)
        {
          ViewVolume.Far = value;
          Invalidate();
        }
      }
    }


    /// <summary>
    /// Gets the width of the view volume at the near view plane.
    /// </summary>
    /// <value>The width of the view volume.</value>
    public float Width
    {
      get { return ViewVolume.Width; }
    }


    /// <summary>
    /// Gets the height of the view volume at the near view plane.
    /// </summary>
    /// <value>The height of the view volume.</value>
    public float Height
    {
      get { return ViewVolume.Height; }
    }


    /// <summary>
    /// Gets the depth of the view volume (<see cref="Far"/> - <see cref="Near"/>).
    /// </summary>
    /// <value>The depth of the view volume (<see cref="Far"/> - <see cref="Near"/>).</value>
    public float Depth
    {
      get { return ViewVolume.Depth; }
    }


    /// <summary>
    /// Gets the aspect ratio (width / height) of the view.
    /// </summary>
    /// <value>The aspect ratio (<see cref="Width"/> / <see cref="Height"/>).</value>
    public float AspectRatio
    {
      get { return ViewVolume.AspectRatio; }
    }


    /// <summary>
    /// Gets the horizontal field of view in radians.
    /// </summary>
    /// <value>
    /// The horizontal field of view in radians. <see cref="float.NaN"/> if this is a orthographic 
    /// view volume.
    /// </value>
    public float FieldOfViewX
    {
      get { return ViewVolume.FieldOfViewX; }
    }


    /// <summary>
    /// Gets the vertical field of view in radians.
    /// </summary>
    /// <value>
    /// The vertical field of view in radians. <see cref="float.NaN"/> if this is a orthographic
    /// view volume.
    /// </value>
    public float FieldOfViewY
    {
      get { return ViewVolume.FieldOfViewY; }
    }


    /// <summary>
    /// Gets the inverse of the projection matrix.
    /// </summary>
    /// <value>The inverse projection matrix.</value>
    /// <remarks>
    /// Setting <see cref="Inverse"/> automatically updates the <see cref="Projection"/>.
    /// </remarks>
    public Matrix44F Inverse
    {
      get
      {
        if (_projectionNeedsToBeUpdated)
          Update();

        return _projectionInverse;
      }
      set
      {
        if (_projectionInverse != value)
        {
          Matrix44F projectionInverse = value;
          Matrix44F projection = value.Inverse;
          Set(projection);
          _projection = projection;
          _projectionInverse = projectionInverse;
          _projectionNeedsToBeUpdated = false;
        }
      }
    }
    private Matrix44F _projectionInverse = Matrix44F.Identity;


    /// <summary>
    /// Gets (or sets) the shape of the view volume (viewing frustum).
    /// </summary>
    /// <value>A <see cref="ViewVolume"/> that describes the viewing frustum.</value>
    public ViewVolume ViewVolume { get; protected set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Projection"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Projection"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Projection"/> derived class and <see cref="CloneCore"/> to create a copy of the
    /// current instance. Classes that derive from <see cref="Projection"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public Projection Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Projection"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method,
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Projection"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Projection CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Projection. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="Projection"/>
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// Do not call this method directly (except when calling base in an implementation). This
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Projection"/> derived class must implement this method. A typical implementation
    /// is to simply call the default constructor and return the result. 
    /// </remarks>
    protected abstract Projection CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified 
    /// <see cref="Projection"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Projection"/> derived class must
    /// implement this method. 
    /// </remarks>
    protected abstract void CloneCore(Projection source);
    #endregion


    /// <summary>
    /// Invalidates the projection matrix.
    /// </summary>
    /// <remarks>
    /// <see cref="Invalidate"/> causes a lazy update: An internal flag is set that indicates that
    /// the matrices need to be recalculated. The calculation is done when <see cref="ToMatrix44F"/>,
    /// or an implicit cast to <see cref="Matrix44F"/> is performed or when <see cref="Inverse"/> is
    /// accessed.
    /// </remarks>
    protected void Invalidate()
    {
      _projectionNeedsToBeUpdated = true;
    }


    /// <summary>
    /// Updates the projection matrix.
    /// </summary>
    private void Update()
    {
      // Let the derived class update the projection.
      _projection = ComputeProjection();
      _projectionInverse = _projection.Inverse;
      _projectionNeedsToBeUpdated = false;
    }


    /// <summary>
    /// Computes the projection matrix.
    /// </summary>
    /// <returns>The projection matrix.</returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The base class <see cref="Projection"/> does not know
    /// how to compute a projection matrix. The classes deriving from <see cref="Projection"/> need
    /// to implement <see cref="ComputeProjection"/> and return a valid projection matrix.
    /// </remarks>
    protected abstract Matrix44F ComputeProjection();


    /// <summary>
    /// Sets the projection from the given projection matrix.
    /// </summary>
    /// <param name="projection">The projection matrix.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    public abstract void Set(Matrix44F projection);


    /// <summary>
    /// Converts a projection to a 4x4 transformation matrix.
    /// </summary>
    /// <returns>A 4x4-matrix that represents the same transformation as the projection.</returns>
    public Matrix44F ToMatrix44F()
    {
      if (_projectionNeedsToBeUpdated)
        Update();

      return _projection;
    }


    /// <summary>
    /// Converts a projection to a 4x4 transformation matrix (XNA Framework). (Only available in the 
    /// XNA-compatible build.)
    /// </summary>
    /// <returns>A 4x4-matrix that represents the same transformation as the projection.</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Graphics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Matrix ToXna()
    {
      if (_projectionNeedsToBeUpdated)
        Update();

      return (Matrix)_projection;
    }


    /// <summary>
    /// Converts the projection to a 4x4 transformation matrix.
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <returns>A 4x4-matrix that represents the same transformation as the projection.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public static implicit operator Matrix44F(Projection projection)
    {
      // return projection.ToMatrix44F();

      // Inlined
      if (projection._projectionNeedsToBeUpdated)
        projection.Update();

      return projection._projection;
    }


    /// <summary>
    /// Converts the projection to a 4x4 transformation matrix (XNA Framework). (Only available in 
    /// the XNA-compatible build.)
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <returns>A 4x4-matrix that represents the same transformation as the projection.</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Graphics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public static implicit operator Matrix(Projection projection)
    {
      // return projection.ToXna();

      // Inlined
      if (projection._projectionNeedsToBeUpdated)
        projection.Update();

      return (Matrix)projection._projection;
    }
    #endregion
  }
}
