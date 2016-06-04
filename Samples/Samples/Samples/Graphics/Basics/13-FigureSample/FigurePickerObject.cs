using System;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  // Create collision objects for the FigureNodes and a collision object
  // that is used for picking.
  class FigurePickerObject : GameObject
  {
    private readonly CameraObject _cameraObject;
    private readonly Scene _scene;
    private readonly DebugRenderer _debugRenderer;

    // A collision domain used for picking intersection tests.
    private readonly CollisionDomain _collisionDomain;

    // A collision objects which is used for picking (ray, cone, or cylinder).
    private readonly CollisionObject _pickingObject;


    public FigurePickerObject(IGraphicsService graphicsService, Scene scene, CameraObject cameraObject, DebugRenderer debugRenderer)
    {
      _cameraObject = cameraObject;
      _scene = scene;
      _debugRenderer = debugRenderer;

      // Create a collision domain which manages all collision objects used for
      // picking: the picking object and the collision objects for figure nodes.
      _collisionDomain = new CollisionDomain(new CollisionDetection());

      // Create the picking object:
      // The picking object represents the mouse cursor or the reticle. Usually 
      // a ray is used, but in this example we want to use a cylinder/cone. This 
      // allows to check which objects within a certain radius of the reticle. A 
      // picking cylinder/cone is helpful for touch devices where the picking is 
      // done with an imprecise input method like the human finger.

      // We want to pick objects in 10 pixel radius around the reticle. To determine 
      // the world space size of the required cylinder/cone, we can use the projection
      // and the viewport. 
      const float pickingRadius = 10;
      var projection = _cameraObject.CameraNode.Camera.Projection;
      var viewport = graphicsService.GraphicsDevice.Viewport;

      Shape pickingShape;
      if (projection is OrthographicProjection)
      {
        // Use cylinder for orthographic projections:
        // The cylinder is centered at the camera position and reaches from the 
        // camera position to the camera far plane. A TransformedShape is used
        // to rotate and translate the cylinder.
        float radius = projection.Width / viewport.Width * pickingRadius;
        pickingShape = new TransformedShape(
          new GeometricObject(
            new CylinderShape(radius, projection.Far),
            new Pose(new Vector3F(0, 0, -projection.Far / 2), Matrix33F.CreateRotationX(ConstantsF.PiOver2))));
      }
      else
      {
        // Use cone for perspective projections:
        // The cone tip is at the camera position and the cone base is at the 
        // camera far plane. 

        // Compute the radius at the far plane that projects to 10 pixels in screen space.
        float radius = viewport.Unproject(
          new Vector3(viewport.Width / 2.0f + pickingRadius, viewport.Height / 2.0f, 1),
          (Matrix)_cameraObject.CameraNode.Camera.Projection.ToMatrix44F(),
          Matrix.Identity,
          Matrix.Identity).X;

        // A transformed shape is used to rotate and translate the cone.
        pickingShape = new TransformedShape(
          new GeometricObject(
            new ConeShape(radius, projection.Far),
            new Pose(new Vector3F(0, 0, -projection.Far), Matrix33F.CreateRotationX(ConstantsF.PiOver2))));
      }

      // Create collision object with the picking shape.
      _pickingObject = new CollisionObject(new GeometricObject(pickingShape, _cameraObject.CameraNode.PoseWorld));
    }


    protected override void OnLoad()
    {
      // Add a collision object for each figure node.
      foreach (var figureNode in _scene.GetDescendants().OfType<FigureNode>())
      {
        var geometricObject = new FigureGeometricObject
        {
          Shape = figureNode.Figure.HitShape,
          Scale = figureNode.ScaleWorld,
          Pose = figureNode.PoseWorld,
          FigureNode = figureNode,
        };
        var collisionObject = new CollisionObject(geometricObject);
        _collisionDomain.CollisionObjects.Add(collisionObject);
      }

      _collisionDomain.CollisionObjects.Add(_pickingObject);
    }


    protected override void OnUnload()
    {
      _collisionDomain.CollisionObjects.Clear();
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Update direction of picking object.
      ((GeometricObject)_pickingObject.GeometricObject).Pose = _cameraObject.CameraNode.PoseWorld;

      // TODO: If figureNodes can move or scale, we have to copy the new Pose 
      // and Scale from the FigureNodes to their CollisionObjects.

      _collisionDomain.Update(deltaTime);

      // Reset colors of figure nodes that where "picked" in the last frame.
      // TODO: To make this faster, loop over the contact objects of the last 
      // frame and not over all nodes in the scene.
      foreach (var figureNode in _scene.GetDescendants().OfType<FigureNode>())
      {
        // Figure nodes which were picked, have the color info in the UserData. 
        if (figureNode.UserData != null)
        {
          figureNode.StrokeColor = ((Pair<Vector3F>)figureNode.UserData).First;
          figureNode.FillColor = ((Pair<Vector3F>)figureNode.UserData).Second;
          figureNode.UserData = null;
        }
      }

      // Change the color of all figure nodes which touch the picking object.
      foreach (var pickedObject in _collisionDomain.GetContactObjects(_pickingObject))
      {
        var myGeometricObject = pickedObject.GeometricObject as FigureGeometricObject;
        if (myGeometricObject != null)
        {
          var figureNode = myGeometricObject.FigureNode;
          _debugRenderer.DrawText("Picked node: " + figureNode.Name);

          // Store original color in UserData.
          figureNode.UserData = new Pair<Vector3F>(figureNode.StrokeColor, figureNode.FillColor);
          // Change color.
          figureNode.StrokeColor = new Vector3F(0.8f, 0.6f, 0.08f);
          figureNode.FillColor = new Vector3F(1, 0.7f, 0.1f);
        }
      }

      // Draw the picking object (for debugging).
      _debugRenderer.DrawObject(_pickingObject.GeometricObject, Color.Red, true, false);
    }
  }
}
