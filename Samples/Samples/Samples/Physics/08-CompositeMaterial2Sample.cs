using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Materials;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create a rigid body with multiple materials.",
    "This is a single rigid body. One box has high friction. The other box has low friction.",
    8)]
  public class CompositeMaterial2Sample : PhysicsSample
  {
    public CompositeMaterial2Sample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(new Vector3F(0, 1, 0.25f).Normalized, 0))
      {
        Name = "GroundPlane",           // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };

      // Adjust the coefficients of friction of the ground plane.
      ((UniformMaterial)groundPlane.Material).DynamicFriction = 0.5f;
      ((UniformMaterial)groundPlane.Material).StaticFriction = 0.5f;
      Simulation.RigidBodies.Add(groundPlane);

      // Prepare two materials: a slippery material and a rough material.
      UniformMaterial slipperyMaterial = new UniformMaterial
      {
        DynamicFriction = 0.001f,
        StaticFriction = 0.001f,
      };
      UniformMaterial roughMaterial = new UniformMaterial
      {
        DynamicFriction = 1,
        StaticFriction = 1,
      };

      // Create a rigid body that consists of multiple shapes: Two boxes and a cylinder between them.
      CompositeShape compositeShape = new CompositeShape();
      compositeShape.Children.Add(new GeometricObject(new BoxShape(1f, 1f, 1f), new Pose(new Vector3F(1.5f, 0f, 0f))));
      compositeShape.Children.Add(new GeometricObject(new BoxShape(1f, 1, 1f), new Pose(new Vector3F(-1.5f, 0f, 0f))));
      compositeShape.Children.Add(new GeometricObject(new CylinderShape(0.1f, 2), new Pose(Matrix33F.CreateRotationZ(ConstantsF.PiOver2))));

      // A CompositeMaterial is used to assign a different material to each shape.
      CompositeMaterial compositeMaterial = new CompositeMaterial();
      compositeMaterial.Materials.Add(roughMaterial);     // Assign the rough material to the first box.
      compositeMaterial.Materials.Add(slipperyMaterial);  // Assign the slippery material to the second box.
      compositeMaterial.Materials.Add(null);              // Use whatever is default for the handle between the boxes.

      RigidBody body = new RigidBody(compositeShape, null, compositeMaterial)
      {
        Pose = new Pose(new Vector3F(0, 2.2f, -5)),
      };
      Simulation.RigidBodies.Add(body);
    }
  }
}
