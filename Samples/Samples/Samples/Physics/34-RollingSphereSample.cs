using System;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Collisions.Algorithms;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework.Input;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates how to fight problems that occur when objects slide/roll over a 
triangle mesh.",
    @"",
    34)]
  [Controls(@"Sample
  Press <Y> on keyboard or on gamepad to toggle smooth movement settings.")]
  public class RollingSphereSample : PhysicsSample
  {
    // When rigid bodies slide or roll over a triangle mesh, it can happen that they bounce
    // when moving over a triangle edges. This happens because at triangle edges the
    // collision detection can produce sub-optimal contact normal vectors. Also other optimizations
    // in the simulation can produce sub-optimal contact positions or normal vectors. In
    // the simulation these bad collision data makes the sliding/rolling object jump as if there
    // is a small hump in the mesh.
    //
    // Here are a few tips and tricks to minimize such problems:
    // 
    // ----- Contact Welding
    // To minimize bad normals activate contact welding for the mesh, for example:
    //    triangleShapeMesh.EnableContactWelding = true;
    // and make the welding more aggressive by setting the welding limit to 1. 
    // This is a static property of the TriangleMeshAlgorithm class:
    //    TriangleMeshAlgorithm.WeldingLimit = 1f;
    // Contact welding improves the contact normal vectors at triangle edges. 
    //
    // ----- Lower friction
    // Try to use a lower friction. If the friction is lower, the bodies can glide more smoothly 
    // over bumps.
    //
    // ----- Allowed Penetration Depth
    // Try to lower the allowed penetration depth. For example: 
    //   simulation.Settings.Constraints.AllowedPenetration = 0.001f;
    // If this parameter is high, then the bodies will sink more into the triangle surface. 
    // And when it rolls to a triangle edge, it will perceive the neighbor edge as a small upwards 
    // step.
    // 
    // ----- Gravity
    // When the gravity is high, small bumps can have a higher impact. Try to use a lower gravity, 
    // for example:
    //   simulation.ForceEffects.Add(new Gravity() { Acceleration = new Vector3F(0, -5, 0)});
    //
    // ----- Height field instead of triangle mesh
    // Try to use a height field instead of a triangle mesh - if the scenario allows to use a 
    // height field. Height fields produce more robust collision detection results in many cases.
    // 
    // ----- Distorted triangles
    // If triangles of the mesh are very distorted (one side is small relative to the other sides), 
    // subdivide the mesh to create more regular triangles. Very long, distorted shapes will cause 
    // high numerical errors in many collision detection and physics algorithms.
    //
    // ----- Perfect sphere contacts 
    // Normally, when a sphere touches another object, the contact normal vector points in the 
    // direction from the sphere center to the contact. When contact welding is used, the contact 
    // positions can be a bit off. - But small errors can cause visible bounces.
    // We can correct this with a custom contact filter, like SphereContactFilter in this example.
    // Set this filter with:
    //   simulation.CollisionDomain.CollisionDetection.ContactFilter = new SphereContactFilter();


    // A custom contact filter is used by a collision domain to filter and reduce the number of 
    // contacts that are produced by the collision detection algorithms.
    private class SphereContactFilter : IContactFilter
    {
      // The default contact filter of a new CollisionDomain is a ContactReducer.
      public readonly ContactReducer DefaultContactFilter = new ContactReducer();

      /// <summary>
      /// Filters the specified contact set.
      /// </summary>
      /// <param name="contactSet">The contact set.</param>
      public void Filter(ContactSet contactSet)
      {
        // Call the default contact filter.
        DefaultContactFilter.Filter(contactSet);

        // Abort if there are no contacts in this contact set.
        if (contactSet.Count == 0)
          return;

        // If this is a sphere vs. * contact set, then we correct the position of the 
        // contact point to make sure that the contact position is in line with the sphere center.

        var sphere = contactSet.ObjectA.GeometricObject.Shape as SphereShape;
        if (sphere != null)
        {
          float radius = sphere.Radius;

          foreach (var contact in contactSet)
            contact.Position = contactSet.ObjectA.GeometricObject.Pose.Position + contact.Normal * (radius - contact.PenetrationDepth / 2);

          return;
        }

        sphere = contactSet.ObjectB.GeometricObject.Shape as SphereShape;
        if (sphere != null)
        {
          float radius = sphere.Radius;

          foreach (var contact in contactSet)
            contact.Position = contactSet.ObjectB.GeometricObject.Pose.Position - contact.Normal * (radius - contact.PenetrationDepth / 2);
        }
      }
    }


    private TimeSpan _timeUntilReset;
    private TriangleMeshShape _triangleMeshShape;
    private RigidBody _sphere;
    private SphereContactFilter _sphereContactFilter;
    private bool _enableSmoothMovement = true;
    private float _originalWeldingLimit;


    public RollingSphereSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // To demonstrate the problems with triangle meshes we increase the gravity and let a
      // sphere roll over a curved surface.

      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity { Acceleration = new Vector3F(0, -30, 0) });   // Higher gravity to make the problem more visible.
      Simulation.ForceEffects.Add(new Damping());

      // Use the custom contact filter to improve sphere contacts.
      _sphereContactFilter = new SphereContactFilter();
      Simulation.CollisionDomain.CollisionDetection.ContactFilter = _sphereContactFilter;

      // The triangle mesh could be loaded from a file, such as an XNA Model.
      // In this example will create a height field and convert the height field into a triangle mesh.
      var numberOfSamplesX = 60;
      var numberOfSamplesZ = 10;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int z = 0; z < numberOfSamplesZ; z++)
        for (int x = 0; x < numberOfSamplesX; x++)
          samples[z * numberOfSamplesX + x] = (float)(Math.Sin(x / 6f) * 10f + 5f);

      var heightField = new HeightField(0, 0, 70, 30, samples, numberOfSamplesX, numberOfSamplesZ);

      // Convert the height field to a triangle mesh.
      ITriangleMesh mesh = heightField.GetMesh(0.01f, 3);

      // Create a shape for the triangle mesh.
      _triangleMeshShape = new TriangleMeshShape(mesh);

      // Enable contact welding. And set the welding limit to 1 for maximal effect.
      _triangleMeshShape.EnableContactWelding = true;
      _originalWeldingLimit = TriangleMeshAlgorithm.WeldingLimit;
      TriangleMeshAlgorithm.WeldingLimit = 1;

      // Optional: Assign a spatial partitioning scheme to the triangle mesh. (A spatial partition
      // adds an additional memory overhead, but it improves collision detection speed tremendously!)
      _triangleMeshShape.Partition = new CompressedAabbTree() { BottomUpBuildThreshold = 0 };

      // Create a static rigid body using the shape and add it to the simulation.
      // We explicitly specify a mass frame. We can use any mass frame for static bodies (because
      // static bodies are effectively treated as if they have infinite mass). If we do not specify 
      // a mass frame in the rigid body constructor, the constructor will automatically compute an 
      // approximate mass frame (which can take some time for large meshes).
      var ground = new RigidBody(_triangleMeshShape, new MassFrame(), null)
      {
        Pose = new Pose(new Vector3F(-34, 0, -40f)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(ground);

      SphereShape sphereShape = new SphereShape(0.5f);
      _sphere = new RigidBody(sphereShape);
      Simulation.RigidBodies.Add(_sphere);

      _enableSmoothMovement = true;
      _timeUntilReset = TimeSpan.Zero;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        TriangleMeshAlgorithm.WeldingLimit = _originalWeldingLimit;
      }

      base.Dispose(disposing);
    }


    public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
      // Create a sphere that rolls down the triangle mesh. The sphere pose and velocity
      // is reset periodically.

      _timeUntilReset -= gameTime.ElapsedGameTime;

      if (_timeUntilReset <= TimeSpan.Zero)
      {
        _timeUntilReset = new TimeSpan(0, 0, 0, 8); // 8 seconds.

        // Reset position and velocity of the sphere.
        _sphere.Pose = new Pose(new Vector3F(-20, 20, -15));
        _sphere.LinearVelocity = Vector3F.Zero;
        _sphere.AngularVelocity = Vector3F.Zero;
      }

      // If Y is pressed, toggle smooth movement tricks.
      if (InputService.IsPressed(Keys.Y, true) || InputService.IsPressed(Buttons.Y, true, LogicalPlayerIndex.One))
      {
        _enableSmoothMovement = !_enableSmoothMovement;
        _triangleMeshShape.EnableContactWelding = _enableSmoothMovement;

        if (_enableSmoothMovement)
          Simulation.CollisionDomain.CollisionDetection.ContactFilter = _sphereContactFilter;
        else
          Simulation.CollisionDomain.CollisionDetection.ContactFilter = _sphereContactFilter.DefaultContactFilter;
      }


      base.Update(gameTime);

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.DrawText("\n\nSmooth Movement: "
        + (_enableSmoothMovement ? "Enabled (sphere should roll smoothly)"
                                 : "Disabled (sphere might bounce at triangle edges)"));
    }
  }
}
