using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // Creates and controls a dynamic object (model + rigid body). 
  // Please note, the shining sphere, which drops when <4> is pressed, casts an 
  // omnidirectional shadow. Dropping multiple spheres can quickly reduce performance
  // due to the amount of shadow maps that need to be rendered. Shadows can start to 
  // flicker, if more than 8 shadow-casting light sources overlap on screen.
  public class DynamicObject : GameObject
  {
    private readonly IServiceLocator _services;
    private readonly int _type;


    public ModelNode ModelNode { get; private set; }
    public RigidBody RigidBody { get; private set; }


    public DynamicObject(IServiceLocator services, int type)
    {
      if (type < 1 || type > 7)
        throw new ArgumentOutOfRangeException("type");

      _services = services;
      _type = type;
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      var contentManager = _services.GetInstance<ContentManager>();
      
      if (_type == 1)
      {
        // A simple cube.
        RigidBody = new RigidBody(new BoxShape(1, 1, 1));
        ModelNode = contentManager.Load<ModelNode>("RustyCube/RustyCube").Clone();
      }
      else if (_type == 2)
      {
        // Another simple cube.
        RigidBody = new RigidBody(new BoxShape(1, 1, 1));
        ModelNode = contentManager.Load<ModelNode>("MetalGrateBox/MetalGrateBox").Clone();
      }
      else if (_type == 3)
      {
        // A TV-like box.
        RigidBody = new RigidBody(new BoxShape(1, 0.6f, 0.8f)) { UserData = "TV" };
        ModelNode = contentManager.Load<ModelNode>("TVBox/TVBox");

        if (ModelNode.Children.OfType<LightNode>().Count() == 0)
        {
          // This is the first time the "TVBox" is loaded.

          // Add a projector light to the model that projects the TV screen. The
          // TV screen is the emissive part of the TV mesh.
          var meshNode = ModelNode.Children.OfType<MeshNode>().First();
          var material = meshNode.Mesh.Materials.First(m => m.Name == "TestCard");

          // Get texture from material.
          // Note: In XNA the effect parameter type is Texture. In MonoGame it is Texture2D.
          Texture2D texture;
          EffectParameterBinding parameterBinding = material["Material"].ParameterBindings["EmissiveTexture"];
          if (parameterBinding is EffectParameterBinding<Texture>)
            texture = (Texture2D)((EffectParameterBinding<Texture>)parameterBinding).Value;
          else
            texture = ((EffectParameterBinding<Texture2D>)parameterBinding).Value;

          var projection = new PerspectiveProjection();
          projection.Near = 0.55f;
          projection.Far = 3.0f;
          projection.SetFieldOfView(MathHelper.ToRadians(60), 0.76f / 0.56f);

          var projectorLight = new ProjectorLight(texture, projection);
          projectorLight.Attenuation = 4;
          var projectorLightNode = new LightNode(projectorLight);
          projectorLightNode.LookAt(new Vector3F(0, 0.2f, 0), Vector3F.Zero, Vector3F.UnitZ);

          // Attach the projector light to the model.
          ModelNode.Children.Add(projectorLightNode);
        }

        ModelNode = ModelNode.Clone();
      }
      else if (_type == 4)
      {
        // A "magic" sphere with a colored point light.
        RigidBody = new RigidBody(new SphereShape(0.25f));
        ModelNode = contentManager.Load<ModelNode>("MagicSphere/MagicSphere");

        if (ModelNode.Children.OfType<LightNode>().Count() == 0)
        {
          // This is the first time the "MagicSphere" is loaded.

          // Change the size of the sphere.
          var meshNode = ModelNode.Children.OfType<MeshNode>().First();
          meshNode.ScaleLocal = new Vector3F(0.5f);

          // Disable shadows. (The sphere acts as a light source.)
          meshNode.CastsShadows = false;

          // Add a point light.
          var pointLight = new PointLight
          {
            Color = new Vector3F(1, 1, 1),
            DiffuseIntensity = 4,
            SpecularIntensity = 4,
            Range = 3,
            Attenuation = 1,
            Texture = contentManager.Load<TextureCube>("MagicSphere/ColorCube"),
          };
          var pointLightNode = new LightNode(pointLight)
          {
            // The point light uses shadow mapping to cast an omnidirectional shadow.
            Shadow = new CubeMapShadow
            {
              PreferredSize = 64,
            }
          };

          ModelNode.Children.Add(pointLightNode);
        }

        ModelNode = ModelNode.Clone();
      }
      else if (_type == 5)
      {
        // A sphere of glass (or "bubble").
        RigidBody = new RigidBody(new SphereShape(0.3f));
        ModelNode = contentManager.Load<ModelNode>("Bubble/Bubble").Clone();
        ModelNode.GetDescendants().OfType<MeshNode>().First().ScaleLocal = new Vector3F(0.3f);
      }
      else if (_type == 6)
      {
        // A rusty barrel with multiple levels of detail (LODs).
        RigidBody = new RigidBody(new CylinderShape(0.35f, 1));
        ModelNode = contentManager.Load<ModelNode>("Barrel/Barrel").Clone();
      }
      else
      {
        // A cube consisting of a frame and transparent sides.
        RigidBody = new RigidBody(new BoxShape(1, 1, 1));
        ModelNode = contentManager.Load<ModelNode>("GlassBox/GlassBox").Clone();
      }

      SampleHelper.EnablePerPixelLighting(ModelNode);

      // Set a random pose.
      var randomPosition = new Vector3F(
        RandomHelper.Random.NextFloat(-10, 10),
        RandomHelper.Random.NextFloat(2, 5),
        RandomHelper.Random.NextFloat(-20, 0));
      RigidBody.Pose = new Pose(randomPosition, RandomHelper.Random.NextQuaternionF());
      ModelNode.PoseWorld = RigidBody.Pose;

      // Add rigid body to physics simulation and model to scene.
      var simulation = _services.GetInstance<Simulation>();
      simulation.RigidBodies.Add(RigidBody);

      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(ModelNode);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Remove model and rigid body.
      ModelNode.Parent.Children.Remove(ModelNode);
      ModelNode.Dispose(false);
      ModelNode = null;

      RigidBody.Simulation.RigidBodies.Remove(RigidBody);
      RigidBody = null;
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Synchronize graphics <--> physics.
      if (ModelNode != null)
      {
        // Update SceneNode.LastPoseWorld - this is required for some effects 
        // like object motion blur. 
        ModelNode.SetLastPose(true);

        ModelNode.PoseWorld = RigidBody.Pose;
      }
    }
  }
}
