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
  /// Represents a box centered at the origin.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class BoxShape : ConvexShape
  {
    // TODO: Optimize: The support vertex distance could be simply computed as Dot(v.absolute().Normalized, halfExtentVector).

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
    /// <value>
    /// The extent of the box (<see cref="WidthX"/>, <see cref="WidthY"/>, <see cref="WidthZ"/>).
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A component of <paramref name="value"/> is negative.
    /// </exception>
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public Vector3F Extent
    {
      get { return new Vector3F(_widthX, _widthY, _widthZ); }
      set
      {
        if (value.X < 0 || value.Y < 0 || value.Z < 0)
          throw new ArgumentOutOfRangeException("value", "The extent of a box must be greater than or equal to 0.");

        if (_widthX != value.X || _widthY != value.Y || _widthZ != value.Z)
        {
          _widthX = value.X;
          _widthY = value.Y;
          _widthZ = value.Z;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }


    /// <summary>
    /// Gets an inner point (center of box).
    /// </summary>
    /// <value>The center of the box (0, 0, 0).</value>
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


    /// <summary>
    /// Gets or sets the width along the z-axis.
    /// </summary>
    /// <value>The width along the z-axis.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float WidthZ
    {
      get { return _widthZ; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The width must be greater than or equal to 0.");

        if (_widthZ != value)
        {
          _widthZ = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _widthZ;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="BoxShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="BoxShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates an empty box.
    /// </remarks>
    public BoxShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="BoxShape"/> class from the given extent vector.
    /// </summary>
    /// <param name="extent">The extent of the box.</param>
    public BoxShape(Vector3F extent)
      : this(extent.X, extent.Y, extent.Z)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="BoxShape"/> class with the given size.
    /// </summary>
    /// <param name="widthX">The width along the x-axis.</param>
    /// <param name="widthY">The width along the y-axis.</param>
    /// <param name="widthZ">The width along the z-axis.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthX"/>, <paramref name="widthY"/>, or <paramref name="widthZ"/> is 
    /// negative.
    /// </exception>
    public BoxShape(float widthX, float widthY, float widthZ)
    {
      if (widthX < 0)
        throw new ArgumentOutOfRangeException("widthX", "The width must be greater than or equal to 0.");
      if (widthY < 0)
        throw new ArgumentOutOfRangeException("widthY", "The width must be greater than or equal to 0.");
      if (widthZ < 0)
        throw new ArgumentOutOfRangeException("widthZ", "The width must be greater than or equal to 0.");

      _widthX = widthX;
      _widthY = widthY;
      _widthZ = widthZ;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new BoxShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (BoxShape)sourceShape;
      _widthX = source.WidthX;
      _widthY = source.WidthY;
      _widthZ = source.WidthZ;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      Vector3F halfExtent = new Vector3F(_widthX / 2, _widthY / 2, _widthZ / 2) * Vector3F.Absolute(scale);

      if (pose == Pose.Identity)
        return new Aabb(-halfExtent, halfExtent);

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
        Z = ((direction.Z >= 0) ? _widthZ / 2 : -_widthZ / 2)
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
        Z = ((directionNormalized.Z >= 0) ? _widthZ / 2 : -_widthZ / 2)
      };
      return supportVertex;
    }


    ///// <summary>
    ///// Gets the box vertex with the given index.
    ///// </summary>
    ///// <param name="index">The index.</param>
    ///// <returns>A box vertex in the local space of the box.</returns>
    //internal Vector3F GetVertex(int index)
    //{
    //  switch (index)
    //  {
    //    case 0:  return new Vector3F(-WidthX / 2, -WidthY / 2, -WidthZ / 2);
    //    case 1:  return new Vector3F(-WidthX / 2, -WidthY / 2,  WidthZ / 2);
    //    case 2:  return new Vector3F(-WidthX / 2,  WidthY / 2, -WidthZ / 2);
    //    case 3:  return new Vector3F(-WidthX / 2,  WidthY / 2,  WidthZ / 2);
    //    case 4:  return new Vector3F( WidthX / 2, -WidthY / 2, -WidthZ / 2);
    //    case 5:  return new Vector3F( WidthX / 2, -WidthY / 2,  WidthZ / 2);
    //    case 6:  return new Vector3F( WidthX / 2,  WidthY / 2, -WidthZ / 2);
    //    default: return new Vector3F( WidthX / 2,  WidthY / 2,  WidthZ / 2);
    //  }
    //}


    internal LineSegment GetEdge(int axis, Vector3F supportDirection, Vector3F scale)
    {
      scale = Vector3F.Absolute(scale);

      var signX = supportDirection.X < 0 ? -1 : 1;
      var signY = supportDirection.Y < 0 ? -1 : 1;
      var signZ = supportDirection.Z < 0 ? -1 : 1;

      Vector3F start = new Vector3F(signX * WidthX / 2 * scale.X, signY * WidthY / 2 * scale.Y, signZ * WidthZ / 2 * scale.Z);
      Vector3F end = start;

      switch (axis)
      {
        case 0:  end.X = -end.X; break;
        case 1:  end.Y = -end.Y; break;
        default: end.Z = -end.Z; break;
      }

      return new LineSegment(start, end);
    }


    /// <overloads>
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the volume of this box.
    /// </summary>
    /// <returns>The volume of this box.</returns>
    public float GetVolume()
    {
      return _widthX * _widthY * _widthZ;
    }


    /// <summary>
    /// Gets the volume of this box.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The volume of this box.</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return GetVolume();
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Half extent:
      float halfExtentX = _widthX / 2;
      float halfExtentY = _widthY / 2;
      float halfExtentZ = _widthZ / 2;

      TriangleMesh mesh = new TriangleMesh();

      // -y face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-halfExtentX, -halfExtentY, halfExtentZ),
        Vertex1 = new Vector3F(-halfExtentX, -halfExtentY, -halfExtentZ),
        Vertex2 = new Vector3F(halfExtentX, -halfExtentY, -halfExtentZ),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(halfExtentX, -halfExtentY, -halfExtentZ),
        Vertex1 = new Vector3F(halfExtentX, -halfExtentY, halfExtentZ),
        Vertex2 = new Vector3F(-halfExtentX, -halfExtentY, halfExtentZ),
      }, true);

      // +x face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(halfExtentX, halfExtentY, halfExtentZ),
        Vertex1 = new Vector3F(halfExtentX, -halfExtentY, halfExtentZ),
        Vertex2 = new Vector3F(halfExtentX, -halfExtentY, -halfExtentZ),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(halfExtentX, -halfExtentY, -halfExtentZ),
        Vertex1 = new Vector3F(halfExtentX, halfExtentY, -halfExtentZ),
        Vertex2 = new Vector3F(halfExtentX, halfExtentY, halfExtentZ),
      }, true);

      // -z face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(halfExtentX, halfExtentY, -halfExtentZ),
        Vertex1 = new Vector3F(halfExtentX, -halfExtentY, -halfExtentZ),
        Vertex2 = new Vector3F(-halfExtentX, -halfExtentY, -halfExtentZ),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-halfExtentX, -halfExtentY, -halfExtentZ),
        Vertex1 = new Vector3F(-halfExtentX, halfExtentY, -halfExtentZ),
        Vertex2 = new Vector3F(halfExtentX, halfExtentY, -halfExtentZ),
      }, true);

      // -x face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-halfExtentX, halfExtentY, -halfExtentZ),
        Vertex1 = new Vector3F(-halfExtentX, -halfExtentY, -halfExtentZ),
        Vertex2 = new Vector3F(-halfExtentX, -halfExtentY, halfExtentZ),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-halfExtentX, -halfExtentY, halfExtentZ),
        Vertex1 = new Vector3F(-halfExtentX, halfExtentY, halfExtentZ),
        Vertex2 = new Vector3F(-halfExtentX, halfExtentY, -halfExtentZ),
      }, true);

      // +z face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-halfExtentX, halfExtentY, halfExtentZ),
        Vertex1 = new Vector3F(-halfExtentX, -halfExtentY, halfExtentZ),
        Vertex2 = new Vector3F(halfExtentX, -halfExtentY, halfExtentZ),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(halfExtentX, -halfExtentY, halfExtentZ),
        Vertex1 = new Vector3F(halfExtentX, halfExtentY, halfExtentZ),
        Vertex2 = new Vector3F(-halfExtentX, halfExtentY, halfExtentZ),
      }, true);

      // +y face
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(-halfExtentX, halfExtentY, -halfExtentZ),
        Vertex1 = new Vector3F(-halfExtentX, halfExtentY, halfExtentZ),
        Vertex2 = new Vector3F(halfExtentX, halfExtentY, halfExtentZ),
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = new Vector3F(halfExtentX, halfExtentY, halfExtentZ),
        Vertex1 = new Vector3F(halfExtentX, halfExtentY, -halfExtentZ),
        Vertex2 = new Vector3F(-halfExtentX, halfExtentY, -halfExtentZ),
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
      return String.Format(CultureInfo.InvariantCulture, "BoxShape {{ WidthX = {0}, WidthY = {1}, WidthZ = {2} }}", _widthX, _widthY, _widthZ);
    }
    #endregion
  }
}
