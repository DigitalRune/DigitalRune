// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Plane = DigitalRune.Geometry.Shapes.Plane;

#if XNA || MONOGAME
using Microsoft.Xna.Framework;
#endif


namespace DigitalRune.Geometry
{
  /// <summary>
  /// Provides helper methods for various geometry tasks.
  /// </summary>
  /// <remarks>
  /// This class is a collection of several helper methods to compute bounding volumes (see 
  /// <see cref="CreateBoundingShape"/>, 
  /// <see cref="ComputeBoundingBox(IList{Vector3F}, out Vector3F, out Pose)"/> and
  /// <see cref="ComputeBoundingSphere"/>) and convex hulls (see 
  /// <see cref="CreateConvexHull(IEnumerable{Vector3F})"/>). The other methods will not be needed
  /// in most situations. For general collision detection tasks use the types in the namespace 
  /// <see cref="DigitalRune.Geometry.Collisions"/>.
  /// </remarks>
  public static partial class GeometryHelper
  {
    /// <summary>
    /// Computes a minimum bounding shape that contains all given points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>A minimum bounding shape that contains all given points.</returns>
    /// <remarks>
    /// The returned shape will be a <see cref="SphereShape"/>, a <see cref="CapsuleShape"/>,
    /// a <see cref="BoxShape"/>, or a <see cref="TransformedShape"/> (containing a sphere, capsule,
    /// or a box). The bounding shape is not guaranteed to be optimal, it is only guaranteed that
    /// the bounding shape includes all given points.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    public static Shape CreateBoundingShape(IList<Vector3F> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count == 0)
        throw new ArgumentException("The list of 'points' is empty.");

      // Compute minimal sphere.
      Vector3F center;
      float radius;
      ComputeBoundingSphere(points, out radius, out center);
      SphereShape sphere = new SphereShape(radius);
      float sphereVolume = sphere.GetVolume();

      // Compute minimal capsule.
      float height;
      Pose capsulePose;
      ComputeBoundingCapsule(points, out radius, out height, out capsulePose);
      CapsuleShape capsule = new CapsuleShape(radius, height);
      float capsuleVolume = capsule.GetVolume();

      // Compute minimal box.
      Vector3F boxExtent;
      Pose boxPose;
      ComputeBoundingBox(points, out boxExtent, out boxPose);
      var box = new BoxShape(boxExtent);
      float boxVolume = box.GetVolume();

      // Return the object with the smallest volume.
      // A TransformedShape is used if the shape needs to be translated or rotated.
      if (sphereVolume < boxVolume && sphereVolume < capsuleVolume)
      {
        if (center.IsNumericallyZero)
          return sphere;

        return new TransformedShape(new GeometricObject(sphere, new Pose(center)));
      }
      else if (capsuleVolume < boxVolume)
      {
        if (!capsulePose.HasTranslation && !capsulePose.HasRotation)
          return capsule;

        return new TransformedShape(new GeometricObject(capsule, capsulePose));
      }
      else
      {
        if (!boxPose.HasTranslation && !boxPose.HasRotation)
          return box;

        return new TransformedShape(new GeometricObject(box, boxPose));
      }
    }


    /// <summary>
    /// Creates a convex hull mesh for a set of points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>
    /// The mesh of the convex hull or <see langword="null"/> if the point list
    /// is <see langword="null"/> or empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned mesh describes the convex hull. All faces are convex polygons.
    /// </para>
    /// <para>
    /// This method calls <see cref="CreateConvexHull(IEnumerable{Vector3F},int,float)"/> with
    /// no vertex limit and 0 skin width.
    /// </para>
    /// </remarks>
    public static DcelMesh CreateConvexHull(IEnumerable<Vector3F> points)
    {
      return CreateConvexHull(points, int.MaxValue, 0);
    }


    /// <summary>
    /// Creates a convex hull mesh for a set of points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="vertexLimit">
    /// The vertex limit. Must be greater than 0. Common values are 32 or 64.
    /// </param>
    /// <param name="skinWidth">
    /// The skin width. Common values are 0.01 or 0.001.
    /// </param>
    /// <returns>
    /// The mesh of the convex hull or <see langword="null"/> if the point list is 
    /// <see langword="null"/> or empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned mesh describes the convex hull. All faces are convex polygons.
    /// </para>
    /// <para>
    /// If the created convex hull has more vertices than <paramref name="vertexLimit"/>, the hull
    /// will be simplified. The simplified hull is conservative, which means it contains all given
    /// <paramref name="points"/> and is less "tight" than the exact hull. It is possible that the 
    /// simplified hull contains slightly more vertices than <paramref name="vertexLimit"/> (e.g. it 
    /// is possible that for a vertex limit of 32 a hull with 34 vertices is returned).
    /// </para>
    /// <para>
    /// All planes of the convex hull are extruded by the <paramref name="skinWidth"/>. This can be 
    /// used to increase or decrease the size of the convex hull. 
    /// </para>
    /// </remarks>
    public static DcelMesh CreateConvexHull(IEnumerable<Vector3F> points, int vertexLimit, float skinWidth)
    {
      // Nothing to do for empty input.
      if (points == null)
        return null;

      ConvexHullBuilder builder = new ConvexHullBuilder();
      builder.Grow(points, vertexLimit, skinWidth);

      if (builder.Type == ConvexHullType.Empty)
        return null;

      return builder.Mesh;
    }


    //--------------------------------------------------------------
    #region Cartesian Coordinates <-> Spherical Coordinates
    //--------------------------------------------------------------

    /// <summary>
    /// Converts the given Cartesian coordinates spherical coordinates.
    /// </summary>
    /// <param name="v">The Cartesian coordinates.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="inclination">
    /// The inclination angle [0, π] from the z-direction. (Also known as polar angle.)
    /// </param>
    /// <param name="azimuth">
    /// The azimuth angle [-π, π] measured from the Cartesian x-axis.
    /// </param>
    internal static void ToSphericalCoordinates(Vector3F v, out float radius, out float inclination, out float azimuth)
    {
      radius = v.Length;
      if (radius == 0.0f)
      {
        azimuth = 0.0f;
        inclination = 0.0f;
      }
      else
      {
        azimuth = (float)Math.Atan2(v.Y, v.X);
        inclination = (float)Math.Acos(v.Z / radius);
      }
    }


    /// <summary>
    /// Converts the given spherical coordinates to Cartesian coordinates.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="inclination">
    /// The inclination angle [0, π] from the z-direction. (Also known as polar angle.)
    /// </param>
    /// <param name="azimuth">
    /// The azimuth angle [-π, π] measured from the Cartesian x-axis.
    /// </param>
    /// <returns>The Cartesian coordinates.</returns>
    internal static Vector3F ToCartesianCoordinates(float radius, float inclination, float azimuth)
    {
      if (radius == 0.0)
        return new Vector3F();

      float sinφ = (float)Math.Sin(azimuth);
      float cosφ = (float)Math.Cos(azimuth);
      float sinθ = (float)Math.Sin(inclination);
      float cosθ = (float)Math.Cos(inclination);
      return new Vector3F(
        radius * sinθ * cosφ,
        radius * sinθ * sinφ,
        radius * cosθ);
    }
    #endregion


    //--------------------------------------------------------------
    #region k-DOP
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Extracts the viewing frustum planes of a world-view-projection matrix.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Extracts the viewing frustum planes of a world-view-projection matrix.
    /// </summary>
    /// <param name="projection">The world-view-projection matrix (DirectX standard).</param>
    /// <param name="planes">
    /// IN: An empty list of planes.
    /// OUT: The planes that define the frustum in the order: near, far, left, right, bottom, top. 
    /// The plane normals are pointing outwards.
    /// </param>
    /// <param name="normalize">
    /// <see langword="true"/> if the planes should be normalized; otherwise 
    /// <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="planes"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void ExtractPlanes(Matrix44F projection, IList<Plane> planes, bool normalize)
    {
      // See "Fast Extraction of Viewing Frustum Planes from the World-View-Projection Matrix",
      // http://crazyjoke.free.fr/doc/3D/plane%20extraction.pdf.

      // Notes: The planes in the paper are given as (a, b, c, d), where (a, b, c)
      // is the normal and d is -DistanceToOrigin. The normals in the paper point 
      // inside, but in our implementation the normals need to point outside!

      Vector3F normal;
      float distance;

      // Near plane
      normal.X = -projection.M20;
      normal.Y = -projection.M21;
      normal.Z = -projection.M22;
      distance =  projection.M23;
      planes.Add(new Plane(normal, distance));

      // Far plane
      normal.X = -projection.M30 + projection.M20;
      normal.Y = -projection.M31 + projection.M21;
      normal.Z = -projection.M32 + projection.M22;
      distance =  projection.M33 - projection.M23;
      planes.Add(new Plane(normal, distance));

      // Left plane
      normal.X = -projection.M30 - projection.M00;
      normal.Y = -projection.M31 - projection.M01;
      normal.Z = -projection.M32 - projection.M02;
      distance =  projection.M33 + projection.M03;
      planes.Add(new Plane(normal, distance));

      // Right plane
      normal.X = -projection.M30 + projection.M00;
      normal.Y = -projection.M31 + projection.M01;
      normal.Z = -projection.M32 + projection.M02;
      distance =  projection.M33 - projection.M03;
      planes.Add(new Plane(normal, distance));

      // Bottom plane
      normal.X = -projection.M30 - projection.M10;
      normal.Y = -projection.M31 - projection.M11;
      normal.Z = -projection.M32 - projection.M12;
      distance =  projection.M33 + projection.M13;
      planes.Add(new Plane(normal, distance));

      // Top plane
      normal.X = -projection.M30 + projection.M10;
      normal.Y = -projection.M31 + projection.M11;
      normal.Z = -projection.M32 + projection.M12;
      distance =  projection.M33 - projection.M13;
      planes.Add(new Plane(normal, distance));

      if (normalize)
      {
        for (int i = 0; i < planes.Count; i++)
        {
          try
          {
            var plane = planes[i];
            plane.Normalize();
            planes[i] = plane;
          }
          catch (DivideByZeroException)
          {
            if (i != 1)
              throw;

            throw new DivideByZeroException("Cannot normalize far plane of view frustum. " +
              "The near-far range of the projection is too large. " +
              "Try to increase the near distance or decrease the far distance.");
          }
        }
      }
    }


#if XNA || MONOGAME
    /// <summary>
    /// Extracts the viewing frustum planes of a world-view-projection matrix. (Only available in
    /// the XNA-compatible build.)
    /// </summary>
    /// <param name="projection">The projection matrix (DirectX standard).</param>
    /// <param name="planes">
    /// IN: An empty list of planes. OUT: The planes that define the shape. The plane normals are
    /// pointing outwards.
    /// </param>
    /// <param name="normalize">
    /// <see langword="true"/> if the planes should be normalized; otherwise 
    /// <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="planes"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the DigitalRune.Geometry.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void ExtractPlanes(Matrix projection, IList<Plane> planes, bool normalize)
    {
      // See "Fast Extraction of Viewing Frustum Planes from the World-View-Projection Matrix",
      // http://crazyjoke.free.fr/doc/3D/plane%20extraction.pdf.

      // Notes: The planes in the paper are given as (a, b, c, d), where (a, b, c)
      // is the normal and d = -DistanceToOrigin. The normal points inside.
      // Our implementation: The normals point outside!

      Vector3F normal;
      float distance;

      // Near plane
      normal.X = -projection.M13;
      normal.Y = -projection.M23;
      normal.Z = -projection.M33;
      distance = projection.M43;
      planes.Add(new Plane(normal, distance));

      // Far plane
      normal.X = -projection.M14 + projection.M13;
      normal.Y = -projection.M24 + projection.M23;
      normal.Z = -projection.M34 + projection.M33;
      distance = projection.M44 - projection.M43;
      planes.Add(new Plane(normal, distance));

      // Left plane
      normal.X = -projection.M14 - projection.M11;
      normal.Y = -projection.M24 - projection.M21;
      normal.Z = -projection.M34 - projection.M31;
      distance = projection.M44 + projection.M41;
      planes.Add(new Plane(normal, distance));

      // Right plane
      normal.X = -projection.M14 + projection.M11;
      normal.Y = -projection.M24 + projection.M21;
      normal.Z = -projection.M34 + projection.M31;
      distance = projection.M44 - projection.M41;
      planes.Add(new Plane(normal, distance));

      // Bottom plane
      normal.X = -projection.M14 - projection.M12;
      normal.Y = -projection.M24 - projection.M22;
      normal.Z = -projection.M34 - projection.M32;
      distance = projection.M44 + projection.M42;
      planes.Add(new Plane(normal, distance));

      // Top plane
      normal.X = -projection.M14 + projection.M12;
      normal.Y = -projection.M24 + projection.M22;
      normal.Z = -projection.M34 + projection.M32;
      distance = projection.M44 - projection.M42;
      planes.Add(new Plane(normal, distance));

      if (normalize)
      {
        for (int i = 0; i < planes.Count; i++)
        {
          var plane = planes[i];
          plane.Normalize();
          planes[i] = plane;
        }
      }
    }
#endif
    #endregion
  }
}
