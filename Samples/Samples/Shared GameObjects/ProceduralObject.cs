using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;


namespace Samples
{
  // Creates a dynamic object (model + rigid body). The graphics model (mesh and material)
  // is created in code and not loaded with the content manager.
  public sealed class ProceduralObject : GameObject
  {
    private readonly IServiceLocator _services;
    private MeshNode _meshNode;
    private RigidBody _rigidBody;


    public ProceduralObject(IServiceLocator services)
    {
      _services = services;
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      var graphicsService = _services.GetInstance<IGraphicsService>();
      var gameObjectService = _services.GetInstance<IGameObjectService>();
      var content = _services.GetInstance<ContentManager>();

      // Check if the game object manager has another ProceduralObject instance.
      var otherProceduralObject = gameObjectService.Objects
                                                   .OfType<ProceduralObject>()
                                                   .FirstOrDefault(o => o != this);
      Mesh mesh;
      if (otherProceduralObject != null)
      {
        // This ProceduralObject is not the first. We re-use rigid body data and 
        // the mesh from the existing instance.
        var otherBody = otherProceduralObject._rigidBody;
        _rigidBody = new RigidBody(otherBody.Shape, otherBody.MassFrame, otherBody.Material);
        mesh = otherProceduralObject._meshNode.Mesh;
      }
      else
      {
        // This is the first ProceduralObject instance. 
        // Create a a new rigid body.
        var shape = new MinkowskiSumShape(new GeometricObject(new SphereShape(0.05f)), new GeometricObject(new BoxShape(0.5f, 0.5f, 0.5f)));
        _rigidBody = new RigidBody(shape);

        // Create a new mesh. See SampleHelper.CreateMesh for more details.
        mesh = SampleHelper.CreateMesh(content, graphicsService, _rigidBody.Shape);
        mesh.Name = "ProceduralObject";
      }

      // Create a scene graph node for the mesh.
      _meshNode = new MeshNode(mesh);

      // Set a random pose.
      var randomPosition = new Vector3F(
        RandomHelper.Random.NextFloat(-10, 10),
        RandomHelper.Random.NextFloat(2, 5),
        RandomHelper.Random.NextFloat(-20, 0));
      _rigidBody.Pose = new Pose(randomPosition, RandomHelper.Random.NextQuaternionF());
      _meshNode.PoseWorld = _rigidBody.Pose;

      // Add mesh node to scene graph.
      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(_meshNode);

      // Add rigid body to the physics simulation.
      var simulation = _services.GetInstance<Simulation>();
      simulation.RigidBodies.Add(_rigidBody);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      _meshNode.Parent.Children.Remove(_meshNode);
      _meshNode.Dispose(false);
      _meshNode = null;

      _rigidBody.Simulation.RigidBodies.Remove(_rigidBody);
      _rigidBody = null;
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Synchronize graphics <--> physics.
      if (_meshNode != null)
      {
        // Update SceneNode.LastPoseWorld - this is required for some effects, 
        // like object motion blur.
        _meshNode.SetLastPose(true);

        _meshNode.PoseWorld = _rigidBody.Pose;
      }
    }
  }
}
