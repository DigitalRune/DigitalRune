using DigitalRune.Game;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;


namespace Samples
{
  // Loads a ground plane model and creates a static rigid body for the ground plane.
  public class GroundObject : GameObject
  {
    private readonly IServiceLocator _services;
    private ModelNode _modelNode;
    private RigidBody _rigidBody;


    public GroundObject(IServiceLocator services)
    {
      _services = services;
      Name = "Ground";
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      // Load model.
      var contentManager = _services.GetInstance<ContentManager>();
      _modelNode = contentManager.Load<ModelNode>("Ground/Ground").Clone();
      _modelNode.ScaleLocal = new Vector3F(0.5f);

      foreach (var node in _modelNode.GetSubtree())
      {
        // Disable the CastsShadows flag for ground meshes. No need to render
        // this model into the shadow map. (This also avoids any shadow acne on 
        // the ground model.)
        node.CastsShadows = false;

        // If models will never move, set the IsStatic flag. This gives the engine 
        // more room for optimizations. Additionally, some effects, like certain 
        // decals, may only affect static geometry.
        node.IsStatic = true;
      }

      // Add model node to scene graph.
      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(_modelNode);

      // Create rigid body.
      _rigidBody = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        MotionType = MotionType.Static,
      };

      // Add rigid body to the physics simulation.
      var simulation = _services.GetInstance<Simulation>();
      simulation.RigidBodies.Add(_rigidBody);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Remove model and rigid body.
      _modelNode.Parent.Children.Remove(_modelNode);
      _modelNode.Dispose(false);
      _modelNode = null;

      _rigidBody.Simulation.RigidBodies.Remove(_rigidBody);
      _rigidBody = null;
    }
  }
}
