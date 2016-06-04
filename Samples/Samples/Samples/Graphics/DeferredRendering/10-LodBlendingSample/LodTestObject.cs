#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;


namespace Samples
{
  // This game objects is used to test LOD blending and to visualize LOD transitions.
  // The LodTestObject adds a cylinder to the physics simulation and two(!) models
  // to the graphics scene. The first model is the original LOD model. The second
  // model is a modified version that uses different materials per LOD: LOD0 is red,
  // LOD1 is yellow and LOD2 is green.
  // The LODs are marked using flags. The graphics screen needs to decide which of
  // models should be rendered.
  public class LodTestObject : GameObject
  {
    private readonly IServiceLocator _services;
    private RigidBody _rigidBody;
    private ModelNode _modelNode0;  // Original LOD model.
    private ModelNode _modelNode1;  // Colored LOD model.


    public LodTestObject(IServiceLocator services)
    {
      _services = services;
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      var contentManager = _services.GetInstance<ContentManager>();
      
      // A rusty barrel with multiple levels of detail (LODs).
      _rigidBody = new RigidBody(new CylinderShape(0.35f, 1));
      _modelNode0 = contentManager.Load<ModelNode>("Barrel/Barrel").Clone();
      SampleHelper.EnablePerPixelLighting(_modelNode0);

      // Mark the LOD nodes with UserFlags = 1.
      _modelNode0.GetDescendants()
                 .OfType<LodGroupNode>()
                 .SelectMany(lodGroupNode => lodGroupNode.Levels)
                 .Select(level => level.Node)
                 .ForEach(node =>
                          {
                            node.UserFlags = 1;
                          });

      // Add a second model where each LOD has a different color.
      _modelNode1 = contentManager.Load<ModelNode>("Barrel/Barrel_Colored").Clone();
      SampleHelper.EnablePerPixelLighting(_modelNode1);

      // Mark the LOD nodes with UserFlags = 2.
      _modelNode1.GetDescendants()
                 .OfType<LodGroupNode>()
                 .SelectMany(lodGroupNode => lodGroupNode.Levels)
                 .Select(level => level.Node)
                 .ForEach(node =>
                         {
                           node.UserFlags = 2;
                         });


      // Set a random pose.
      var randomPosition = new Vector3F(
        RandomHelper.Random.NextFloat(-10, 10),
        RandomHelper.Random.NextFloat(2, 5),
        RandomHelper.Random.NextFloat(-10, 0));
      _rigidBody.Pose = new Pose(randomPosition, RandomHelper.Random.NextQuaternionF());
      _modelNode0.PoseWorld = _rigidBody.Pose;
      _modelNode1.PoseWorld = _rigidBody.Pose;

      // Add rigid body to physics simulation and models to scene.
      var simulation = _services.GetInstance<Simulation>();
      simulation.RigidBodies.Add(_rigidBody);

      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(_modelNode0);
      scene.Children.Add(_modelNode1);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Remove model and rigid body.
      _modelNode0.Parent.Children.Remove(_modelNode0);
      _modelNode0.Dispose(false);

      _modelNode1.Parent.Children.Remove(_modelNode0);
      _modelNode1.Dispose(false);

      _rigidBody.Simulation.RigidBodies.Remove(_rigidBody);
      _rigidBody = null;
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Synchronize graphics <--> physics.
      _modelNode0.SetLastPose(true);
      _modelNode0.PoseWorld = _rigidBody.Pose;

      _modelNode1.SetLastPose(true);
      _modelNode1.PoseWorld = _rigidBody.Pose;
    }
  }
}
#endif