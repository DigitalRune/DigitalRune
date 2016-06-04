using System;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using CurveLoopType = DigitalRune.Mathematics.Interpolation.CurveLoopType;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to move a kinematic body along a path.",
    "",
    4)]
  public class KinematicSample : PhysicsSample
  {
    private readonly RigidBody _kinematicBody;  // The kinematic body that moves along the path.
    private Path3F _path;                       // The 3D path.
    private float _currentPathPosition;         // A value indicating the current position on the path.
    private readonly Vector3F[] _pointList = new Vector3F[200];


    public KinematicSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",           // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Create the kinematic body. (Kinematic means that the velocity of the body is not 
      // the result of simulated forces and constraints. Instead the velocity of the body 
      // is directly controlled by the application.)
      _kinematicBody = new RigidBody(new ConeShape(0.3f, 1))
      {
        MotionType = MotionType.Kinematic
      };
      Simulation.RigidBodies.Add(_kinematicBody);

      // Create a cyclic path.
      CreatePath();

      // Add a number of random boxes.
      const int numberOfBoxes = 20;
      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < numberOfBoxes; i++)
      {
        Vector3F randomPosition = RandomHelper.Random.NextVector3F(-5, 5);
        randomPosition.Y = 5;
        QuaternionF randomOrientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(boxShape)
        {
          Pose = new Pose(randomPosition, randomOrientation),
        };
        Simulation.RigidBodies.Add(body);
      }
    }


    private void CreatePath()
    {
      // Create a cyclic path. (More information on paths can be found in the DigitalRune
      // Mathematics documentation and related samples.)
      _path = new Path3F
      {
        SmoothEnds = true,
        PreLoop = CurveLoopType.Cycle,
        PostLoop = CurveLoopType.Cycle
      };

      // The curvature of the path is defined by a number of path keys.
      _path.Add(new PathKey3F
      {
        Parameter = 0,                                  // The path parameter defines position of the path key on the curve.
        Point = new Vector3F(-4, 0.5f, -3),             // The world space position of the path key.
        Interpolation = SplineInterpolation.CatmullRom, // The type of interpolation that is used between this path key and the next.
      });
      _path.Add(new PathKey3F
      {
        Parameter = 1,
        Point = new Vector3F(-1, 0.5f, -5),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      _path.Add(new PathKey3F
      {
        Parameter = 2,
        Point = new Vector3F(3, 0.5f, -4),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      _path.Add(new PathKey3F
      {
        Parameter = 3,
        Point = new Vector3F(0, 0.5f, 0),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      _path.Add(new PathKey3F
      {
        Parameter = 4,
        Point = new Vector3F(-3, 0.5f, 3),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      _path.Add(new PathKey3F
      {
        Parameter = 5,
        Point = new Vector3F(-1, 0.5f, 5),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      _path.Add(new PathKey3F
      {
        Parameter = 6,
        Point = new Vector3F(0, 0.5f, 0),
        Interpolation = SplineInterpolation.CatmullRom,
      });

      // The last key uses the same position as the first key to create a closed path.
      PathKey3F lastKey = new PathKey3F
      {
        Parameter = _path.Count,
        Point = _path[0].Point,
        Interpolation = SplineInterpolation.CatmullRom,
      };
      _path.Add(lastKey);

      // The current path parameter goes from 0 to 7. This path parameter is not linearly
      // proportional to the path length. This is not suitable for animations.
      // To move an object with constant speed along a path, the path parameter should
      // be linearly proportional to the length of the path.
      // ParameterizeByLength() changes the path parameter so that the path parameter
      // at the each key is equal to the length of path (measured from the first key position
      // to the current key position).
      // ParameterizeByLength() uses an iterative process, we end the process after 10 
      // iterations or when the error is less than 0.01f.
      _path.ParameterizeByLength(10, 0.01f);

      // Sample the path for rendering.
      int numberOfSamples = _pointList.Length - 1;
      float pathLength = _path.Last().Parameter;
      for (int i = 0; i <= numberOfSamples; i++)
      {
        Vector3F pointOnPath = _path.GetPoint(pathLength / numberOfSamples * i);
        _pointList[i] = pointOnPath;
      }
    }


    public override void Update(GameTime gameTime)
    {
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Move an object with constant speed along the path.
      const float speed = 5;
      float newPathPosition = _currentPathPosition + deltaTime * speed;

      // Get path parameter where the path length is equal to newPathPosition.
      float parameter = _path.GetParameterFromLength(newPathPosition, 10, 0.01f);

      // Get path point at the newPathPosition.
      Vector3F position = _path.GetPoint(parameter);

      // Get the path tangent at newPathPosition and use it as the forward direction.
      Vector3F forward = _path.GetTangent(parameter).Normalized;

      QuaternionF currentOrientation = QuaternionF.CreateRotation(_kinematicBody.Pose.Orientation);
      QuaternionF targetOrientation = QuaternionF.CreateRotation(Vector3F.UnitY, forward);
      QuaternionF orientationDelta = targetOrientation * currentOrientation.Conjugated;

      // Selective Negation:
      // A certain rotation can be described by two quaternions: q and -q. For example, if you look 
      // to the north and want to look to the east you can either rotate 90 degrees clockwise or 
      // 270 degrees counter-clockwise. In the game we always want to rotate using the 
      // smaller angle. This is done using "Selective Negation": The dot product of two quaternions 
      // is proportional to the cosine of the rotation angle. If the cosine of the angle is < 0, 
      // the angle is larger than +/- 90 degrees. In this case we must use -orientationDelta to 
      // rotate using the smaller angle.
      if (QuaternionF.Dot(currentOrientation, targetOrientation) < 0)
        orientationDelta = -orientationDelta;

      // We could directly set the new position of the kinematic body. However, directly 
      // setting a position is like "teleporting" an object. The body would not interact 
      // properly with other objects in the physics simulation.
      // Instead we apply a linear and angular velocity to the body. The simulation will
      // automatically update the position of the body. If the body touches other objects
      // along the way it will push these objects with the appropriate force.

      // Note that the physics simulation may advance with a different time step than the
      // rest of the game.
      deltaTime = Math.Max(deltaTime, Simulation.Settings.Timing.FixedTimeStep);

      // Determine the linear velocity that moves the body forward.
      Vector3F linearVelocity = (position - _kinematicBody.Pose.Position) / deltaTime;

      // Determine the angular velocity that rotates the body.
      Vector3F angularVelocity;
      Vector3F rotationAxis = orientationDelta.Axis;
      if (!rotationAxis.IsNumericallyZero)
      {
        // The angular velocity is computed as rotationAxis * rotationSpeed.
        // The rotation speed is computed as angle / time. (Note: The angle is given in radians.)
        float rotationSpeed = (orientationDelta.Angle / deltaTime);
        angularVelocity = rotationAxis * rotationSpeed;
      }
      else
      {
        // The axis of rotation is 0. That means the no rotation should be applied.
        angularVelocity = Vector3F.Zero;
      }

      _kinematicBody.LinearVelocity = linearVelocity;
      _kinematicBody.AngularVelocity = angularVelocity;

      _currentPathPosition = newPathPosition;

      // Let the base class render the rigid bodies.
      base.Update(gameTime);

      // Draw the 3D path.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      for (int i = 0; i < _pointList.Length; i++)
        debugRenderer.DrawLine(_pointList[i], _pointList[(i + 1) % _pointList.Length], Color.Black, false);
    }
  }
}
