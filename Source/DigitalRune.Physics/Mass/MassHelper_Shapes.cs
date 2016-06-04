// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics
{
  public static partial class MassHelper
  {
    // Notes:
    // For inertia formulas see Game Engine Gems 1, Chapter 14.
    //
    // Unless noted otherwise, the inertia is always computed around the center of mass - not around
    // the shape space origin or the world space origin!
    //
    // If a composite shape contains a rigid bodies, the mass properties of this bodies are used not 
    // the given density.


    /// <summary>
    /// Gets the mass properties for the given shape and related properties.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="densityOrMass">The density or mass value.</param>
    /// <param name="isDensity">
    /// If set to <see langword="true"/> <paramref name="densityOrMass"/> is treated as density;
    /// otherwise, the value is used for the target mass.
    /// </param>
    /// <param name="relativeDistanceThreshold">
    /// The relative distance threshold for shape approximations.
    /// </param>
    /// <param name="iterationLimit">
    /// The iteration limit. Can be 0 or -1 to use approximate mass properties.
    /// </param>
    /// <param name="mass">The mass.</param>
    /// <param name="centerOfMass">The center of mass.</param>
    /// <param name="inertia">The inertia.</param>
    /// <remarks>
    /// <para>
    /// This method computes <paramref name="mass"/>, <paramref name="centerOfMass"/> and the 
    /// <paramref name="inertia"/> matrix for the local space of the shape.
    /// </para>
    /// <para>
    /// If the <paramref name="shape"/> is a composite shape and a child geometric object is a
    /// rigid body, the mass properties of the rigid body are used for the child. Otherwise, new
    /// child mass properties are computed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="densityOrMass"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="relativeDistanceThreshold"/> is negative.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Keep code simple.")]
    internal static void GetMass(Shape shape, Vector3F scale, float densityOrMass, bool isDensity, float relativeDistanceThreshold, int iterationLimit,
                                 out float mass, out Vector3F centerOfMass, out Matrix33F inertia)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");
      if (densityOrMass <= 0)
        throw new ArgumentOutOfRangeException("densityOrMass", "The density or mass must be greater than 0.");
      if (relativeDistanceThreshold < 0)
        throw new ArgumentOutOfRangeException("relativeDistanceThreshold", "The relative distance threshold must not be negative.");

      mass = 0;
      centerOfMass = Vector3F.Zero;
      inertia = Matrix33F.Zero;

      // Note: We support all shape types of DigitalRune Geometry.
      // To support user-defined shapes we could add an interface IMassSource which can be 
      // implemented by shapes. In the else-case below we can check whether the shape implements 
      // the interface.
      if (shape is EmptyShape)
      {
        return;
      }
      else if (shape is InfiniteShape)
      {
        mass = float.PositiveInfinity;
        inertia = Matrix33F.CreateScale(float.PositiveInfinity);
      }
      else if (shape is BoxShape)
      {
        GetMass((BoxShape)shape, scale, densityOrMass, isDensity, out mass, out inertia);
      }
      else if (shape is CapsuleShape)
      {
        GetMass((CapsuleShape)shape, scale, densityOrMass, isDensity, out mass, out inertia);
      }
      else if (shape is ConeShape)
      {
        GetMass((ConeShape)shape, scale, densityOrMass, isDensity, out mass, out centerOfMass, out inertia);
      }
      else if (shape is CylinderShape)
      {
        GetMass((CylinderShape)shape, scale, densityOrMass, isDensity, out mass, out inertia);
      }
      else if (shape is ScaledConvexShape)
      {
        var scaledConvex = (ScaledConvexShape)shape;
        GetMass(scaledConvex.Shape, scale * scaledConvex.Scale, densityOrMass, isDensity, relativeDistanceThreshold, iterationLimit, out mass, out centerOfMass, out inertia);
      }
      else if (shape is SphereShape)
      {
        GetMass((SphereShape)shape, scale, densityOrMass, isDensity, out mass, out inertia);
      }
      else if (shape is TransformedShape)
      {
        var transformed = (TransformedShape)shape;

        // Call GetMass for the contained GeometricObject.
        GetMass(transformed.Child, scale, densityOrMass, isDensity, relativeDistanceThreshold, iterationLimit, out mass, out centerOfMass, out inertia);
      }
      else if (shape is HeightField)
      {
        // Height fields should always be static. Therefore, they we can treat them as having
        // infinite or zero mass.
        return;
      }
      else if (shape is CompositeShape)
      {
        var composite = (CompositeShape)shape;
        float density = (isDensity) ? densityOrMass : 1;
        foreach (var child in composite.Children)
        {
          // Call GetMass for the child geometric object.
          float childMass;
          Vector3F childCenterOfMass;
          Matrix33F childInertia;
          GetMass(child, scale, density, true, relativeDistanceThreshold, iterationLimit, out childMass, out childCenterOfMass, out childInertia);

          // Add child mass to total mass.
          mass = mass + childMass;

          // Add child inertia to total inertia and consider the translation.
          inertia += GetTranslatedMassInertia(childMass, childInertia, childCenterOfMass);

          // Add weighted centerOfMass.
          centerOfMass = centerOfMass + childCenterOfMass * childMass;
        }

        // centerOfMass must be divided by total mass because child center of mass were weighted
        // with the child masses.
        centerOfMass /= mass;

        // Make inertia relative to center of mass.
        inertia = GetUntranslatedMassInertia(mass, inertia, centerOfMass);

        if (!isDensity)
        {
          // Yet, we have not computed the correct total mass. We have to adjust the total mass to 
          // be equal to the given target mass.
          AdjustMass(densityOrMass, ref mass, ref inertia);
        }
      }
      else if (iterationLimit <= 0)
      {
        // We do not have a special formula for this kind of shape and iteration limit is 0 or less.
        // --> Use mass properties of AABB.
        var aabb = shape.GetAabb(scale, Pose.Identity);
        var extent = aabb.Extent;
        centerOfMass = aabb.Center;
        GetMass(extent, densityOrMass, isDensity, out mass, out inertia);
      }
      else
      {
        // We do not have a special formula for this kind of shape.
        // --> General polyhedron mass from triangle mesh.
        var mesh = shape.GetMesh(relativeDistanceThreshold, iterationLimit);
        mesh.Transform(Matrix44F.CreateScale(scale));
        GetMass(mesh, out mass, out centerOfMass, out inertia);

        // Mass was computed for density = 1. --> Scale mass.
        if (isDensity)
        {
          var volume = mesh.GetVolume();
          var targetMass = volume * densityOrMass;
          AdjustMass(targetMass, ref mass, ref inertia);
        }
        else
        {
          AdjustMass(densityOrMass, ref mass, ref inertia);
        }

        if (Numeric.IsLessOrEqual(mass, 0))
        {
          // If the mass is not valid, we fall back to the AABB mass.
          // This can happen for non-closed meshes that have a "negative" volume.
          GetMass(shape, scale, densityOrMass, isDensity, relativeDistanceThreshold, -1, out mass, out centerOfMass, out inertia);
          return;
        }
      }
    }


    private static void GetMass(SphereShape sphere, Vector3F scale, float densityOrMass, bool isDensity, out float mass, out Matrix33F inertia)
    {
      scale = Vector3F.Absolute(scale);
      Vector3F radius = sphere.Radius * scale;
      Vector3F radiusSquared = radius * radius;

      mass = (isDensity) ? 4.0f / 3.0f * ConstantsF.Pi * radius.X * radius.Y * radius.Z * densityOrMass : densityOrMass;

      inertia = Matrix33F.Zero;
      inertia.M00 = 1.0f / 5.0f * mass * (radiusSquared.Y + radiusSquared.Z);
      inertia.M11 = 1.0f / 5.0f * mass * (radiusSquared.X + radiusSquared.Z);
      inertia.M22 = 1.0f / 5.0f * mass * (radiusSquared.X + radiusSquared.Y);
    }


    private static void GetMass(CylinderShape cylinder, Vector3F scale, float densityOrMass, bool isDensity, out float mass, out Matrix33F inertia)
    {
      scale = Vector3F.Absolute(scale);
      float radiusX = cylinder.Radius * scale.X;
      float heightY = cylinder.Height * scale.Y;
      float radiusZ = cylinder.Radius * scale.Z;
      mass = (isDensity) ? ConstantsF.Pi * radiusX * radiusZ * heightY * densityOrMass : densityOrMass;

      inertia = Matrix33F.Zero;
      inertia.M00 = 1.0f / 4.0f * mass * radiusZ * radiusZ + 1.0f / 12.0f * mass * heightY * heightY;
      inertia.M11 = 1.0f / 4.0f * mass * radiusX * radiusX + 1.0f / 4.0f * mass * radiusZ * radiusZ;
      inertia.M22 = 1.0f / 4.0f * mass * radiusX * radiusX + 1.0f / 12.0f * mass * heightY * heightY;
    }


    private static void GetMass(ConeShape cone, Vector3F scale, float densityOrMass, bool isDensity, out float mass, out Vector3F centerOfMass, out Matrix33F inertia)
    {
      float radiusX = cone.Radius * Math.Abs(scale.X);
      float radiusZ = cone.Radius * Math.Abs(scale.Z);
      float height = cone.Height * Math.Abs(scale.Y);

      mass = (isDensity) ? 1.0f / 3.0f * ConstantsF.Pi * radiusX * radiusZ * height * densityOrMass : densityOrMass;

      centerOfMass = new Vector3F(0, height / 4, 0) * Math.Sign(scale.Y);

      inertia = Matrix33F.Zero;
      inertia.M00 = 3.0f / 20.0f * mass * (radiusZ * radiusZ + 1.0f / 4.0f * height * height);
      inertia.M11 = 3.0f / 20.0f * mass * (radiusX * radiusX + radiusZ * radiusZ);
      inertia.M22 = 3.0f / 20.0f * mass * (radiusX * radiusX + 1.0f / 4.0f * height * height);
    }


    private static void GetMass(CapsuleShape capsule, Vector3F scale, float densityOrMass, bool isDensity, out float mass, out Matrix33F inertia)
    {
      scale = Vector3F.Absolute(scale);
      Vector3F radius = capsule.Radius * scale;
      Vector3F radius2 = radius * radius;
      float height = capsule.Height * scale.Y - 2 * radius.Y; // Height of cylinder part.
      float height2 = height * height;
      mass = (isDensity) ? ConstantsF.Pi * radius.X * radius.Z * (4.0f / 3.0f * radius.Y + height) * densityOrMass : densityOrMass;

      inertia = Matrix33F.Zero;
      float denom = 4 * radius.Y + 3 * height;
      inertia.M00 = 1.0f / denom * mass * (2 * radius.Y * (2.0f / 5.0f * (radius2.Y + radius2.Z) + 3.0f / 4.0f * height * radius.Y + 1.0f / 2.0f * height2) + 3.0f * height * (1.0f / 4.0f * radius2.Z + 1.0f / 12.0f * height2));
      inertia.M11 = 1.0f / denom * mass * (2 * radius.Y * 2.0f / 5.0f * (radius2.X + radius2.Z) + 3.0f * height * 1.0f / 4.0f * (radius2.X + radius2.Z));
      inertia.M22 = 1.0f / denom * mass * (2 * radius.Y * (2.0f / 5.0f * (radius2.X + radius2.Y) + 3.0f / 4.0f * height * radius.Y + 1.0f / 2.0f * height2) + 3.0f * height * (1.0f / 4.0f * radius2.X + 1.0f / 12.0f * height2));
    }


    private static void GetMass(BoxShape box, Vector3F scale, float densityOrMass, bool isDensity, out float mass, out Matrix33F inertia)
    {
      scale = Vector3F.Absolute(scale);
      Vector3F extent = box.Extent * scale;
      GetMass(extent, densityOrMass, isDensity, out mass, out inertia);
    }


    /// <summary>
    /// Gets the mass properties of a box.
    /// </summary>
    private static void GetMass(Vector3F boxExtent, float densityOrMass, bool isDensity, out float mass, out Matrix33F inertia)
    {
      Vector3F extentSquared = boxExtent * boxExtent;
      if (isDensity)
        mass = boxExtent.X * boxExtent.Y * boxExtent.Z * densityOrMass;
      else
        mass = densityOrMass;

      inertia = Matrix33F.Zero;
      inertia.M00 = 1.0f / 12.0f * mass * (extentSquared.Y + extentSquared.Z);
      inertia.M11 = 1.0f / 12.0f * mass * (extentSquared.X + extentSquared.Z);
      inertia.M22 = 1.0f / 12.0f * mass * (extentSquared.X + extentSquared.Y);
    }


    /// <summary>
    /// Gets the mass properties of a geometric object.
    /// </summary>
    /// <remarks>
    /// If the geometric object is a <see cref="RigidBody"/>, the mass frame of the this rigid body
    /// is used. If the geometric object is not a <see cref="RigidBody"/> the mass is computed
    /// for the contained shape.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static void GetMass(IGeometricObject geometricObject, Vector3F scale, float densityOrMass, bool isDensity, float relativeDistanceThreshold, int iterationLimit,
                                out float mass, out Vector3F centerOfMass, out Matrix33F inertia)
    {
      // Computes mass in parent/world space of the geometric object!
      // centerOfMass is in world space and inertia is around the CM in world space!

      Pose pose = geometricObject.Pose;

      if ((scale.X != scale.Y || scale.Y != scale.Z) && pose.HasRotation)
        throw new NotSupportedException("NON-UNIFORM scaling of a TransformedShape or a CompositeShape with ROTATED children is not supported.");
      if (scale.X < 0 || scale.Y < 0 || scale.Z < 0)
        throw new NotSupportedException("Negative scaling is not supported..");

      var shape = geometricObject.Shape;
      var totalScale = scale * geometricObject.Scale;

      // Inertia around center of mass in local space.
      Matrix33F inertiaCMLocal;

      Vector3F centerOfMassLocal;

      var body = geometricObject as RigidBody;
      if (body != null)
      {
        // The geometric object is a rigid body and we use the properties of this body.

        if (!Vector3F.AreNumericallyEqual(scale, Vector3F.One))
          throw new NotSupportedException("Scaling is not supported when a child geometric object is a RigidBody.");
        
        var massFrame = body.MassFrame;
        mass = massFrame.Mass;
        centerOfMassLocal = massFrame.Pose.Position;
        inertiaCMLocal = massFrame.Pose.Orientation * Matrix33F.CreateScale(massFrame.Inertia) * massFrame.Pose.Orientation.Transposed;
      }
      else
      {
        // Compute new mass properties for the shape.
        GetMass(shape, totalScale, densityOrMass, isDensity, relativeDistanceThreshold, iterationLimit,
                out mass, out centerOfMassLocal, out inertiaCMLocal);
      }

      // Get inertia around center of mass in world space:
      // The world inertia is equal to: move to local space using the inverse orientation, then
      // apply local inertia then move to world space with the orientation matrix.
      inertia = pose.Orientation * inertiaCMLocal * pose.Orientation.Transposed;

      // Convert center of mass offset in world space. - Do not forget scale!
      centerOfMass = pose.ToWorldPosition(centerOfMassLocal) * scale;
    }
  }
}
