// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a body of water, e.g. a lake, river or an infinite ocean.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The appearance (color, reflection, refraction, etc.) of the water is defined by the
  /// property <see cref="Water"/>. 
  /// </para>
  /// <para>
  /// The <see cref="Volume"/> defines the surface and the underwater volume. If 
  /// <see cref="Volume"/> is  <see langword="null"/>, an infinite water plane (e.g. for an ocean) 
  /// is created. The top surface of the water volume should go through the local origin of the
  /// <see cref="WaterNode"/>. This is expected for depth-based water color computations.
  /// </para>
  /// <para>
  /// The water supports planar reflections (see <see cref="PlanarReflection"/>) and cube map/skybox
  /// reflections (see <see cref="SkyboxReflection"/>). The water can flow into user-defined
  /// directions, defined in <see cref="Flow"/>. The water surface can also be displaced using a
  /// displacement map to create waves (see <see cref="Waves"/>).
  /// </para>
  /// <para>
  /// <see cref="WaterNode"/>s are rendered by the <see cref="WaterRenderer"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="WaterNode"/> is cloned the properties 
  /// <see cref="Water"/>, <see cref="Volume"/>, <see cref="PlanarReflection"/>,
  /// <see cref="SkyboxReflection"/>, <see cref="Flow"/> and <see cref="Waves"/> are copied by
  /// reference (shallow copy). The original and the cloned node will reference the same instances.
  /// </para>
  /// </remarks>
  /// <seealso cref="Graphics.Water"/>
  public class WaterNode : SceneNode
  {
    // Notes:
    // This node can perform IsUnderwater tests, it caches the single last result. 
    // The IsDirty flag is used to invalidate the cached result.

    //--------------------------------------------------------------
    #region Static Fields
    //--------------------------------------------------------------

    // For underwater tests. Use these only in a critical section.
    private static readonly object _underwaterTestLock = new object();
    private static SphereShape _sphereShape;
    private static RayShape _rayShape;
    private static CollisionObject _testCollisionObject;
    private static CollisionObject _waterCollisionObject;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached underwater test result.
    private Vector3F _lastTestPosition = new Vector3F(float.NaN);
    private bool _lastTestResult;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the water properties.
    /// </summary>
    /// <value>The water properties.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    public Water Water
    {
      get { return _water; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _water = value;
      }
    }
    private Water _water;


    /// <summary>
    /// Gets or sets a value indicating whether an underwater effect should be displayed
    /// if the camera is underwater.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if an underwater effect is displayed when the camera is underwater; 
    /// otherwise, <see langword="false" />. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Set this property to <see langword="true"/> if the player can dive into the water. If this
    /// value is <see langword="false"/>, the objects under the water surface are rendered normally
    /// when the camera peeks under the water surface. To check if an object is underwater, use
    /// the <see cref="IsUnderwater"/> method.
    /// </para>
    /// </remarks>
    public bool EnableUnderwaterEffect { get; set; }


    /// <summary>
    /// Gets or sets the shape which defines the water volume.
    /// </summary>
    /// <value>
    /// The shape which defines the water volume. Can be <see langword="null"/> to create an
    /// infinite ocean plane. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this value is <see langword="null"/> (default), then an infinite plane of water is
    /// rendered. The water plane is horizontal in world space and goes through the local origin of
    /// the <see cref="WaterNode"/>.
    /// </para>
    /// <para>
    /// If the <see cref="Volume"/> is set to a shape, then the water is only rendered in the shape.
    /// The shape defines the water surface as well as the underwater volume. Examples:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// A <see cref="RectangleShape"/> can be used to define a swimming pool without an underwater
    /// effect.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    ///  A <see cref="BoxShape"/> can be used to define a swimming pool with an underwater effect-
    ///  which means, a special effect is rendered when the player is diving in the swimming pool.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A <see cref="TriangleMeshShape"/> can be used to define a complex meandering river.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// The top surface of the water volume should go through the local origin of the
    /// <see cref="WaterNode"/>. This is expected for depth-based water color computations.
    /// </para>
    /// <para>
    /// If the water should be rendered with displacement mapping (see <see cref="Waves"/>), then the
    /// <see cref="Volume"/> must be a shape with a tessellated surface because the displacement is
    /// applied to vertices. This means, that waves will not work with a simple
    /// <see cref="RectangleShape"/>. You have to specify a <see cref="TriangleMeshShape"/> with a
    /// sufficient level of tessellation on the top side of the shape. If the <see cref="Volume"/> is
    /// <see langword="null"/>, the renderer will automatically use a tessellated mesh (e.g. a
    /// projected grid) if required.
    /// </para>
    /// </remarks>
    public Shape Volume
    {
      get { return _volume; }
      set
      {
        if (_volume == value)
          return;

        if (_volume != null)
          _volume.Changed -= OnVolumeChanged;

        _volume = value;
        Update(true);

        if (_volume != null)
          _volume.Changed += OnVolumeChanged;
      }
    }
    private Shape _volume;


    /// <summary>
    /// Gets or sets the extra height added to the bounding shape.
    /// </summary>
    /// <value>The extra height added to the bounding shape. The default value is 0.</value>
    /// <remarks>
    /// <para>
    /// This value is only relevant if displacement mapping (see <see cref="Waves"/>) is applied to
    /// the water. This property must specify the max wave height above the water surface (defined
    /// by <see cref="Volume"/>). The best way to set this property is to visualize the bounding
    /// shape (<see cref="SceneNode.Shape"/>) of the scene node and increase the
    /// <see cref="ExtraHeight"/> until all waves are within the bounding shape.
    /// </para>
    /// <para>
    /// If the this value is not set properly, then bounding shape of the water used for view
    /// frustum culling might be too small, and the water might be culled even if some waves should
    /// be visible.
    /// </para>
    /// </remarks>
    public float ExtraHeight
    {
      get { return _extraHeight; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (value == _extraHeight)
          return;

        _extraHeight = value;
        Update(false);
      }
    }
    private float _extraHeight;


    ///// <summary>
    ///// Gets or sets the maximum depth of the water node.
    ///// </summary>
    ///// <value>The maximum depth. The default value is 100.</value>
    ///// <remarks>
    ///// This property defines the max depth of the water used for depth-based rendering effects,
    ///// like underwater fog. Normally, the depth is defined by the ground meshes under the water
    ///// surface. But if the ground mesh has holes, then <see cref="MaxDepth"/> is used. This is
    ///// usually used for an ocean where underwater geometry is only define near the shores.
    ///// </remarks>
    //public float MaxDepth { get; set; }


    /// <summary>
    /// Gets or sets the planar reflection.
    /// </summary>
    /// <value>The planar reflection.</value>
    /// <remarks>
    /// Usually, you will want to set a <see cref="PlanarReflection"/> or a
    /// <see cref="SkyboxReflection"/>, but not both. If both are set then the renderer can choose
    /// one of them, or combine them (e.g. to blend to the skybox if some reflected rays are not
    /// inside the planar reflection texture).
    /// </remarks>
    public PlanarReflectionNode PlanarReflection { get; set; }


    /// <summary>
    /// Gets or sets the skybox that is reflected.
    /// </summary>
    /// <value>The skybox that is reflected.</value>
    /// <inheritdoc cref="PlanarReflection"/>
    public SkyboxNode SkyboxReflection { get; set; }


    /// <summary>
    /// Gets or sets the water flow used to define water movement.
    /// </summary>
    /// <value>
    /// The water flow, used to define water movement. The default value is <see langword="null"/>.
    /// </value>
    public WaterFlow Flow { get; set; }


    // TODO: Waves or Waves0/1 or WaveLayers or Waves +ImpactWaves?

    /// <summary>
    /// Gets or sets the maps used to displace the water surface.
    /// </summary>
    /// <value>
    /// The maps used to displace the water surface. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The class <see cref="WaterWaves"/> is used to define a displacement map and a normal map
    /// which are applied to the water surface. The displacement map is used to move vertices,
    /// therefore the water <see cref="Volume"/> must have a tessellated surface. See also
    /// <see cref="Volume"/>.
    /// </para>
    /// <para>
    /// <strong>Important: <see cref="ExtraHeight"/></strong><br/>
    /// When a displacement mapping is used, the property <see cref="ExtraHeight"/> must be set to a
    /// value which is larger than the max wave displacement. See <see cref="ExtraHeight"/> for
    /// more details.
    /// </para>
    /// </remarks>
    public WaterWaves Waves { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the water surface is rendered into the depth buffer.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the water surface is rendered into the depth buffer; otherwise,
    /// <see langword="false"/> if the water does not change the depth buffer. The default value is
    /// <see langword="false"/>.
    /// </value>
    public bool DepthBufferWriteEnable { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="WaterNode" /> class.
    /// </summary>
    /// <param name="water">The water.</param>
    /// <param name="volume">
    /// The water volume. Can be <see langword="null"/>, see <see cref="Volume"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="water"/> is <see langword="null"/>.
    /// </exception>
    public WaterNode(Water water, Shape volume)
    {
      if (water == null)
        throw new ArgumentNullException("water");

      _water = water;
      Volume = volume;

      // Normally, Volume setter automatically calls Update() - but not when volume is null.
      if (Volume == null)
        Update(false);

      // The IsRenderable flag needs to be set to indicate that the scene node should 
      // be handled during rendering.
      IsRenderable = true;

      EnableUnderwaterEffect = true;

      //MaxDepth = 100;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          if (disposeData)
          {
            Flow.SafeDispose();
            Waves.SafeDispose();
          }
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new WaterNode Clone()
    {
      return (WaterNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new WaterNode(_water, _volume);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone the SceneNode properties (base class).
      base.CloneCore(source);

      // Clone the RenderToTextureNode properties.
      var sourceTyped = (WaterNode)source;
      EnableUnderwaterEffect = sourceTyped.EnableUnderwaterEffect;
      ExtraHeight = sourceTyped.ExtraHeight;
      //MaxDepth = sourceTyped.MaxDepth;
      PlanarReflection = sourceTyped.PlanarReflection;
      SkyboxReflection = sourceTyped.SkyboxReflection;
      Flow = sourceTyped.Flow;
      Waves = sourceTyped.Waves;
      DepthBufferWriteEnable = sourceTyped.DepthBufferWriteEnable;
    }
    #endregion


    private void OnVolumeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      Update(true);
    }


    private void Update(bool invalidateRenderData)
    {
      if (invalidateRenderData)
        RenderData.SafeDispose();

      // Update shape.
      if (Volume == null)
      {
        // Use a PlaneShape for an infinite ocean.

        var planeShape = Shape as PlaneShape;
        if (planeShape != null)
        {
          planeShape.Normal = new Vector3F(0, 1, 0);
          planeShape.DistanceFromOrigin = ExtraHeight;
        }
        else
        {
          Shape = new PlaneShape(new Vector3F(0, 1, 0), ExtraHeight);
        }

        return;
      }

      // Check if we have a valid AABB.
      var aabb = Volume.GetAabb();

      if (!Numeric.IsZeroOrPositiveFinite(aabb.Extent.LengthSquared))
        throw new GraphicsException("Invalid water volume. The water volume must be a finite shape or null.");

      // Apply ExtraHeight. We also apply it horizontally because choppy waves
      // move vertices horizontally too.
      aabb.Minimum.X -= ExtraHeight;

      // Minimum y should be at least max y - ExtraHeight.
      aabb.Minimum.Y = Math.Min(aabb.Minimum.Y, aabb.Maximum.Y - ExtraHeight);
      aabb.Minimum.Z -= ExtraHeight;
      aabb.Maximum.X += ExtraHeight;
      aabb.Maximum.Y += ExtraHeight;
      aabb.Maximum.Z += ExtraHeight;

      // Create shape from volume AABB.
      if (aabb.Center.IsNumericallyZero)
      {
        // Use BoxShape.
        var boxShape = Shape as BoxShape;
        if (boxShape != null)
          boxShape.Extent = aabb.Extent;
        else
          Shape = new BoxShape(aabb.Extent);
      }
      else
      {
        BoxShape boxShape = null;
        var transformedShape = Shape as TransformedShape;
        if (transformedShape != null)
          boxShape = transformedShape.Child.Shape as BoxShape;

        if (boxShape != null)
        {
          boxShape.Extent = aabb.Extent;
          ((GeometricObject)transformedShape.Child).Pose = new Pose(aabb.Center);
        }
        else
        {
          Shape = new TransformedShape(
            new GeometricObject(new BoxShape(aabb.Extent), new Pose(aabb.Center)));
        }
      }
    }


    /// <summary>
    /// Determines whether the specified world space position is underwater.
    /// </summary>
    /// <param name="position">The position in world space.</param>
    /// <returns>
    /// <see langword="true"/> if the position is underwater; otherwise, <see langword="false"/>
    /// </returns>
    /// <remarks>
    /// A position is underwater if it is inside the <see cref="Shape"/> of this node.
    /// </remarks>
    public bool IsUnderwater(Vector3F position)
    {
      //if (!EnableUnderwaterEffect)
      //  return false;

      // Oceans are treated like a horizontal plane through the node origin.
      if (Volume == null)
        return position.Y < PoseWorld.Position.Y;

      // Thread-safety: We lock this operation because all tests use the same cache 
      // and test objects.
      lock (_underwaterTestLock)
      {
        // Return cached result if the point and the water pose/shape are still the same.
        if (!IsDirty)
        {
          if (Vector3F.AreNumericallyEqual(position, _lastTestPosition))
            return _lastTestResult;
        }
        else
        {
          // Clear flag. We will cache a new result.
          IsDirty = false;
        }

        _lastTestPosition = position;
        _lastTestResult = false;

        // Use a shared collision detection instance.
        var collisionDetection = SceneHelper.CollisionDetection;

        if (_sphereShape == null)
        {
          // First time initializations.
          _sphereShape = new SphereShape(0);
          _rayShape = new RayShape();
          _testCollisionObject = new CollisionObject(TestGeometricObject.Create());
          _waterCollisionObject = new CollisionObject(TestGeometricObject.Create());
        }

        var testGeometricObject = (TestGeometricObject)_testCollisionObject.GeometricObject;
        var waterGeometricObject = (TestGeometricObject)_waterCollisionObject.GeometricObject;
        try
        {
          // Initialize water collision object.
          waterGeometricObject.Shape = Volume;
          waterGeometricObject.Scale = ScaleWorld;
          waterGeometricObject.Pose = PoseWorld;

          // Test if point touches underwater volume. (Skip this test for triangle mesh shapes.)
          if (!(Shape is TriangleMeshShape))
          {
            testGeometricObject.Pose = new Pose(position);
            testGeometricObject.Shape = _sphereShape;
            if (collisionDetection.HaveContact(_testCollisionObject, _waterCollisionObject))
            {
              _lastTestResult = true;
              return true;
            }

            // For convex shapes, the above test is sufficient.
            if (Shape is ConvexShape)
              return false;
          }

          // For triangle meshes - which are hollow - we have to make a more complex test.
          // We shoot vertical rays and check if we hit the underwater volume surface.

          // Make explicit point vs. AABB test first.
          if (!collisionDetection.HaveAabbContact(_testCollisionObject, _waterCollisionObject))
            return false;

          // Switch to ray shape.
          testGeometricObject.Shape = _rayShape;

          // Shoot down. Start 1 unit above the surface.
          Vector3F origin = position;
          origin.Y = Math.Max(Aabb.Maximum.Y, origin.Y) + 1;
          _rayShape.Origin = origin;
          _rayShape.Length = (origin - position).Length;
          _rayShape.Direction = Vector3F.Down;
          if (!collisionDetection.HaveContact(_testCollisionObject, _waterCollisionObject))
            return false; // Camera is above water.

          // Shoot up. Start 1 m under the water volume.
          origin = position;
          origin.Y = Math.Min(Aabb.Minimum.Y, origin.Y) - 1;
          _rayShape.Origin = origin;
          _rayShape.Length = (origin - position).Length;
          _rayShape.Direction = Vector3F.Up;

          _lastTestResult = collisionDetection.HaveContact(_testCollisionObject, _waterCollisionObject);
          return _lastTestResult;
        }
        finally
        {
          // Remove references to avoid "memory leaks".
          waterGeometricObject.Shape = Volume;
        }
      }
    }
    #endregion
  }
}
