#if WP7
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Camera = DigitalRune.Graphics.Camera;
using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample was created for the Windows Phone.",
    @"This sample creates a simple 3D scene on the phone. The phone vibrates when a strong 
collision is detected.",
    100)]
  [Controls(@"Sample
  Tap ..... A tap creates a new object and drops it into the scene.
  Flick ... A vertical flick creates an explosion in the center of the scene.
  Hold .... Tap-and-hold can be used to clear the scene.
  Drag .... Objects can be moved around by simple dragging them with a finger.
  Tilt .... The viewing direction can be changed by tilting the phone.
  Pinch ... A pinch/stretch gesture can be used to zoom in or out.")]
  public class WindowsPhoneSample : BasicSample
  {
    // ----- Input processing
    private readonly GestureType _originalEnabledGestures;

    // We use a low-pass filter to smooth the accelerometer signal and reduce jitter.
    private readonly LowPassFilter _lowPassFilter;
    
    // ----- Physics
    private List<Shape> _shapes;

    // Hit-testing whether the user taps an object.
    private RayShape _rayShape;
    private CollisionObject _rayCollisionObject;

    // Dragging a rigid body using a spring.
    private BallJoint _spring;
    private float _springAnchorDistanceFromCamera;
    
    // ----- Camera
    private const float MinCameraDistance = 10.0f;
    private const float MaxCameraDistance = 200.0f;
    private float _cameraDistance = 30.0f;
    private Vector3F _cameraPosition;


    /// <summary>
    /// Gets a value indicating whether the user is currently dragging a rigid body.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if user is dragging a rigid body; otherwise, <see langword="false"/>.
    /// </value>
    private bool UserIsDraggingObject
    {
      get { return _spring != null; }
    }


    public WindowsPhoneSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;

      // Set a fixed camera.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        MathHelper.ToRadians(30),
        GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        1f,
        1000.0f);
      Vector3F cameraTarget = new Vector3F(0, 1, 0);
      Vector3F cameraPosition = new Vector3F(0, 12, 0);
      Vector3F cameraUpVector = new Vector3F(0, 0, -1);
      GraphicsScreen.CameraNode = new CameraNode(new Camera(projection))
      {
        View = Matrix44F.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector),
      };

      // We use the accelerometer to control the camera view. The accelerometer registers every 
      // little change, but we do not want a shaky camera. We can use a low-pass filter to smooth
      // the sensor signal. 
      _lowPassFilter = new LowPassFilter(new Vector3F(0, -1, 0))
      {
        TimeConstant = 0.15f, // Let's try a time constant of 0.15 seconds.
        // When increasing the time constant the camera becomes more stable,
        // but also slower.
      };

      // Enable touch gestures
      _originalEnabledGestures = TouchPanel.EnabledGestures;
      TouchPanel.EnabledGestures =
        GestureType.Tap           // Tap is used to drop new bodies.
        | GestureType.Flick       // Flick creates an explosion.
        | GestureType.Hold        // Hold to clear the scene.
        | GestureType.Pinch;      // Pinch can be used to zoom in or out.

      InitializePhysics();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Restore touch panel settings.
        TouchPanel.EnabledGestures = _originalEnabledGestures;
      }

      base.Dispose(disposing);
    }


    /// <summary>
    /// Initializes the physics simulation.
    /// </summary>
    private void InitializePhysics()
    {
      // Define the timing of the physics simulation:
      // The Windows Phone runs with 30 FPS, so one frame takes 1/30 seconds.
      // We can run the simulation using the same timing.
      // However, 1/30 seconds is a very large time step for a physics simulation. A large
      // time step improves performance, but reduces the quality of the simulation.
      // Instead we can run the simulation with a time step of 1/60 seconds and compute 
      // 2 time steps per frame.
      // Additionally, we can reduce the number of constraint solver iterations to gain additional
      // performance. But if you want to simulate complex scene with stable stacks or walls, you
      // might want to leave the default value.
      // Below are two variants - you can comment/uncomment the variants to compare them.

      // -----  Variant #1: "Fast, less stable"
      Simulation.Settings.Timing.FixedTimeStep = 1.0f / 30.0f;
      Simulation.Settings.Timing.MaxNumberOfSteps = 1;
      Simulation.Settings.Constraints.NumberOfConstraintIterations = 6;

      // ----- Variant #2: "Slower, more stable"
      //_simulation.Settings.Timing.FixedTimeStep = 1.0f / 60.0f;
      //_simulation.Settings.Timing.MaxNumberOfSteps = 2;
      //_simulation.Settings.Constraints.NumberOfConstraintIterations = 10;
      
      // Add the typical force effect for gravity and damping.
      Simulation.ForceEffects.Add(new Gravity { Acceleration = new Vector3F(0, -10, 0) });
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane (static object).
      var groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Create a set of predefined shapes. 
      // The shapes are chosen randomly when a new rigid body is added.
      _shapes = new List<Shape>();
      _shapes.Add(new BoxShape(1.0f, 1.0f, 1.0f));
      _shapes.Add(new BoxShape(1.5f, 0.5f, 1.0f));
      _shapes.Add(new BoxShape(1.0f, 0.75f, 1.0f));
      _shapes.Add(new CapsuleShape(0.4f, 2.0f));
      _shapes.Add(new SphereShape(0.6f));

      // Add convex shape.
      var randomPoints = new List<Vector3F>();
      for (int i = 0; i < 20; i++)
        randomPoints.Add(RandomHelper.Random.NextVector3F(-1, 1));
      _shapes.Add(new ConvexPolyhedron(randomPoints));

      // Add a composite shape looking like table.
      var board = new BoxShape(2.0f, 0.3f, 1.2f);
      var leg = new BoxShape(0.2f, 1f, 0.2f);
      var table = new CompositeShape();
      table.Children.Add(new GeometricObject(board, new Pose(new Vector3F(0.0f, 1.0f, 0.0f))));
      table.Children.Add(new GeometricObject(leg, new Pose(new Vector3F(-0.7f, 0.5f, -0.4f))));
      table.Children.Add(new GeometricObject(leg, new Pose(new Vector3F(-0.7f, 0.5f, 0.4f))));
      table.Children.Add(new GeometricObject(leg, new Pose(new Vector3F(0.7f, 0.5f, 0.4f))));
      table.Children.Add(new GeometricObject(leg, new Pose(new Vector3F(0.7f, 0.5f, -0.4f))));
      _shapes.Add(table);
    }


    public override void Update(GameTime gameTime)
    {
      TimeSpan deltaTime = gameTime.ElapsedGameTime;

      // The user can drag objects using touch.
      DragBodies();

      // Check touch gestures.
      foreach (var gesture in InputService.Gestures)
      {
        if (!UserIsDraggingObject)
        {
          switch (gesture.GestureType)
          {
            case GestureType.Tap: // Drop body.
              AddRigidBody();
              break;
            case GestureType.Flick: // Create explosion.
              if (Math.Abs(gesture.Delta.Y) > Math.Abs(gesture.Delta.X))  // only for vertical flicks
                Explode();
              break;
            case GestureType.Hold: // Remove all rigid bodies.
              ClearScene();
              break;
            case GestureType.Pinch: // Zoom camera in/out.
              ZoomCamera(gesture);
              break;
          }
        }
      }

      // Every collision causes a vibration of the phone device.
      ApplyForceFeedback();

      // By tilting the phone we can change the view of the camera.
      TiltCamera(deltaTime);

      // ----- Draw rigid bodies using the DebugRenderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      foreach (var body in Simulation.RigidBodies)
      {
        var color = Color.Gray;
        // Draw static with different colors.
        if (body.MotionType == MotionType.Static)
          color = Color.LightGray;

        debugRenderer.DrawObject(body, color, false, false);
      }
    }


    /// <summary>
    /// Allows the user to drag a rigid body by using touch.
    /// </summary>
    private void DragBodies()
    {
      // Here is how it works:
      // We first make a hit-test using a ray to check whether the user touches a rigid body.
      // If there is a hit we create a spring (using a ball-socket joint) and connect the rigid
      // body to the touch location. Every time the user moves her finger we update the position
      // of the spring and the spring pulls the rigid body towards the finger.

      // We use raw touch points to select and drag a rigid body.
      TouchCollection touches = InputService.TouchCollection;
      if (touches.Count == 0)
      {
        // No touches detected.
        if (_spring != null)
        {
          // There is an active spring, so the user is currently dragging a rigid body.
          // Release the body by removing the spring.
          _spring.Simulation.Constraints.Remove(_spring);
          _spring = null;
        }
      }
      else
      {
        // Touch detected.
        TouchLocation touchLocation = touches[0];

        // Convert the touch location from screen coordinates to world coordinates.
        var cameraNode = GraphicsScreen.CameraNode;
        Vector3 pScreen = new Vector3(touchLocation.Position.X, touchLocation.Position.Y, 0);
        Vector3 pWorld = GraphicsService.GraphicsDevice.Viewport.Unproject(
          pScreen, 
          cameraNode.Camera.Projection,
          (Matrix)cameraNode.View,
          Matrix.Identity);

        // pWorld is point on the near clip plane of the camera.
        // Set the origin and direction of the ray for hit-testing.
        Vector3F rayOrigin = _cameraPosition;
        Vector3F rayDirection = ((Vector3F)pWorld - _cameraPosition).Normalized;

        if (touchLocation.State == TouchLocationState.Pressed)
        {
          // Let's create a ray and see if we hit a rigid body.
          // (Create the ray shape and the required collision object only once.)
          if (_rayShape == null)
          {
            _rayShape = new RayShape { StopsAtFirstHit = true };
            _rayCollisionObject = new CollisionObject(new GeometricObject(_rayShape));
          }

          // Set the origin and direction of the ray.
          _rayShape.Origin = rayOrigin;
          _rayShape.Direction = rayDirection.Normalized;

          // Make a hit test using the collision detection and get the first contact found.
          var contactSet = Simulation.CollisionDomain
                                     .GetContacts(_rayCollisionObject)
                                     .FirstOrDefault();
          if (contactSet != null && contactSet.Count > 0)
          {
            // Get the point where the ray hits the rigid body.
            Contact contact = contactSet[0];

            // The contact sets contains two objects ("ObjectA" and "ObjectB"). 
            // One is the ray the other is the object that was hit by the ray.
            var hitCollisionObject = (contactSet.ObjectA == _rayCollisionObject)
                                       ? contactSet.ObjectB
                                       : contactSet.ObjectA;

            // Check whether the object is a dynamic rigid body.
            var hitBody = hitCollisionObject.GeometricObject as RigidBody;
            if (hitBody != null && hitBody.MotionType == MotionType.Dynamic)
            {
              // Remove the old joint, in case a rigid body is already grabbed.
              if (_spring != null && _spring.Simulation != null)
                _spring.Simulation.Constraints.Remove(_spring);

              // The penetration depth tells us the distance from the ray origin to the rigid body
              // in view direction.
              _springAnchorDistanceFromCamera = contact.PenetrationDepth;

              // Get the position where the ray hits the other object.
              // (The position is defined in the local space of the object.)
              Vector3F hitPositionLocal = (contactSet.ObjectA == _rayCollisionObject)
                                            ? contact.PositionBLocal
                                            : contact.PositionALocal;

              // Attach the rigid body at the touch location using a ball-socket joint.
              // (Note: We could also use a FixedJoint, if we don't want any rotations.)
              _spring = new BallJoint
              {
                BodyA = hitBody,
                AnchorPositionALocal = hitPositionLocal,

                // We need to attach the grabbed object to a second body. In this case we just want to 
                // anchor the object at a specific point in the world. To achieve this we can use the 
                // special rigid body "World", which is defined in the simulation.
                BodyB = Simulation.World,
                AnchorPositionBLocal = rayOrigin + rayDirection * _springAnchorDistanceFromCamera,

                // Some constraint adjustments.
                ErrorReduction = 0.3f,

                // We set a softness > 0. This makes the joint "soft" and it will act like 
                // damped spring. 
                Softness = 0.00001f,

                // We limit the maximal force. This reduces the ability of this joint to violate
                // other constraints. 
                MaxForce = 1e6f
              };

              // Add the spring to the simulation.
              Simulation.Constraints.Add(_spring);
            }
          }
        }
        else if (touchLocation.State == TouchLocationState.Moved)
        {
          if (_spring != null)
          {
            // User has grabbed something.

            // Update the position of the object by updating the anchor position of the ball-socket
            // joint.
            _spring.AnchorPositionBLocal = rayOrigin + rayDirection * _springAnchorDistanceFromCamera;

            // Reduce the angular velocity by a certain factor. (This acts like a damping because we
            // do not want the object to rotate like crazy.)
            _spring.BodyA.AngularVelocity *= 0.9f;
          }
        }
      }
    }


    /// <summary>
    /// Drops a new rigid body into the scene.
    /// </summary>
    private void AddRigidBody()
    {
      int index = RandomHelper.Random.Next(_shapes.Count);
      Shape shape = _shapes[index];
      RigidBody body = new RigidBody(shape)
      {
        Pose = new Pose(new Vector3F(0, 10, 0))
      };
      Simulation.RigidBodies.Add(body);
    }


    /// <summary>
    /// Removes all dynamic rigid bodies from the simulation.
    /// </summary>
    private void ClearScene()
    {
      // Remove all rigid bodies, except static bodies.
      var staticBodies = Simulation.RigidBodies
                                   .Where(body => body.MotionType == MotionType.Static)
                                   .ToArray();

      Simulation.RigidBodies.Clear();
      Simulation.RigidBodies.AddRange(staticBodies);

      Simulation.Constraints.Clear();
      _spring = null;
    }


    /// <summary>
    /// Creates an explosion in near the center of the scene.
    /// </summary>
    private void Explode()
    {
      Explosion explosion = new Explosion
      {
        Force = 5e5f,
        Position = RandomHelper.Random.NextVector3F(-2, 2),
        Radius = 12
      };
      Simulation.ForceEffects.Add(explosion);
    }


    /// <summary>
    /// Lets the Windows Phone vibrate if a collision between rigid bodies is detected.
    /// </summary>
    private void ApplyForceFeedback()
    {
      // Check all contacts between rigid bodies.
      foreach (var contactConstraint in Simulation.ContactConstraints)
      {
        // Check whether we have a new contact that has just been created. 
        if (contactConstraint.Contact.Lifetime == 0.0f)
        {
          // We only want to vibrate the phone if the collision has a certain impact strength.
          // (In a physics simulation when one body is resting on another there can be several
          // 'micro-collisions'. We want to ignore those.)
          if (contactConstraint.LinearConstraintImpulse.Length > 2000.0f)
          {
            // Impact detected.
            VibrateController.Default.Start(TimeSpan.FromMilliseconds(10));
          }
        }
      }
    }


    /// <summary>
    /// Zooms the camera when Pinch/Stretch is detected.
    /// </summary>
    /// <param name="pinchGesture">The pinch gesture.</param>
    private void ZoomCamera(GestureSample pinchGesture)
    {
      // Get the current and the previous location of the two fingers.
      Vector2 p1New = pinchGesture.Position;
      Vector2 p1Old = pinchGesture.Position - pinchGesture.Delta;
      Vector2 p2New = pinchGesture.Position2;
      Vector2 p2Old = pinchGesture.Position2 - pinchGesture.Delta2;

      // Get the distance between the current and the previous locations.
      float dNew = Vector2.Distance(p1New, p2New);
      float dOld = Vector2.Distance(p1Old, p2Old);

      // Use the ratio between old and new distance to scale the camera distance.
      float scale = dOld / dNew;
      if (!Numeric.IsNaN(scale))
      {
        _cameraDistance *= scale;
        _cameraDistance = MathHelper.Clamp(_cameraDistance, MinCameraDistance, MaxCameraDistance);
      }
    }


    /// <summary>
    /// Changes the camera based on the tilt of the Windows Phone.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last call of <see cref="Update"/>.</param>
    private void TiltCamera(TimeSpan deltaTime)
    {
      // (Note: We use DigitalRune Mathematics instead of the XNA math types - mainly because 
      // DigitalRune Mathematics provides a 3x3 matrix to describe rotations and the QuaternionF 
      // provides a nice helper function to create a rotation from two given vectors.
      // 
      // Please note that DigitalRune Mathematics uses column vectors whereas XNA uses row vectors.
      // When using Vector3F and Matrix33F we need to multiple them in this order: v' = M * v.
      // When using Vector3 and Matrix we need to multiple them in this order: v' = v * M.)

      // Get accelerometer value transformed into world space.
      Vector3F accelerometerVector = new Vector3F(
        -InputService.AccelerometerValue.Y,
        InputService.AccelerometerValue.Z,
        -InputService.AccelerometerValue.X);

      // Run the accelerometer signal through a low-pass filter to remove noise and jitter.
      Vector3F currentGravityDirection = _lowPassFilter.Filter(accelerometerVector, (float)deltaTime.TotalSeconds);

      Matrix33F cameraTilt;
      if (!currentGravityDirection.IsNumericallyZero)
      {
        // We have some valid sensor readings.
        // Let's compute the tilt of the camera. When the phone is lying flat on a table the camera
        // looks down onto the scene. When the phone is tilted we want to rotate the position of
        // the camera. QuaternionF contains a useful helper function that creates a rotation from
        // two given directions - exactly what we need here.
        cameraTilt = QuaternionF.CreateRotation(currentGravityDirection, new Vector3F(0, -1, 0)).ToRotationMatrix33();
      }
      else
      {
        // Current acceleration is nearly (0, 0, 0). We cannot infer any useful direction from
        // this vector. Reset the camera tilt.
        cameraTilt = Matrix33F.Identity;
      }

      // ----- Set up the view matrix (= the position and orientation of the camera).
      Vector3F cameraTarget = new Vector3F(0, 2, 0);                  // That's were the camera is looking at.
      Vector3F cameraPosition = new Vector3F(0, _cameraDistance, 0);  // That's the default position of the camera.
      Vector3F cameraUpVector = new Vector3F(0, 0, -1);               // That's the up-vector of the camera.

      // Apply the camera tilt to the position and orientation.
      cameraPosition = cameraTilt * cameraPosition;
      cameraUpVector = cameraTilt * cameraUpVector;

      // Keep the camera above the ground.
      cameraPosition.Y = Math.Max(0.5f, cameraPosition.Y);

      // Create the view matrix from these points.
      GraphicsScreen.CameraNode.View = Matrix44F.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);

      // Save the camera position because it is required for hit-testing in DragBodies().
      _cameraPosition = cameraPosition;
    }
  }
}
#endif