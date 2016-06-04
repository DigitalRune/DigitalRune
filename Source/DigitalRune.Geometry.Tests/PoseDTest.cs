using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using NUnit.Framework;


namespace DigitalRune.Geometry.Tests
{

  [TestFixture]
  public class PoseDTest
  {
    [Test]
    public void Test1()
    {
      PoseD p = PoseD.Identity;

      Assert.AreEqual(Matrix44D.Identity, p.ToMatrix44D());
      Assert.AreEqual(Matrix33D.Identity, p.Orientation);
      Assert.AreEqual(Vector3D.Zero, p.Position);

      p.Position = new Vector3D(1, 2, 3);

      p.Orientation = Matrix33D.CreateRotation(new Vector3D(3, -4, 9), 0.49);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToWorldDirection(Vector3D.UnitX), 0), p * new Vector4D(1, 0, 0, 0)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToWorldDirection(Vector3D.UnitY), 0), p * new Vector4D(0, 1, 0, 0)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToWorldDirection(Vector3D.UnitZ), 0), p * new Vector4D(0, 0, 1, 0)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToWorldPosition(Vector3D.UnitX), 1), p * new Vector4D(1, 0, 0, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToWorldPosition(Vector3D.UnitY), 1), p * new Vector4D(0, 1, 0, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToWorldPosition(Vector3D.UnitZ), 1), p * new Vector4D(0, 0, 1, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToLocalDirection(Vector3D.UnitX), 0), p.Inverse * new Vector4D(1, 0, 0, 0)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToLocalDirection(Vector3D.UnitY), 0), p.Inverse * new Vector4D(0, 1, 0, 0)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToLocalDirection(Vector3D.UnitZ), 0), p.Inverse * new Vector4D(0, 0, 1, 0)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToLocalPosition(Vector3D.UnitX), 1), p.Inverse * new Vector4D(1, 0, 0, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToLocalPosition(Vector3D.UnitY), 1), p.Inverse * new Vector4D(0, 1, 0, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(p.ToLocalPosition(Vector3D.UnitZ), 1), p.Inverse * new Vector4D(0, 0, 1, 1)));

      PoseD p2 = PoseD.FromMatrix(new Matrix44D(p.Orientation, Vector3D.Zero));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(p.Orientation, p2.Orientation));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(p2.Position, Vector3D.Zero));

      Matrix44D m = p2;
      m.SetColumn(3, new Vector4D(p.Position, 1));
      p2 = PoseD.FromMatrix(m);
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(p.Orientation, p2.Orientation));
      Assert.AreEqual(p.Position, p2.Position);
      //Assert.IsTrue(Vector3D.AreNumericallyEqual(p.Position, p2.Position));

      // Test other constructors.
      Assert.AreEqual(Vector3D.Zero, new PoseD(QuaternionD.CreateRotationX(0.3)).Position);
      Assert.AreEqual(Matrix33D.CreateRotationX(0.3), new PoseD(Matrix33D.CreateRotationX(0.3)).Orientation);
      Assert.AreEqual(new Vector3D(1, 2, 3), new PoseD(new Vector3D(1, 2, 3)).Position);
      Assert.AreEqual(Matrix33D.Identity, new PoseD(new Vector3D(1, 2, 3)).Orientation);
    }


    [Test]
    public void IsValid()
    {
      Matrix44D inValidPose = new Matrix44D(new double[,] { {1, 2, 3, 0},
                                                            {4, 5, 6, 0},
                                                            {7, 8, 9, 0},
                                                            {0, 0, 0, 1},
                                                          });

      Assert.IsFalse(PoseD.IsValid(inValidPose));

      Assert.IsTrue(PoseD.IsValid(Matrix44D.CreateRotationZ(0.3)));


      inValidPose = new Matrix44D(new double[,] { {1, 0, 0, 0},
                                                  {0, 1, 0, 0},
                                                  {0, 0, 1, 0},
                                                  {0, 1, 0, 1},
                                                });
      Assert.IsFalse(PoseD.IsValid(inValidPose));

      inValidPose = new Matrix44D(new double[,] { {1, 0, 0, 0},
                                                  {0, 1, 0, 0},
                                                  {0, 0, -1, 0},
                                                  {0, 1, 0, 1},
                                                });
      Assert.IsFalse(PoseD.IsValid(inValidPose));
    }


    [Test]
    public void Equals()
    {
      PoseD p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      PoseD p2 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));

      Assert.AreEqual(p1, p2);
      Assert.IsTrue(p1.Equals((object)p2));
      Assert.IsTrue(p1.Equals(p2));
      Assert.IsFalse(p1.Equals(p2.ToMatrix44D()));
    }


    [Test]
    public void GetHashCodeTest()
    {
      PoseD p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      PoseD p2 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));

      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());

      p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      p2 = new PoseD(new Vector3D(2, 1, 3), QuaternionD.CreateRotationY(0.3));
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());

      // Too bad two rotation matrices that differ only by the sign of the angle
      // (+/- angle with same axis) have the same hashcodes. See KB -> .NET --> GetHashCode
      //p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      //p2 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(-0.3));
      //Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
    }


    [Test]
    public void Multiply()
    {
      PoseD p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      PoseD p2 = new PoseD(new Vector3D(-4, 5, -6), QuaternionD.CreateRotationZ(-0.1));

      Assert.IsTrue(Vector4D.AreNumericallyEqual(
                    p1.ToMatrix44D() * p2.ToMatrix44D() * new Vector4D(1, 2, 3, 1),
                    PoseD.Multiply(PoseD.Multiply(p1, p2), new Vector4D(1, 2, 3, 1))));
    }


    [Test]
    public void MultiplyOperator()
    {
      PoseD p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      PoseD p2 = new PoseD(new Vector3D(-4, 5, -6), QuaternionD.CreateRotationZ(-0.1));

      Assert.IsTrue(Vector4D.AreNumericallyEqual(
                    p1.ToMatrix44D() * p2.ToMatrix44D() * new Vector4D(1, 2, 3, 1),
                    p1 * p2 * new Vector4D(1, 2, 3, 1)));
    }


    [Test]
    public void Interpolate()
    {
      PoseD p1 = new PoseD(new Vector3D(1, 2, 3), QuaternionD.CreateRotationY(0.3));
      PoseD p2 = new PoseD(new Vector3D(-4, 5, -6), QuaternionD.CreateRotationZ(-0.1));

      Assert.IsTrue(Vector3D.AreNumericallyEqual(p1.Position, PoseD.Interpolate(p1, p2, 0).Position));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(p1.Orientation, PoseD.Interpolate(p1, p2, 0).Orientation));

      Assert.IsTrue(Vector3D.AreNumericallyEqual(p2.Position, PoseD.Interpolate(p1, p2, 1).Position));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(p2.Orientation, PoseD.Interpolate(p1, p2, 1).Orientation));

      Assert.IsTrue(Vector3D.AreNumericallyEqual(InterpolationHelper.Lerp(p1.Position, p2.Position, 0.3), PoseD.Interpolate(p1, p2, 0.3).Position));
      Assert.IsTrue(
        QuaternionD.AreNumericallyEqual(
          InterpolationHelper.Lerp(QuaternionD.CreateRotation(p1.Orientation), QuaternionD.CreateRotation(p2.Orientation), 0.3),
          QuaternionD.CreateRotation(PoseD.Interpolate(p1, p2, 0.3).Orientation)));
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new PoseD(new Vector3D(1, 2, 3), QuaternionD.Identity).ToString()
        .StartsWith("PoseD { Position = (1; 2; 3), Orientation = (1; 0; "));
    }


    [Test]
    public void AreNumericallyEqual()
    {
      var a = new PoseD(new Vector3D(1, 2, 3), new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9));
      var b = a;

      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a));

      b = AddToAllComponents(a, Numeric.EpsilonD / 10);
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));

      b = AddToAllComponents(a, Numeric.EpsilonD * 10);
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD * 100));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD * 100));

      b = a;
      b.Position.X -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Position.Y -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Position.Z -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M00 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M01 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M02 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M10 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M11 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M12 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M20 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M21 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));

      b = a;
      b.Orientation.M22 -= 0.0001;
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(a, b, Numeric.EpsilonD));
      Assert.AreEqual(false, PoseD.AreNumericallyEqual(b, a, Numeric.EpsilonD));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(a, b, 0.001));
      Assert.AreEqual(true, PoseD.AreNumericallyEqual(b, a, 0.001));
    }


    private PoseD AddToAllComponents(PoseD pose, double epsilon)
    {
      var result = pose;
      result.Position.X += epsilon;
      result.Position.Y += epsilon;
      result.Position.Z += epsilon;

      result.Orientation.M00 += epsilon;
      result.Orientation.M01 += epsilon;
      result.Orientation.M02 += epsilon;
      result.Orientation.M10 += epsilon;
      result.Orientation.M11 += epsilon;
      result.Orientation.M12 += epsilon;
      result.Orientation.M20 += epsilon;
      result.Orientation.M21 += epsilon;
      result.Orientation.M22 += epsilon;

      return result;
    }


    [Test]
    public void ToPose()
    {
      PoseD poseD = new PoseD(new Vector3D(1, 2, 3), new Matrix33D(4, 5, 6, 7, 8, 9, 10, 11, 12));
      Pose pose = new Pose(new Vector3F(1, 2, 3), new Matrix33F(4, 5, 6, 7, 8, 9, 10, 11, 12));

      Assert.AreEqual(pose, (Pose)poseD);
      Assert.AreEqual(pose, poseD.ToPose());
    }


    [Test]
    public void SerializationXml()
    {
      PoseD pose1 = new PoseD(new Vector3D(1, 2, 3), new Matrix33D(4, 5, 6, 7, 8, 9, 10, 11, 12));
      PoseD pose2;

      string fileName = "SerializationPoseD.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(PoseD));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, pose1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        pose2 = (PoseD)serializer.ReadObject(reader);

      Assert.AreEqual(pose1, pose2);
    }


    [Test]
    public void SerializationJson()
    {
      PoseD pose1 = new PoseD(new Vector3D(1, 2, 3), new Matrix33D(4, 5, 6, 7, 8, 9, 10, 11, 12));
      PoseD pose2;

      string fileName = "SerializationPoseD.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(PoseD));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, pose1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        pose2 = (PoseD)serializer.ReadObject(stream);

      Assert.AreEqual(pose1, pose2);
    }
  }
}
