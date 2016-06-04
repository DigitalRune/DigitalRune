using System.Linq;
using DigitalRune.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Particles
{
  [Sample(SampleCategory.Particles,
    @"This sample uses a particle sample to draw decals. It also shows how to manually initialize
particles.",
    @"Note: This sample demonstrates capabilities of DigitalRune Particles. But in a real game, 
decals are not created using generic particle systems",
    14)]
  [Controls(@"Sample
  Press the <Left Mouse> or <Right Trigger> to place a decal.")]
  public class DecalSample : ParticleSample
  {
    private readonly ParticleSystem _decals;
    private readonly ParticleSystemNode _particleSystemNode;


    public DecalSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.DrawReticle = true;
      SetCamera(new Vector3F(0, 2, 6), 0, 0);

      _decals = Decals.Create(ContentManager);
      ParticleSystemService.ParticleSystems.Add(_decals);

      _particleSystemNode = new ParticleSystemNode(_decals);
      GraphicsScreen.Scene.Children.Add(_particleSystemNode);
    }


    public override void Update(GameTime gameTime)
    {
      if (InputService.IsPressed(MouseButtons.Left, true) || InputService.IsPressed(Buttons.RightTrigger, true, LogicalPlayerIndex.One))
      {
        var cameraPose = GraphicsScreen.CameraNode.PoseWorld;
        Vector3F cameraPosition = cameraPose.Position;
        Vector3F cameraDirection = cameraPose.ToWorldDirection(Vector3F.Forward);

        // Create a ray for picking.
        RayShape ray = new RayShape(cameraPosition, cameraDirection, 1000);

        // The ray should stop at the first hit. We only want the first object.
        ray.StopsAtFirstHit = true;

        // The collision detection requires a CollisionObject.
        CollisionObject rayCollisionObject = new CollisionObject(new GeometricObject(ray, Pose.Identity));

        // Get the first object that has contact with the ray.
        ContactSet contactSet = Simulation.CollisionDomain.GetContacts(rayCollisionObject).FirstOrDefault();
        if (contactSet != null && contactSet.Count > 0)
        {
          // The ray has hit something.

          // The contact set contains all detected contacts between the ray and the rigid body.
          // Get the first contact in the contact set. (A ray hit usually contains exactly 1 contact.)
          Contact contact = contactSet[0];
          var hitPosition = contact.Position;
          var normal = contact.Normal;
          if (contactSet.ObjectA == rayCollisionObject)
            normal = -normal;

          // The particle parameter arrays are circular buffers. Get the particle array index 
          // where the next particle is created:
          int particleIndex = (_decals.ParticleStartIndex + _decals.NumberOfActiveParticles) % _decals.MaxNumberOfParticles;

          // Add 1 particle.
          int numberOfCreatedParticles = _decals.AddParticles(1, null);
          if (numberOfCreatedParticles > 0)
          {
            // We initialize the particle parameters Position, Normal and Axis manually using
            // the results of the collision detection:
            var positionParameter = _decals.Parameters.Get<Vector3F>(ParticleParameterNames.Position);
            positionParameter.Values[particleIndex] = hitPosition + normal * 0.01f;  // We add a slight 1 cm offset to avoid z-fighting.

            var normalParameter = _decals.Parameters.Get<Vector3F>("Normal");
            normalParameter.Values[particleIndex] = normal;

            var axisParameter = _decals.Parameters.Get<Vector3F>("Axis");
            axisParameter.Values[particleIndex] = (normal == Vector3F.Up) ? Vector3F.Backward : Vector3F.Up;
          }
        }
      }

      // Synchronize particles <-> graphics.
      _particleSystemNode.Synchronize(GraphicsService);

      Profiler.AddValue("ParticleCount", ParticleHelper.CountNumberOfParticles(ParticleSystemService.ParticleSystems));
    }
  }
}
