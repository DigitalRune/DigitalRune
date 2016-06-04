#if !WP7 && !WP8
using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to create distortion effects.",
    @"A new post-processing effect called DistortionFilter is implemented, which distorts the scene.
The distortion direction and strength is controlled by a distortion texture. The distortion texture
is created by the DistortionFilter: The DistortionFilter has a scene and the content of this scene is
rendered into the distortion texture.
The sample adds particle systems to the scene. The particle texture is similar to a normal map. The
red and green channels control the distortion offset.
One particle system adds distortion to the campfire. The second particle system creates a distortion
from an explosion. The third creates a nova-shaped distortion.
The distortion texture is visualized in the lower right screen corner.",
    121)]
  [Controls(@"Sample
Press <H> to toggle distortion of campfire.
Press <J> to trigger explosion effect.
Press <K> to trigger nova effect.")]
  public class DistortionSample : Sample
  {
    // Notes: 
    // This sample does not explain how to setup and use particle systems. Check out the particle
    // Samples in this sample project for detailed comments.


    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly DistortionFilter _distortionFilter;

    private readonly ParticleSystemNode _fireDistortionParticleSystemNode;
    private readonly ParticleSystemNode _explosionDistortionParticleSystemNode;
    private readonly ParticleSystemNode _novaDistortionParticleSystemNode;


    public DistortionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.Register(typeof(DebugRenderer), null, _graphicsScreen.DebugRenderer);
      Services.Register(typeof(IScene), null, _graphicsScreen.Scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services)); // Skybox + some lights.

      //GameObjectService.Objects.Add(new GroundObject(Services));
      // Add a ground plane with some detail to see the water refractions.
      Simulation.RigidBodies.Add(new RigidBody(new PlaneShape(new Vector3F(0, 1, 0), 0)));
      GameObjectService.Objects.Add(new StaticObject(Services, "Gravel/Gravel", 1, new Pose(new Vector3F(0, 0.001f, 0))));

      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new DynamicObject(Services, 1));
      GameObjectService.Objects.Add(new DynamicObject(Services, 2));
      GameObjectService.Objects.Add(new DynamicObject(Services, 3));
      GameObjectService.Objects.Add(new DynamicObject(Services, 5));
      GameObjectService.Objects.Add(new DynamicObject(Services, 6));
      GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new CampfireObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = new Vector3F(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree", scale, new Pose(position, orientation)));
      }

      // Add DistortionFilter to post-processors.
      _distortionFilter = new DistortionFilter(GraphicsService, ContentManager);
      _graphicsScreen.PostProcessors.Add(_distortionFilter);

      // Add 3 particle systems. 
      // The ParticleSystems are added to the IParticleSystemService.
      // The ParticleSystemNodes are added to the Scene of the DistortionFilter - not the usual Scene!
      _fireDistortionParticleSystemNode = new ParticleSystemNode(CreateFireDistortionParticleSystem())
      {
        PoseLocal = new Pose(new Vector3F(0, 0f, -1), Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      ParticleSystemService.ParticleSystems.Add(_fireDistortionParticleSystemNode.ParticleSystem);
      _distortionFilter.Scene.Children.Add(_fireDistortionParticleSystemNode);

      _explosionDistortionParticleSystemNode = new ParticleSystemNode(CreateExplosionDistortionParticleSystem())
      {
        PoseLocal = new Pose(new Vector3F(0, 0, -1)),
      };
      ParticleSystemService.ParticleSystems.Add(_explosionDistortionParticleSystemNode.ParticleSystem);
      _distortionFilter.Scene.Children.Add(_explosionDistortionParticleSystemNode);

      _novaDistortionParticleSystemNode = new ParticleSystemNode(CreateNovaDistortionParticleSystem())
      {
        PoseLocal = new Pose(new Vector3F(0, 0.5f, -1), Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      ParticleSystemService.ParticleSystems.Add(_novaDistortionParticleSystemNode.ParticleSystem);
      _distortionFilter.Scene.Children.Add(_novaDistortionParticleSystemNode);
    }


    private ParticleSystem CreateFireDistortionParticleSystem()
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "FireDistortion",
        MaxNumberOfParticles = 10,
      };

      ps.ReferenceFrame = ParticleReferenceFrame.Local;

      // Lifetime
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(1.5f, 1.8f),
      });

      // Emitter
      ps.Effectors.Add(new StreamEmitter { DefaultEmissionRate = 5, });

      // Position
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new CircleDistribution { OuterRadius = 0.4f, InnerRadius = 0 }
      });

      // Velocity
      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Direction).DefaultValue = Vector3F.Forward;
      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0, 1),
      });
      ps.Effectors.Add(new LinearVelocityEffector());

      // Damping
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleDampingEffector());

      // Wind
      ps.Parameters.AddUniform<Vector3F>("Wind").DefaultValue = new Vector3F(-1, -0.5f, -3);//new Vector3F(-1, 3, -0.5f);
      ps.Effectors.Add(new LinearAccelerationEffector { AccelerationParameter = "Wind" });

      // Angle
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });

      // Angular Velocity
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-2f, 2f),
      });
      ps.Effectors.Add(new AngularVelocityEffector());

      // Size
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Size).DefaultValue = 1.5f;

      // Alpha
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        FadeInStart = 0.0f,
        FadeInEnd = 0.1f,
        FadeOutStart = 0.8f,
        FadeOutEnd = 1.0f,
      });

      // Texture
      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        ContentManager.Load<Texture2D>("Particles/Distortion");

      // Softness
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Softness).DefaultValue = float.NaN; // NaN = automatic

      // Bounding shape
      ps.Shape = new TransformedShape(new GeometricObject(new BoxShape(2.5f, 2.5f, 4f), new Pose(new Vector3F(0, 0, -1))));

      return ps;
    }


    private ParticleSystem CreateExplosionDistortionParticleSystem()
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "ExplosionDistortion",
        MaxNumberOfParticles = 60,
      };

      ps.ReferenceFrame = ParticleReferenceFrame.Local;

      // Lifetime
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 0.5f;

      // Position
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3F.Zero,
      });

      // Velocity
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new SphereDistribution { InnerRadius = 1.0f, OuterRadius = 1.0f },
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(10, 20),
      });
      ps.Effectors.Add(new LinearVelocityEffector());

      // Angle
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });

      // Angular Velocity
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-0.4f, 0.4f),
      });
      ps.Effectors.Add(new AngularVelocityEffector());

      // Size
      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(2f, 4.0f),
      });
      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(6f, 16.0f),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      // Alpha
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.05f,
        FadeOutStart = 0.5f,
        FadeOutEnd = 1.0f,
      });

      // Texture
      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        ContentManager.Load<Texture2D>("Particles/Distortion");

      // Softness
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Softness).DefaultValue = float.NaN; // NaN = automatic

      // Bounding shape
      ps.Shape = new SphereShape(10);

      return ps;
    }


    private ParticleSystem CreateNovaDistortionParticleSystem()
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "NovaDistortion",
        MaxNumberOfParticles = 80,
      };

      ps.ReferenceFrame = ParticleReferenceFrame.Local;

      // Lifetime
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 0.3f;

      // Position
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3F.Zero,
      });

      // Velocity
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new CircleDistribution { InnerRadius = 1.0f, OuterRadius = 1.0f },
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.LinearSpeed).DefaultValue = 20;
      ps.Effectors.Add(new LinearVelocityEffector());

      // Size
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Size).DefaultValue = 2;

      // Alpha
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.01f,
        FadeOutStart = 0.90f,
        FadeOutEnd = 1.0f,
      });

      // Texture
      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        ContentManager.Load<Texture2D>("Particles/Distortion");

      // Softness
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Softness).DefaultValue = float.NaN; // NaN = automatic

      // Bounding shape
      ps.Shape = new BoxShape(15, 15, 2);

      return ps;
    }


    public override void Update(GameTime gameTime)
    {
      // Toggle fire distortion.
      if (InputService.IsPressed(Keys.H, false))
      {
        bool isEnabled = _fireDistortionParticleSystemNode.IsEnabled;
        _fireDistortionParticleSystemNode.IsEnabled = !isEnabled;
        _fireDistortionParticleSystemNode.ParticleSystem.Enabled = !isEnabled;
      }

      // Trigger explosion.
      if (InputService.IsPressed(Keys.J, false))
        _explosionDistortionParticleSystemNode.ParticleSystem.AddParticles(20);

      // Trigger nova
      if (InputService.IsPressed(Keys.K, false))
        _novaDistortionParticleSystemNode.ParticleSystem.AddParticles(40);

      // Synchronize particle data and render data.
      _fireDistortionParticleSystemNode.Synchronize(GraphicsService);
      _explosionDistortionParticleSystemNode.Synchronize(GraphicsService);
      _novaDistortionParticleSystemNode.Synchronize(GraphicsService);

      var debugRenderer = _graphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      //debugRenderer.DrawObject(_fireDistortionParticleSystemNode, Color.Red, true, false);
      //debugRenderer.DrawObject(_explosionDistortionParticleSystemNode, Color.Green, true, false);
      //debugRenderer.DrawObject(_novaDistortionParticleSystemNode, Color.Blue, true, false);

      // Draw distortion texture in lower right corner.
      int height = GraphicsService.GraphicsDevice.PresentationParameters.BackBufferHeight;
      debugRenderer.DrawTexture(_distortionFilter.DistortionTexture, new Rectangle(0, height - 200, 200, 200));
    }
  }
}
#endif