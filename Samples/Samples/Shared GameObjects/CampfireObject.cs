using System;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles;
using DigitalRune.Particles.Effectors;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // Adds particle effects for fire and smoke and a flickering point light.
  public class CampfireObject : GameObject
  {
    private readonly IServiceLocator _services;
    private readonly IGraphicsService _graphicsService;
    private SceneNode _campfire;
    private ParticleSystemNode _fireParticles;
    private ParticleSystemNode _smokeParticles;
    private LightNode _light;

    private Random _random = new Random();
    private float _elapsed;


    public bool IsEnabled
    {
      get { return _campfire.IsEnabled; }
      set
      {
        _campfire.IsEnabled = value;
        _fireParticles.IsEnabled = value;
        _smokeParticles.IsEnabled = value;
      }
    }


    public CampfireObject(IServiceLocator services)
    {
      _services = services;
      _graphicsService = _services.GetInstance<IGraphicsService>();
      Name = "Campfire";
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      var particleSystemService = _services.GetInstance<IParticleSystemService>();

      // The campfire consists of two particle systems (fire + smoke) and a light source.
      // 
      //   _campfire (SceneNode)
      //      |
      //      +-- _fireParticles (ParticleSystemNode)
      //      |
      //      +-- _smokeParticles (ParticleSystemNode)
      //      |
      //      +-- _light (LightNode)

      // Use a basic scene node as the root node for the campfire.
      _campfire = new SceneNode
      {
        Name = "Campfire",
        PoseLocal = new Pose(new Vector3F(0, 0, -1)),
        Children = new SceneNodeCollection()
      };

      // Add fire particles.
      var contentManager = _services.GetInstance<ContentManager>();
      var particleSystem = CreateFire(contentManager);
      particleSystemService.ParticleSystems.Add(particleSystem);
      _fireParticles = new ParticleSystemNode(particleSystem)
      {
        // The fire effect lies in the xy plane and shoots into the forward direction (= -z axis).
        // Therefore we rotate the particle system to shoot upwards.
        PoseLocal = new Pose(new Vector3F(0, 0.2f, 0), Matrix33F.CreateRotationX(ConstantsF.PiOver2))
      };
      _campfire.Children.Add(_fireParticles);

      // Add smoke particles.
      particleSystem = CreateSmoke(contentManager);
      particleSystemService.ParticleSystems.Add(particleSystem);
      _smokeParticles = new ParticleSystemNode(particleSystem)
      {
        PoseLocal = new Pose(new Vector3F(0, 0.2f, 0), Matrix33F.CreateRotationX(ConstantsF.PiOver2))
      };
      _campfire.Children.Add(_smokeParticles);

      // Add a point light that illuminates the environment.
      var light = new PointLight
      {
        Attenuation = 0.1f,
        Color = new Vector3F(1, 0.2f, 0),
        HdrScale = 20,
        Range = 4
      };
      _light = new LightNode(light)
      {
        // Optional: We can make this light cast shadows - but this will cost performance!
        //Shadow = new CubeMapShadow { PreferredSize = 64, FilterRadius = 2, JitterResolution = 2048 },
        PoseLocal = new Pose(new Vector3F(0, 1f, 0))
      };
      _campfire.Children.Add(_light);

      // Add campfire to scene.
      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(_campfire);

      // Particle effects can be added multiple times to the scene (= "instancing").
      // Uncomment the following lines to add a few more instance to the scene.
      //for (int i = 0; i < 10; i++)
      //{
      //  var clone = _campfire.Clone();

      //  // Set random scale, position, orientation.
      //  clone.ScaleLocal = _random.NextVector3F(0.5f, 1.5f);
      //  var pose = _campfire.PoseWorld;
      //  pose.Position.X += _random.NextFloat(-10, 10);
      //  pose.Position.Z += _random.NextFloat(-10, 10);
      //  pose.Orientation = Matrix33F.CreateRotationY(_random.NextFloat(-ConstantsF.PiOver2, ConstantsF.PiOver2));
      //  clone.PoseLocal = pose;

      //  scene.Children.Add(clone);
      //}

      // Add GUI controls to the Options window.
      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "CampfireObject");
      SampleHelper.AddCheckBox(
        panel,
        "Enable campfire",
        IsEnabled,
        isChecked => IsEnabled = isChecked);
    }


    // Check out the ParticleSample ("Samples/DigitalRune.Particles/ParticleSample")
    // to learn more about DigitalRune Particles. Also, make sure to read the class 
    // documentation of the ParticleSystemNode. The documentation describes all particle 
    // parameters that are supported by DigitalRune Graphics!
    private static ParticleSystem CreateFire(ContentManager content)
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "Campfire",
        MaxNumberOfParticles = 25
      };

      // Make all computations relative to the pose (position and orientation) of the
      // particle system to allow instancing. (I.e. the particle system can be added
      // more than once to the scene.)
      ps.ReferenceFrame = ParticleReferenceFrame.Local;

      // Each particle lives for a random time span.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(0.8f, 1.2f),
      });

      // Add an effector that emits particles at a constant rate.
      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 20,
      });

      // Particle positions start on a circular area (in the xy-plane).
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new CircleDistribution { OuterRadius = 0.4f, InnerRadius = 0 }
      });

      // Set default axis of billboards. (Usually Vector3F.Up, but in this case the 
      // particle system is rotated.)
      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Axis).DefaultValue = Vector3F.Forward;

      // Particles move in forward direction with a random speed.
      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Direction).DefaultValue = Vector3F.Forward;
      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0, 1),
      });

      // The LinearVelocityEffector uses the Direction and LinearSpeed to update the Position
      // of particles.
      ps.Effectors.Add(new LinearVelocityEffector());

      // Lets apply a damping (= exponential decay) to the LinearSpeed using the SingleDampingEffector.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleDampingEffector
      {
        // Following parameters are equal to the default values. No need to set them.
        //ValueParameter = ParticleParameterNames.LinearSpeed,
        //DampingParameter = ParticleParameterNames.Damping,
      });

      // To create a wind effect, we apply an acceleration to all particles.
      ps.Parameters.AddUniform<Vector3F>("Wind").DefaultValue = new Vector3F(-1, -0.5f, -3);//new Vector3F(-1, 3, -0.5f);
      ps.Effectors.Add(new LinearAccelerationEffector { AccelerationParameter = "Wind" });

      // Each particle starts with a random rotation angle and a random angular speed.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-2f, 2f),
      });

      // The AngularVelocityEffector uses the AngularSpeed to update the particle Angle.
      ps.Effectors.Add(new AngularVelocityEffector());

      // All particle have the same size.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Size).DefaultValue = 0.8f;

      // Particle alpha fades in to 1 and then back out to 0.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        FadeInStart = 0.0f,
        FadeInEnd = 0.3f,
        FadeOutStart = 0.7f,
        FadeOutEnd = 1.0f,
      });

      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Color).DefaultValue = new Vector3F(5, 5, 5);

      // DigitalRune Graphics supports a "Texture" parameter of type Texture2D or 
      // PackedTexture. The texture "FireParticles.tga" is a tile set, which can be 
      // described using a PackedTexture.
      ps.Parameters.AddUniform<PackedTexture>(ParticleParameterNames.Texture).DefaultValue =
        new PackedTexture("FireParticles", content.Load<Texture2D>("Campfire/FireParticles"), Vector2F.Zero, Vector2F.One, 4, 1);

      // Each particle chooses a random image of the tile set when it is created.
      // The "AnimationTime" parameter selects an image:
      //   0 = start of animation = first image in tile set
      //   1 = end of animation = last image in tile set)
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AnimationTime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AnimationTime,
        Distribution = new UniformDistributionF(0, 1),  // Random value between 0 and 1.
      });

      // The fire effect uses additive blending (BlendMode = 0).
      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      // Enable soft particles.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Softness).DefaultValue = float.NaN; // NaN = automatic

      // Optional: Set a bounding shape for frustum culling. The bounding shape needs 
      // to be large enough to include all fire particles.
      ps.Shape = new TransformedShape(new GeometricObject(new BoxShape(2.5f, 2.5f, 2.5f), new Pose(new Vector3F(0, 0, -1))));

      return ps;
    }


    private static ParticleSystem CreateSmoke(ContentManager content)
    {
      ParticleSystem ps = new ParticleSystem
      {
        Name = "CampfireSmoke",
        MaxNumberOfParticles = 24,
        PreloadDuration = new TimeSpan(0, 0, 0, 2),
      };

      // Make all computations relative to the pose (position and orientation) of the
      // particle system to allow instancing. (I.e. the particle system can be added
      // more than once to the scene.)
      ps.ReferenceFrame = ParticleReferenceFrame.Local;

      // Each particle lives for a random time span.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(2.0f, 2.4f),
      });

      // Add an effector that emits particles at a constant rate.
      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 10,
      });

      // Particle positions start on a circular area (in the xy-plane).
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new CircleDistribution { OuterRadius = 0.4f, InnerRadius = 0 }
      });

      // Set default axis of billboards. (Usually Vector3F.Up, but in this case the 
      // particle system is rotated.)
      ps.Parameters.AddUniform<Vector3F>(ParticleParameterNames.Axis).DefaultValue = Vector3F.Forward;

      // Particles move in up direction with a slight random deviation with a random speed.
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = 0.15f, Direction = Vector3F.Forward },
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0, 1),
      });

      // The LinearVelocityEffector uses the Direction and LinearSpeed to update the Position
      // of particles.
      ps.Effectors.Add(new LinearVelocityEffector());

      // Lets apply a damping (= exponential decay) to the LinearSpeed using the SingleDampingEffector.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleDampingEffector
      {
        // Following parameters are equal to the default values. No need to set them.
        //ValueParameter = ParticleParameterNames.LinearSpeed,
        //DampingParameter = ParticleParameterNames.Damping,
      });

      // To create a wind effect, we apply an acceleration to all particles.
      ps.Parameters.AddUniform<Vector3F>("Wind").DefaultValue = new Vector3F(-1, -0.5f, -3);//new Vector3F(-1, 3, -0.5f);
      ps.Effectors.Add(new LinearAccelerationEffector { AccelerationParameter = "Wind" });

      // Each particle starts with a random rotation angle and a random angular speed.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.PiOver2, ConstantsF.PiOver2),
      });
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-2f, 2f),
      });

      // The AngularVelocityEffector uses the AngularSpeed to update the particle Angle.
      ps.Effectors.Add(new AngularVelocityEffector
      {
        AngleParameter = ParticleParameterNames.Angle,
        SpeedParameter = ParticleParameterNames.AngularSpeed,
      });

      // Each particle gets a random start and end size.
      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(0.5f, 0.7f),
      });
      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(1.0f, 1.4f),
      });

      // The Size is computed from linear interpolation between the StartSize and the EndSize.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      // The Color slowly changes linearly from light gray to a darker gray.
      ps.Parameters.AddUniform<Vector3F>("StartColor").DefaultValue = new Vector3F(0.1f, 0.1f, 0.1f);
      ps.Parameters.AddUniform<Vector3F>("EndColor").DefaultValue = new Vector3F(0.01f, 0.01f, 0.01f);
      ps.Parameters.AddVarying<Vector3F>(ParticleParameterNames.Color);
      ps.Effectors.Add(new Vector3FLerpEffector
      {
        ValueParameter = ParticleParameterNames.Color,
        StartParameter = "StartColor",
        EndParameter = "EndColor",
      });

      // The Alpha value is 0 for a short time, then it fades in to the TargetAlpha and finally
      // it fades out again.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.36f,
        FadeInEnd = 0.6f,
        FadeOutStart = 0.6f,
        FadeOutEnd = 1.0f,
      });

      // DigitalRune Graphics supports a "Texture" parameter of type Texture2D or 
      // PackedTexture. The texture "Smoke2.png" contains a tile set, which can be 
      // described using the PackedTexture class.
      ps.Parameters.AddUniform<PackedTexture>(ParticleParameterNames.Texture).DefaultValue =
        new PackedTexture("Smoke", content.Load<Texture2D>("Campfire/Smoke2"), Vector2F.Zero, Vector2F.One, 2, 1);

      // Each particle chooses a random image of the tile set when it is created.
      // The "AnimationTime" parameter selects an image:
      //   0 = start of animation = first image in tile set
      //   1 = end of animation = last image in tile set)
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AnimationTime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AnimationTime,
        Distribution = new UniformDistributionF(0, 1),  // Random value between 0 and 1.
      });

      // The smoke effect uses a mix of additive blending (BlendMode = 0)
      // and alpha blending (BlendMode = 1).
      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0.5f;

      // Optional: Set a bounding shape for frustum culling. The bounding shape needs 
      // to be large enough to include all smoke particles.
      ps.Shape = new TransformedShape(new GeometricObject(new BoxShape(3, 3, 4), new Pose(new Vector3F(-1, 0, -3))));

      ps.Parameters.AddUniform<int>(ParticleParameterNames.DrawOrder).DefaultValue = 1;

      return ps;
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Clean up.
      _campfire.Parent.Children.Remove(_campfire);
      _campfire.Dispose(false);
      _campfire = null;

      _fireParticles = null;
      _smokeParticles = null;
      _light = null;
      _random = null;
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      if (!IsEnabled)
        return;

      // Let the light flicker every ~0.1 seconds.
      _elapsed += (float)deltaTime.TotalSeconds;
      if (_elapsed > 0.1f)
      {
        var light = (PointLight)_light.Light;
        light.HdrScale = _random.NextFloat(16, 24);
        _elapsed = 0;
      }

      // Synchronize particle data and render data. Needs to be called once per frame!
      // (The method basically takes a snapshot of the particle system, which is then
      // rendered in the current frame.)
      _fireParticles.Synchronize(_graphicsService);
      _smokeParticles.Synchronize(_graphicsService);
    }
  }
}
