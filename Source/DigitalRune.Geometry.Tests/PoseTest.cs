using System.IO;
using System.Linq.Expressions;
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
  public class PoseTest
  {
    [Test]
    public void Test1()
    {
      Pose p = Pose.Identity;

      Assert.AreEqual(Matrix44F.Identity, p.ToMatrix44F());
      Assert.AreEqual(Matrix33F.Identity, p.Orientation);
      Assert.AreEqual(Vector3F.Zero, p.Position);

      p.Position = new Vector3F(1, 2, 3);

      p.Orientation = Matrix33F.CreateRotation(new Vector3F(3, -4, 9), 0.49f);
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToWorldDirection(Vector3F.UnitX), 0), p * new Vector4F(1, 0, 0, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToWorldDirection(Vector3F.UnitY), 0), p * new Vector4F(0, 1, 0, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToWorldDirection(Vector3F.UnitZ), 0), p * new Vector4F(0, 0, 1, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToWorldPosition(Vector3F.UnitX), 1), p * new Vector4F(1, 0, 0, 1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToWorldPosition(Vector3F.UnitY), 1), p * new Vector4F(0, 1, 0, 1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToWorldPosition(Vector3F.UnitZ), 1), p * new Vector4F(0, 0, 1, 1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToLocalDirection(Vector3F.UnitX), 0), p.Inverse * new Vector4F(1, 0, 0, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToLocalDirection(Vector3F.UnitY), 0), p.Inverse * new Vector4F(0, 1, 0, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToLocalDirection(Vector3F.UnitZ), 0), p.Inverse * new Vector4F(0, 0, 1, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToLocalPosition(Vector3F.UnitX), 1), p.Inverse * new Vector4F(1, 0, 0, 1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToLocalPosition(Vector3F.UnitY), 1), p.Inverse * new Vector4F(0, 1, 0, 1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(new Vector4F(p.ToLocalPosition(Vector3F.UnitZ), 1), p.Inverse * new Vector4F(0, 0, 1, 1)));

      Pose p2 = Pose.FromMatrix(new Matrix44F(p.Orientation, Vector3F.Zero));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(p.Orientation, p2.Orientation));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(p2.Position, Vector3F.Zero));

      Matrix44F m = p2;
      m.SetColumn(3, new Vector4F(p.Position, 1));
      p2 = Pose.FromMatrix(m);
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(p.Orientation, p2.Orientation));
      Assert.AreEqual(p.Position, p2.Position);
      //Assert.IsTrue(Vector3F.AreNumericallyEqual(p.Position, p2.Position));

      // Test other constructors.
      Assert.AreEqual(Vector3F.Zero, new Pose(QuaternionF.CreateRotationX(0.3f)).Position);
      Assert.AreEqual(Matrix33F.CreateRotationX(0.3f), new Pose(Matrix33F.CreateRotationX(0.3f)).Orientation);
      Assert.AreEqual(new Vector3F(1, 2, 3), new Pose(new Vector3F(1, 2, 3)).Position);
      Assert.AreEqual(Matrix33F.Identity, new Pose(new Vector3F(1, 2, 3)).Orientation);
    }


    [Test]
    public void IsValid()
    {
      Matrix44F inValidPose = new Matrix44F(new float[,] { {1, 2, 3, 0},
                                                           {4, 5, 6, 0},
                                                           {7, 8, 9, 0},
                                                           {0, 0, 0, 1},
                                                         });

      Assert.IsFalse(Pose.IsValid(inValidPose));

      Assert.IsTrue(Pose.IsValid(Matrix44F.CreateRotationZ(0.3f)));


      inValidPose = new Matrix44F(new float[,] { {1, 0, 0, 0},
                                                 {0, 1, 0, 0},
                                                 {0, 0, 1, 0},
                                                 {0, 1, 0, 1},
                                                });
      Assert.IsFalse(Pose.IsValid(inValidPose));

      inValidPose = new Matrix44F(new float[,] { {1, 0, 0, 0},
                                                 {0, 1, 0, 0},
                                                 {0, 0, -1, 0},
                                                 {0, 1, 0, 1},
                                                });
      Assert.IsFalse(Pose.IsValid(inValidPose));
    }


    [Test]
    public void Equals()
    {
      Pose p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      Pose p2 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));

      Assert.AreEqual(p1, p2);
      Assert.IsTrue(p1.Equals((object)p2));
      Assert.IsTrue(p1.Equals(p2));
      Assert.IsFalse(p1.Equals(p2.ToMatrix44F()));
    }


    [Test]
    public void GetHashCodeTest()
    {
      Pose p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      Pose p2 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));

      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());

      p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      p2 = new Pose(new Vector3F(2, 1, 3), QuaternionF.CreateRotationY(0.3f));
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());

      // Too bad two rotation matrices that differ only by the sign of the angle
      // (+/- angle with same axis) have the same hashcodes. See KB -> .NET --> GetHashCode
      //p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      //p2 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(-0.3f));
      //Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
    }


    [Test]
    public void Multiply()
    {
      Pose p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      Pose p2 = new Pose(new Vector3F(-4, 5, -6), QuaternionF.CreateRotationZ(-0.1f));

      Assert.IsTrue(Vector4F.AreNumericallyEqual(
                      p1.ToMatrix44F() * p2.ToMatrix44F() * new Vector4F(1, 2, 3, 1),
                      Pose.Multiply(Pose.Multiply(p1, p2), new Vector4F(1, 2, 3, 1))));
    }


    [Test]
    public void MultiplyOperator()
    {
      Pose p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      Pose p2 = new Pose(new Vector3F(-4, 5, -6), QuaternionF.CreateRotationZ(-0.1f));

      Assert.IsTrue(Vector4F.AreNumericallyEqual(
                      p1.ToMatrix44F() * p2.ToMatrix44F() * new Vector4F(1, 2, 3, 1),
                      p1 * p2 * new Vector4F(1, 2, 3, 1)));
    }


    [Test]
    public void Interpolate()
    {
      Pose p1 = new Pose(new Vector3F(1, 2, 3), QuaternionF.CreateRotationY(0.3f));
      Pose p2 = new Pose(new Vector3F(-4, 5, -6), QuaternionF.CreateRotationZ(-0.1f));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(p1.Position, Pose.Interpolate(p1, p2, 0).Position));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(p1.Orientation, Pose.Interpolate(p1, p2, 0).Orientation));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(p2.Position, Pose.Interpolate(p1, p2, 1).Position));
      Assert.IsTrue(Matrix33F.AreNumericallyEqual(p2.Orientation, Pose.Interpolate(p1, p2, 1).Orientation));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(InterpolationHelper.Lerp(p1.Position, p2.Position, 0.3f), Pose.Interpolate(p1, p2, 0.3f).Position));
      Assert.IsTrue(
        QuaternionF.AreNumericallyEqual(
          InterpolationHelper.Lerp(QuaternionF.CreateRotation(p1.Orientation), QuaternionF.CreateRotation(p2.Orientation), 0.3f),
          QuaternionF.CreateRotation(Pose.Interpolate(p1, p2, 0.3f).Orientation)));
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new Pose(new Vector3F(1, 2, 3), QuaternionF.Identity).ToString()
        .StartsWith("Pose { Position = (1; 2; 3), Orientation = (1; 0; "));
    }


    [Test]
    public void AreNumericallyEqual()
    {
      var a = new Pose(new Vector3F(1, 2, 3), new Matrix33F(1, 2, 3, 4, 5, 6, 7, 8, 9));
      var b = a;

      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a));

      b = AddToAllComponents(a, Numeric.EpsilonF / 10);
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));

      b = AddToAllComponents(a, Numeric.EpsilonF * 10);
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF * 100));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF * 100));

      b = a;
      b.Position.X -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Position.Y -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Position.Z -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M00 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M01 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M02 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M10 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M11 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M12 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M20 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M21 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));

      b = a;
      b.Orientation.M22 -= 0.001f;
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(a, b, Numeric.EpsilonF));
      Assert.AreEqual(false, Pose.AreNumericallyEqual(b, a, Numeric.EpsilonF));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(a, b, 0.01f));
      Assert.AreEqual(true, Pose.AreNumericallyEqual(b, a, 0.01f));
    }


    private Pose AddToAllComponents(Pose pose, float epsilon)
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
    public void ToPoseD()
    {
      Pose pose = new Pose(new Vector3F(1, 2, 3), new Matrix33F(4, 5, 6, 7, 8, 9, 10, 11, 12));
      PoseD poseD = new PoseD(new Vector3D(1, 2, 3), new Matrix33D(4, 5, 6, 7, 8, 9, 10, 11, 12));

      Assert.AreEqual(poseD, (PoseD)pose);
      Assert.AreEqual(poseD, pose.ToPoseD());
    }


    [Test]
    public void SerializationXml()
    {
      Pose pose1 = new Pose(new Vector3F(1, 2, 3), new Matrix33F(4, 5, 6, 7, 8, 9, 10, 11, 12));
      Pose pose2;

      string fileName = "SerializationPose.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Pose));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, pose1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        pose2 = (Pose)serializer.ReadObject(reader);

      Assert.AreEqual(pose1, pose2);
    }


    [Test]
    public void SerializationJson()
    {
      Pose pose1 = new Pose(new Vector3F(1, 2, 3), new Matrix33F(4, 5, 6, 7, 8, 9, 10, 11, 12));
      Pose pose2;

      string fileName = "SerializationPose.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Pose));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, pose1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        pose2 = (Pose)serializer.ReadObject(stream);

      Assert.AreEqual(pose1, pose2);
    }
  }
}
