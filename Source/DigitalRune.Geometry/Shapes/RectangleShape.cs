// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Xml.Serialization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a rectangle in the xy-plane centered at the origin.
  /// </summary>
  /// <remarks>
  /// The front face is visible from the positive z half-space.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class RectangleShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the extent vector.
    /// </summary>
    /// <value>The extent of the rectangle (<see cref="WidthX"/>, <see cref="WidthY"/>).</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A component of <paramref name="value"/> is negative.
    /// </exception>
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public Vector2F Extent
    {
      get { return new Vector2F(_widthX, _widthY); }
      set
      {
        if (value.X < 0 || value.Y < 0)
          throw new ArgumentOutOfRangeException("value", "The extent of a rectangle must be greater than or equal to 0.");

        if (_widthX != value.X || _widthY != value.Y)
        {
          _widthX = value.X;
          _widthY = value.Y;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }


    /// <summary>
    /// Gets an inner point (center of rectangle).
    /// </summary>
    /// <value>The center of the rectangle (0, 0, 0).</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return Vector3F.Zero; }
    }


    /// <summary>
    /// Gets or sets the width along the x-axis.
    /// </summary>
    /// <value>The width along the x-axis.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float WidthX
    {
      get { return _widthX; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The width must be greater than or equal to 0.");

        if (_widthX != value)
        {
          _widthX = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _widthX;


    /// <summary>
    /// Gets or sets the width along the y-axis.
    /// </summary>
    /// <value>The width along the y-axis.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float WidthY
    {
      get { return _widthY; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The width must be greater than or equal to 0.");

        if (_widthY != value)
        {
          _widthY = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _widthY;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates an empty rectangle with a size of 0.
    /// </remarks>
    public RectangleShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleShape"/> class with the given extent.
    /// </summary>
    /// <param name="extent">The extent vector.</param>
    public RectangleShape(Vector2F extent) : this(extent.X, extent.Y)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleShape"/> class with the given width
    /// and height.
    /// </summary>
    /// <param name="widthX">The width along the x-axis.</param>
    /// <param name="widthY">The width along the y-axis.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthX"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthY"/> is negative.
    /// </exception>
    public RectangleShape(float widthX, float widthY)
    {
      if (widthX < 0)
        throw new ArgumentOutOfRangeException("widthX", "The width must be greater than or equal to 0.");
      if (widthY < 0)
        throw new ArgumentOutOfRangeException("widthY", "The width must be greater than or equal to 0.");

      _widthX = widthX;
      _widthY = widthY;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new RectangleShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (RectangleShape)sourceShape;
      _widthX = source.WidthX;
      _widthY = source.WidthY;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      Vector3F halfExtent = new Vector3F(_widthX / 2, _widthY / 2, 0) * Vector3F.Absolute(scale);

      // Get world axes in local space. They are equal to the rows of the orientation matrix.
      Matrix33F rotationMatrix = pose.Orientation;
      Vector3F worldX = rotationMatrix.GetRow(0);
      Vector3F worldY = rotationMatrix.GetRow(1);
      Vector3F worldZ = rotationMatrix.GetRow(2);

      // The half extent vector is in the +x/+y/+z octant of the world. We want to project
      // the extent onto the world axes. The half extent projected onto world x gives us the 
      // x extent. 
      // The world axes in local space could be in another world octant. We could now either find 
      // out the in which octant the world axes is pointing and build the correct half extent vector
      // for this octant. OR we mirror the world axis vectors into the +x/+y/+z octant by taking
      // the absolute vector.
      worldX = Vector3F.Absolute(worldX);
      worldY = Vector3F.Absolute(worldY);
      worldZ = Vector3F.Absolute(worldZ);

      // Now we project the extent onto the world axes.
      Vector3F halfExtentWorld = new Vector3F(Vector3F.Dot(halfExtent, worldX),
                                              Vector3F.Dot(halfExtent, worldY),
                                              Vector3F.Dot(halfExtent, worldZ));

      return new Aabb(pose.Position - halfExtentWorld, pose.Position + halfExtentWorld);
    }


    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point. The vector does not need to be normalized.
    /// The result is undefined if the vector is a zero vector.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public override Vector3F GetSupportPoint(Vector3F direction)
    {
      Vector3F supportVertex = new Vector3F
      {
        X = ((direction.X >= 0) ? _widthX / 2 : -_widthX / 2),
        Y = ((direction.Y >= 0) ? _widthY / 2 : -_widthY / 2),
        Z = 0
      };
      return supportVertex;
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point.
    /// </param>
    /// <returns>
    /// A support point regarding the given direction.
    /// </returns>
    /// <remarks>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </remarks>
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      Vector3F supportVertex = new Vector3F
      {
        X = ((directionNormalized.X >= 0) ? _widthX / 2 : -_widthX / 2),
        Y = ((directionNormalized.Y >= 0) ? _widthY / 2 : -_widthY / 2),
        Z = 0
      };
      return supportVertex;
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>0</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return 0;
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(_widthX / 2, _widthY / 2, 0),
        Vertex1 = new Vector3F(-_widthX / 2, _widthY / 2, 0),
        Vertex2 = new Vector3F(-_widthX / 2, -_widthY / 2, 0),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-_widthX / 2, -_widthY / 2, 0),
        Vertex1 = new Vector3F(_widthX / 2, -_widthY / 2, 0),
        Vertex2 = new Vector3F(_widthX / 2, _widthY / 2, 0),
      }, true);
      return mesh;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "RectangleShape {{ WidthX = {0}, WidthY = {1} }}", _widthX, _widthY);
    }
    #endregion
  }
}
