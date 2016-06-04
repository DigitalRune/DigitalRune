using System;
using NUnit.Framework;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class RandomHelperTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RandomShouldThrowArgumentNullException()
    {
      RandomHelper.Random = null;
    }


    [Test]
    public void RandomDouble()
    {
      double random = RandomHelper.Random.NextDouble(10.0, 11.0);
      Assert.IsTrue(10.0 <= random);
      Assert.IsTrue(random <= 11.0);

      // Must not throw exception.
      RandomHelper.NextDouble(null, 10.0, 11.0);
    }


    [Test]
    public void RandomFloat()
    {
      float random = RandomHelper.Random.NextFloat(20.0f, 21.0f);
      Assert.IsTrue(20.0f <= random);
      Assert.IsTrue(random <= 21.0f);

      // Must not throw exception.
      RandomHelper.NextFloat(null, 10.0f, 11.0f);
    }


    [Test]
    public void RandomInt()
    {
      int random = RandomHelper.Random.NextInteger(10, 20);
      Assert.IsTrue(10 <= random);
      Assert.IsTrue(random <= 20);

      random = RandomHelper.Random.NextInteger(0, 0);
      Assert.AreEqual(0, random);

      random = RandomHelper.Random.NextInteger(-20, -20);
      Assert.AreEqual(-20, random);

      // Must not throw exception.
      RandomHelper.NextInteger(null, 10, 11);
    }


    [Test]
    public void RandomByte()
    {
      int random = RandomHelper.Random.NextByte();
      Assert.IsTrue(0 <= random);
      Assert.IsTrue(random <= 255);

      // Must not throw exception.
      RandomHelper.NextByte(null);
    }


    [Test]
    public void RandomBool()
    {
      bool random = RandomHelper.Random.NextBool();
      bool b2 = RandomHelper.NextBool(null);

      // Must not throw exception.
      RandomHelper.NextBool(null);
    }


    [Test]
    public void RandomNumberGenerator()
    {
      Random random = RandomHelper.Random;
      Assert.IsNotNull(random);

      random = new Random();
      RandomHelper.Random = random;
      Assert.AreSame(random, RandomHelper.Random);
    }


    [Test]
    public void RandomVector2F()
    {
      Vector2F vector = RandomHelper.Random.NextVector2F(-20.0f, -10.0f);
      Assert.IsTrue(-20.0f <= vector.X && vector.X <= -10.0f);
      Assert.IsTrue(-20.0f <= vector.Y && vector.Y <= -10.0f);

      vector = RandomHelper.Random.NextVector2F(1.0f, 1.0f);
      Assert.AreEqual(1.0f, vector.X);
      Assert.AreEqual(1.0f, vector.Y);

      // Must not throw exception.
      RandomHelper.NextVector2F(null, 1, 3);
    }


    [Test]
    public void RandomVector2D()
    {
      Vector2D vector = RandomHelper.Random.NextVector2D(-20.0, -10.0);
      Assert.IsTrue(-20.0 <= vector.X && vector.X <= -10.0);
      Assert.IsTrue(-20.0 <= vector.Y && vector.Y <= -10.0);

      vector = RandomHelper.Random.NextVector2D(1.0, 1.0);
      Assert.AreEqual(1.0, vector.X);
      Assert.AreEqual(1.0, vector.Y);

      // Must not throw exception.
      RandomHelper.NextVector2D(null, 1, 3);
    }


    [Test]
    public void RandomVector3F()
    {
      Vector3F vector = RandomHelper.Random.NextVector3F(-20.0f, -10.0f);
      Assert.IsTrue(-20.0f <= vector.X && vector.X <= -10.0f);
      Assert.IsTrue(-20.0f <= vector.Y && vector.Y <= -10.0f);
      Assert.IsTrue(-20.0f <= vector.Z && vector.Z <= -10.0f);

      vector = RandomHelper.Random.NextVector3F(1.0f, 1.0f);
      Assert.AreEqual(1.0f, vector.X);
      Assert.AreEqual(1.0f, vector.Y);
      Assert.AreEqual(1.0f, vector.Z);

      // Must not throw exception.
      RandomHelper.NextVector3F(null, 1, 3);
    }


    [Test]
    public void RandomVector3D()
    {
      Vector3D vector = RandomHelper.Random.NextVector3D(-20.0, -10.0);
      Assert.IsTrue(-20.0 <= vector.X && vector.X <= -10.0);
      Assert.IsTrue(-20.0 <= vector.Y && vector.Y <= -10.0);
      Assert.IsTrue(-20.0 <= vector.Z && vector.Z <= -10.0);

      vector = RandomHelper.Random.NextVector3D(1.0, 1.0);
      Assert.AreEqual(1.0, vector.X);
      Assert.AreEqual(1.0, vector.Y);
      Assert.AreEqual(1.0, vector.Z);

      // Must not throw exception.
      RandomHelper.NextVector3D(null, 1, 3);
    }


    [Test]
    public void RandomVector4F()
    {
      Vector4F vector = RandomHelper.Random.NextVector4F(-20.0f, -10.0f);
      Assert.IsTrue(-20.0f <= vector.X && vector.X <= -10.0f);
      Assert.IsTrue(-20.0f <= vector.Y && vector.Y <= -10.0f);
      Assert.IsTrue(-20.0f <= vector.Z && vector.Z <= -10.0f);
      Assert.IsTrue(-20.0f <= vector.W && vector.W <= -10.0f);

      vector = RandomHelper.Random.NextVector4F(-1.0f, -1.0f);
      Assert.AreEqual(-1.0f, vector.X);
      Assert.AreEqual(-1.0f, vector.Y);
      Assert.AreEqual(-1.0f, vector.Z);
      Assert.AreEqual(-1.0f, vector.W);

      // Must not throw exception.
      RandomHelper.NextVector4F(null, 1, 3);
    }


    [Test]
    public void RandomVector4D()
    {
      Vector4D vector = RandomHelper.Random.NextVector4D(-20.0, -10.0);
      Assert.IsTrue(-20.0 <= vector.X && vector.X <= -10.0);
      Assert.IsTrue(-20.0 <= vector.Y && vector.Y <= -10.0);
      Assert.IsTrue(-20.0 <= vector.Z && vector.Z <= -10.0);
      Assert.IsTrue(-20.0 <= vector.W && vector.W <= -10.0);

      vector = RandomHelper.Random.NextVector4D(-1.0, -1.0);
      Assert.AreEqual(-1.0, vector.X);
      Assert.AreEqual(-1.0, vector.Y);
      Assert.AreEqual(-1.0, vector.Z);
      Assert.AreEqual(-1.0, vector.W);

      // Must not throw exception.
      RandomHelper.NextVector4D(null, 1, 3);
    }


    [Test]
    public void RandomVectorF()
    {
      var vector = new VectorF(6);
      RandomHelper.Random.NextVectorF(vector, -2.0f, 0.5f);
      for (int i = 0; i < 6; i++)
      {
        Assert.IsTrue(-2.0f <= vector[i] && vector[i] <= 0.5f);
      }

      // Must not throw exception.
      RandomHelper.NextVectorF(null, vector, 1, 3);
    }


    [Test]
    public void RandomVectorD()
    {
      var vector = new VectorD(6);
      RandomHelper.Random.NextVectorD(vector, -2.0, 0.5);
      for (int i = 0; i < 6; i++)
      {
        Assert.IsTrue(-2.0 <= vector[i] && vector[i] <= 0.5);
      }

      // Must not throw exception.
      RandomHelper.NextVectorD(null, vector, 1, 3);
    }

    
    [Test]
    public void RandomQuaternionF()
    {
      QuaternionF quaternion1 = RandomHelper.Random.NextQuaternionF();
      QuaternionF quaternion2 = RandomHelper.Random.NextQuaternionF();
      Assert.AreNotEqual(quaternion1, quaternion2);
      Assert.IsTrue(quaternion1.IsNumericallyNormalized);
      Assert.IsTrue(quaternion2.IsNumericallyNormalized);

      // Must not throw exception.
      RandomHelper.NextQuaternionF(null);
    }


    [Test]
    public void RandomQuaternionD()
    {
      QuaternionD quaternion1 = RandomHelper.Random.NextQuaternionD();
      QuaternionD quaternion2 = RandomHelper.Random.NextQuaternionD();
      Assert.AreNotEqual(quaternion1, quaternion2);
      Assert.IsTrue(quaternion1.IsNumericallyNormalized);
      Assert.IsTrue(quaternion2.IsNumericallyNormalized);

      // Must not throw exception.
      RandomHelper.NextQuaternionD(null);
    }


    [Test]
    public void RandomMatrix22F()
    {
      Matrix22F matrix = RandomHelper.Random.NextMatrix22F(10.0f, 100.0f);
      for (int i = 0; i < 4; i++)
      {
        Assert.IsTrue(10.0f <= matrix[i] && matrix[i] <= 100.0f);
      }

      // Must not throw exception.
      RandomHelper.NextMatrix22F(null, 1, 3);
    }


    [Test]
    public void RandomMatrix22D()
    {
      Matrix22D matrix = RandomHelper.Random.NextMatrix22D(10.0, 100.0);
      for (int i = 0; i < 4; i++)
      {
        Assert.IsTrue(10.0 <= matrix[i] && matrix[i] <= 100.0);
      }

      // Must not throw exception.
      RandomHelper.NextMatrix22D(null, 1, 3);
    }


    [Test]
    public void RandomMatrix33F()
    {
      Matrix33F matrix = RandomHelper.Random.NextMatrix33F(10.0f, 100.0f);
      for (int i = 0; i < 9; i++)
      {
        Assert.IsTrue(10.0f <= matrix[i] && matrix[i] <= 100.0f);
      }

      // Must not throw exception.
      RandomHelper.NextMatrix33F(null, 1, 3);
    }


    [Test]
    public void RandomMatrix33D()
    {
      Matrix33D matrix = RandomHelper.Random.NextMatrix33D(10.0, 100.0);
      for (int i = 0; i < 9; i++)
      {
        Assert.IsTrue(10.0 <= matrix[i] && matrix[i] <= 100.0);
      }

      // Must not throw exception.
      RandomHelper.NextMatrix33D(null, 1, 3);
    }


    [Test]
    public void RandomMatrix44F()
    {
      Matrix44F matrix = RandomHelper.Random.NextMatrix44F(-2.0f, 0.5f);
      for (int i = 0; i < 16; i++)
      {
        Assert.IsTrue(-2.0f <= matrix[i] && matrix[i] <= 0.5f);
      }

      // Must not throw exception.
      RandomHelper.NextMatrix44F(null, 1, 3);
    }


    [Test]
    public void RandomMatrix44D()
    {
      Matrix44D matrix = RandomHelper.Random.NextMatrix44D(-2.0, 0.5);
      for (int i = 0; i < 16; i++)
      {
        Assert.IsTrue(-2.0 <= matrix[i] && matrix[i] <= 0.5);
      }

      // Must not throw exception.
      RandomHelper.NextMatrix44D(null, 1, 3);
    }


    [Test]
    public void RandomMatrixF()
    {
      var matrix = new MatrixF(2, 3);
      RandomHelper.Random.NextMatrixF(matrix, -2.0f, 0.5f);
      for (int i = 0; i < 6; i++)
      {
        Assert.IsTrue(-2.0f <= matrix[i] && matrix[i] <= 0.5f);
      }

      // Must not throw exception.
      RandomHelper.NextMatrixF(null, matrix, 1, 3);
    }


    [Test]
    public void RandomMatrixD()
    {
      var matrix = new MatrixD(2, 3);
      RandomHelper.Random.NextMatrixD(matrix, -2.0, 0.5);
      for (int i = 0; i < 6; i++)
      {
        Assert.IsTrue(-2.0 <= matrix[i] && matrix[i] <= 0.5);
      }

      // Must not throw exception.
      RandomHelper.NextMatrixD(null, matrix, 1, 3);
    }


    [Test]
    public void RandomFromDistribution()
    {
      Distribution<float> distribution = new ConstValueDistribution<float>(123.456f);
      float f = RandomHelper.Next(null, distribution);
      Assert.AreEqual(123.456f, f);

      var random = new Random(123456);
      distribution = new UniformDistributionF(1.0f, 2.0f);
      f = random.Next(distribution);
      Assert.IsTrue(1.0f <= f && f <= 2.0f);
    }
  }
}
