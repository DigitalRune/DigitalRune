// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Stores the data that is needed to compute buoyancy for a single body.
  /// </summary>
  /// <remarks>
  /// All values are stored unscaled! The <see cref="IGeometricObject.Scale"/> must be applied in 
  /// the <see cref="Buoyancy"/> effect.
  /// </remarks>
  internal sealed class BuoyancyData
  {
    /// <summary>
    /// Gets or sets the triangle mesh that represents the shape of the rigid body.
    /// </summary>
    /// <value>The triangle mesh of the body.</value>
    internal TriangleMesh Mesh { get; set; }


    /// <summary>
    /// Gets or sets the total volume of the body.
    /// </summary>
    /// <value>The volume.</value>
    internal float Volume { get; set; }


    /// <summary>
    /// Gets or sets the length of the body.
    /// </summary>
    /// <value>The length.</value>
    /// <remarks>
    /// This is simply a value that is proportional to the size of the rigid body. 
    /// See <see cref="Buoyancy.Prepare"/>.
    /// </remarks>
    internal float Length { get; set; }
  }


  /// <summary>
  /// Applies a buoyancy force to create swimming bodies.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This force field applies a buoyancy force to create the effect of rigid bodies swimming in 
  /// water. The force effect is applied to all bodies in the <see cref="ForceField.AreaOfEffect"/>.
  /// Typically this will be an <see cref="GeometricAreaOfEffect"/>: A collision object models the
  /// area of the water, e.g. a box can be used to model a swimming pool and all bodies touching
  /// this box are subject to the buoyancy effect. But the area of effect does not define the 
  /// plane of the water surface - this is defined using the property <see cref="Surface"/>. 
  /// The area of effect could also be a <see cref="GlobalAreaOfEffect"/> which means that simply
  /// all rigid bodies in a simulation are subject to the buoyancy force. Forces are only applied
  /// if a part of the body is below the water surface.
  /// </para>
  /// <para>
  /// Since this effect uses a <see cref="Plane"/> to model the water surface, it cannot be used
  /// to model uneven water surface (waves, waterfalls, etc.).
  /// </para>
  /// <para>
  /// This effects also applies a drag force (damping of movement) on the swimming bodies. The 
  /// strength of the damping depends on <see cref="LinearDrag"/> and <see cref="AngularDrag"/>.
  /// To find good values for these coefficients, you can use following approach: First, set the
  /// coefficients to 0 and drop an average body into the water. The body will fall into the water
  /// and shoot back out. Repeat the experiment with increasing <see cref="LinearDrag"/> values 
  /// until you have a good value. Then drop rotating bodies in the water and increase the 
  /// <see cref="AngularDrag"/> until rotational damping fits your needs.
  /// </para>
  /// <para>
  /// This force effect stores additional information in the rigid bodies. This additional 
  /// information is computed when a rigid body starts to touch the water. The computation of this
  /// information can take some time, which can be a problem if many bodies are dropped into the
  /// water simultaneously. To avoid this problem, the method <see cref="Prepare"/> can be called
  /// to compute the additional information ahead of time (e.g. when the game level is loading).
  /// If there are several instances of <see cref="Buoyancy"/> in a simulation, it is sufficient
  /// to call <see cref="Prepare"/> only once per rigid body. The buoyancy information is shared
  /// between <see cref="Buoyancy"/> instances.
  /// </para>
  /// </remarks>
  public class Buoyancy : ForceField
  {
    // See Game Programming Gems 6

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the density of the liquid.
    /// </summary>
    /// <value>The density. The default density is <c>1000</c> (water density).</value>
    /// <remarks>
    /// Higher density values create stronger upward forces.
    /// </remarks>
    public float Density { get; set; }


    /// <summary>
    /// Gets or sets the gravity acceleration.
    /// </summary>
    /// <value>The gravity acceleration. The default value is <c>9.81</c>.</value>
    /// <remarks>
    /// The gravity acts against the normal vector direction of the <see cref="Surface"/>. This 
    /// gravity is only used to compute the magnitude of the buoyancy force. The 
    /// <see cref="Buoyancy"/> does not actually pull objects "down". The <see cref="Gravity"/>
    /// force effect is responsible for pulling objects down.
    /// </remarks>
    public float Gravity { get; set; }


    /// <summary>
    /// Gets or sets the water surface plane (in world space).
    /// </summary>
    /// <value>
    /// The water surface plane (in world space). The default value is 
    /// <c>new Plane(Vector3F.UnitY, 0)</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This plane defines the surface of the body of water. If a rigid body in the 
    /// <see cref="ForceField.AreaOfEffect"/> is partially or totally below this plane, the 
    /// buoyancy force is applied.
    /// </para>
    /// </remarks>
    public Plane Surface { get; set; }


    /// <summary>
    /// Gets or sets the angular drag coefficient.
    /// </summary>
    /// <value>The angular drag coefficient. The default value is <c>0.5</c>.</value>
    /// <remarks>
    /// The rotation of swimming rigid bodies is damped. The damping is proportional to this
    /// value.
    /// </remarks>
    public float AngularDrag { get; set; }


    /// <summary>
    /// Gets or sets the linear drag coefficient.
    /// </summary>
    /// <value>The linear drag coefficient. The default value is <c>5.0</c>.</value>
    /// <remarks>
    /// The linear movement of swimming rigid bodies is damped. The damping is proportional to this
    /// value.
    /// </remarks>
    public float LinearDrag { get; set; }


    /// <summary>
    /// Gets or sets the linear velocity of the water.
    /// </summary>
    /// <value>The linear water velocity. The default is a (0, 0, 0).</value>
    /// <remarks>
    /// This vector can be used to create flowing water that drags objects in the velocity
    /// direction.
    /// </remarks>
    public Vector3F Velocity { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Buoyancy"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Buoyancy"/> class.
    /// </summary>
    /// <remarks>
    /// The property <see cref="ForceField.AreaOfEffect"/> is initialized with a new instance of
    /// <see cref="GlobalAreaOfEffect"/>.
    /// </remarks>
    public Buoyancy()
    {
      Initialize();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Buoyancy"/> class.
    /// </summary>
    /// <param name="areaOfEffect">The area of effect.</param>
    public Buoyancy(IAreaOfEffect areaOfEffect)
      : base(areaOfEffect)
    {
      Initialize();
    }


    private void Initialize()
    {
      Density = 1000;
      Gravity = 10;
      Surface = new Plane(Vector3F.UnitY, 0);
      AngularDrag = 0.5f;
      LinearDrag = 5.0f;
      Velocity = Vector3F.Zero;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override void Apply(RigidBody body)
    {
      if (body == null)
        throw new ArgumentNullException("body", "Rigid body in area of effect must not be null.");

      // Get BuoyancyData. If necessary create new data.
      BuoyancyData data = body.BuoyancyData;
      if (data == null)
      {
        Prepare(body);
        data = body.BuoyancyData;
      }

      // Compute size of volume that is in the water and the center of the submerged volume.
      Vector3F centerOfSubmergedVolume;
      float submergedVolume = GetSubmergedVolume(body.Scale, body.Pose, data.Mesh, out centerOfSubmergedVolume);

      if (submergedVolume > 0)
      {
        // The up force.
        Vector3F buoyancyForce = (Density * submergedVolume * Gravity) * Surface.Normal;

        // The total volume of the body.
        float totalVolume = data.Volume * body.Scale.X * body.Scale.Y * body.Scale.Z;

        // The fraction of the total mass that is under the water (assuming constant density).
        float submergedMass = body.MassFrame.Mass * submergedVolume / totalVolume;

        // Compute linear drag.
        Vector3F centerLinearVelocity = body.GetVelocityOfWorldPoint(centerOfSubmergedVolume);
        Vector3F dragForce = (submergedMass * LinearDrag) * (Velocity - centerLinearVelocity);

        // Apply up force and linear drag force.
        Vector3F totalForce = buoyancyForce + dragForce;
        AddForce(body, totalForce, centerOfSubmergedVolume);

        // Apply torque for angular drag.
        // body.Length is proportional to the unscaled shape. Apply scaling to get a new 
        // proportional value for the scaled shape.
        float length = data.Length * 1.0f / 3.0f * (body.Scale.X + body.Scale.Y + body.Scale.Z);
        float lengthSquared = length * length;
        Vector3F dragTorque = (-submergedMass * AngularDrag * lengthSquared) * body.AngularVelocity;
        AddTorque(body, dragTorque);
      }
    }


    /// <summary>
    /// Prepares the specified rigid body for the buoyancy effect.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <remarks>
    /// <para>
    /// This method is automatically called for each body that touches the water. It computes
    /// additional information per rigid body that is needed for the buoyancy effect. 
    /// </para>
    /// <para>
    /// To prepare the rigid bodies ahead of time, this method can be called manually - but this
    /// is not required. It is sufficient to call this method once per rigid body, then the body
    /// is prepared for all buoyancy effect instances.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    public static void Prepare(RigidBody body)
    {
      // Must be called once per shape not really per body - but we keep this info internal in 
      // case we make changes in the future, like per-body buoyancy settings.

      if (body == null)
        throw new ArgumentNullException("body");

      Shape shape = body.Shape;

      // Try to use existing data for the same shape.
      var simulation = body.Simulation;
      if (simulation != null)
      {
        int numberOfRigidBodies = simulation.RigidBodies.Count;
        for (int i = 0; i < numberOfRigidBodies; i++)
        {
          var otherBody = simulation.RigidBodies[i];
          if (otherBody.Shape == shape && otherBody.BuoyancyData != null)
          {
            body.BuoyancyData = otherBody.BuoyancyData;
            return;
          }
        }
      }

      BuoyancyData data = new BuoyancyData();
      data.Mesh = shape.GetMesh(0.01f, 4);
      data.Volume = data.Mesh.GetVolume();
      data.Length = shape.GetAabb(Pose.Identity).Extent.LargestComponent;

      body.BuoyancyData = data;
    }


    // Computes the volume of the submerged mesh part and the center of buoyancy.
    private float GetSubmergedVolume(Vector3F scale, Pose pose, TriangleMesh mesh, out Vector3F center)
    {
      center = Vector3F.Zero;

      // Get surface plane in local space.
      Plane planeLocal = new Plane
      {
        Normal = pose.ToLocalDirection(Surface.Normal),
        DistanceFromOrigin = Surface.DistanceFromOrigin - Vector3F.Dot(Surface.Normal, pose.Position),
      };

      const float tinyDepth = -1e-6f;

      // Vertex heights relative to surface plane. Positive = above water.
      int numberOfVertices = mesh.Vertices.Count;

      // Compute depth of each vertex.
      List<float> depths = ResourcePools<float>.Lists.Obtain();

      // Use try-finally block to properly recycle resources from pool.
      try
      {
        int numberOfSubmergedVertices = 0;
        int sampleVertexIndex = 0;
        for (int i = 0; i < numberOfVertices; i++)
        {
          float depth = Vector3F.Dot(planeLocal.Normal, mesh.Vertices[i] * scale) - planeLocal.DistanceFromOrigin;
          if (depth < tinyDepth)
          {
            numberOfSubmergedVertices++;
            sampleVertexIndex = i;
          }

          depths.Add(depth);
        }

        // Abort if no vertex is in water.
        if (numberOfSubmergedVertices == 0)
          return 0;

        // Get the reference point. We project a submerged vertex onto the surface plane.
        Vector3F point = mesh.Vertices[sampleVertexIndex] - depths[sampleVertexIndex] * planeLocal.Normal;

        float volume = 0;

        // Add contribution of each triangle.
        int numberOfTriangles = mesh.NumberOfTriangles;
        for (int i = 0; i < numberOfTriangles; i++)
        {
          // Triangle vertex indices.
          int i0 = mesh.Indices[i * 3 + 0];
          int i1 = mesh.Indices[i * 3 + 1];
          int i2 = mesh.Indices[i * 3 + 2];

          // Vertices and depths.
          Vector3F v0 = mesh.Vertices[i0] * scale;
          float d0 = depths[i0];
          Vector3F v1 = mesh.Vertices[i1] * scale;
          float d1 = depths[i1];
          Vector3F v2 = mesh.Vertices[i2] * scale;
          float d2 = depths[i2];

          if (d0 * d1 < 0)
          {
            // v0 - v1 crosses the surface.
            volume += ClipTriangle(point, v0, v1, v2, d0, d1, d2, ref center);
          }
          else if (d0 * d2 < 0)
          {
            // v0 - v2 crosses the surface.
            volume += ClipTriangle(point, v2, v0, v1, d2, d0, d1, ref center);
          }
          else if (d1 * d2 < 0)
          {
            // v1 - v2 crosses the surface.
            volume += ClipTriangle(point, v1, v2, v0, d1, d2, d0, ref center);
          }
          else if (d0 < 0 || d1 < 0 || d2 < 0)
          {
            // Fully submerged triangle.
            volume += GetSignedTetrahedronVolume(point, v0, v1, v2, ref center);
          }
        }

        // If the volume is near zero or negative (numerical errors), we abort.
        const float tinyVolume = 1e-6f;
        if (volume <= tinyVolume)
        {
          center = Vector3F.Zero;
          return 0;
        }

        // Normalize the center (was weighted by volume).
        center = center / volume;

        // Transform center to world space.
        center = pose.ToWorldPosition(center);

        return volume;
      }
      finally
      {
        // Recycle temporary collections.
        ResourcePools<float>.Lists.Recycle(depths);
      }
    }


    // Clips the partially submerged triangle.
    // Returns the volume of the submerged tetrahedra. The center (weighted by the volumes)
    // is also computed.
    private static float ClipTriangle(Vector3F point, Vector3F v0, Vector3F v1, Vector3F v2, float d0, float d1, float d2, ref Vector3F center)
    {
      Debug.Assert(d0 * d1 < 0);

      Vector3F vc0 = v0 + d0 / (d0 - d1) * (v1 - v0);

      float volume = 0;

      if (d0 < 0)
      {
        if (d2 < 0)
        {
          // Two triangles in the water.
          Vector3F vc1 = v1 + d1 / (d1 - d2) * (v2 - v1);
          volume += GetSignedTetrahedronVolume(point, vc0, vc1, v0, ref center);
          volume += GetSignedTetrahedronVolume(point, vc1, v2, v0, ref center);
        }
        else
        {
          // One triangle in the water.
          Vector3F vc1 = v0 + d0 / (d0 - d2) * (v2 - v0);
          volume += GetSignedTetrahedronVolume(point, vc0, vc1, v0, ref center);
        }
      }
      else
      {
        if (d2 < 0)
        {
          // Two triangles in the water.
          Vector3F vc1 = v0 + d0 / (d0 - d2) * (v2 - v0);
          volume += GetSignedTetrahedronVolume(point, vc0, v1, v2, ref center);
          volume += GetSignedTetrahedronVolume(point, vc0, v2, vc1, ref center);
        }
        else
        {
          // One triangle in the water.
          Vector3F vc1 = v1 + d1 / (d1 - d2) * (v2 - v1);
          volume += GetSignedTetrahedronVolume(point, vc0, v1, vc1, ref center);
        }
      }

      return volume;
    }


    // Returns the volume of the tetrahedron (p, v0, v1, v2) and returns the tetrahedron center
    // weighted with the volume.
    private static float GetSignedTetrahedronVolume(Vector3F p, Vector3F v0, Vector3F v1, Vector3F v2, ref Vector3F center)
    {
      // See Game Programming Gems 6 - Chapter Buoyancy
      var a = v1 - v0;
      var b = v2 - v0;
      var r = p - v0;

      float volume = 1.0f / 6.0f * Vector3F.Dot(Vector3F.Cross(b, a), r);
      center += 0.25f * volume * (v0 + v1 + v2 + p);
      return volume;
    }
    #endregion
  }
}
