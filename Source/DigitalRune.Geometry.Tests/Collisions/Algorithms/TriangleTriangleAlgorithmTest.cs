using System;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class TriangleTriangleAlgorithmTest
  {
    //--------------------------------------------------------------
    #region Face-Vertex contacts
    //--------------------------------------------------------------

    [Test]
    public void FaceAVertexB0Up()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(0, -1, 0), new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceAVertexB0Down()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(0, 1, 0), new Vector3F(10, -10, 0), new Vector3F(-10, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceAVertexB1Up()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-10, 10, 0), new Vector3F(0, -1, 0), new Vector3F(10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceAVertexB1Down()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-10, -10, 0), new Vector3F(0, 1, 0), new Vector3F(10, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceAVertexB2Up()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0), new Vector3F(0, -1, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceAVertexB2Down()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, -10, 0), new Vector3F(-10, -10, 0), new Vector3F(0, 1, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceBVertexA0Down()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(0, -1, 0), new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceBVertexA0Up()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(0, 1, 0), new Vector3F(10, -10, 0), new Vector3F(-10, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceBVertexA1Down()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(-10, 10, 0), new Vector3F(0, -1, 0), new Vector3F(10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceBVertexA1Up()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(-10, -10, 0), new Vector3F(0, 1, 0), new Vector3F(10, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceBVertexA2Down()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0), new Vector3F(0, -1, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void FaceBVertexA2Up()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(10, -10, 0), new Vector3F(-10, -10, 0), new Vector3F(0, 1, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.5f, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }
    #endregion


    //--------------------------------------------------------------
    #region Edge-Edge contacts
    //--------------------------------------------------------------

    [Test]
    public void EdgeA0EdgeB0_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(-1, 10, 0), new Vector3F(-1, -10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA0EdgeB0_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(-1, -10, 0), new Vector3F(-1, 10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA0EdgeB1_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(-1, 10, 0), new Vector3F(-1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA0EdgeB1_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(-1, -10, 0), new Vector3F(-1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA0EdgeB2_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(-1, -10, 0), new Vector3F(10, 0, 0), new Vector3F(-1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA0EdgeB2_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(-1, 10, 0), new Vector3F(10, 0, 0), new Vector3F(-1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA1EdgeB0_0()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(-1, 10, 0), new Vector3F(-1, -10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA1EdgeB0_1()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(-1, -10, 0), new Vector3F(-1, 10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA1EdgeB1_0()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(-1, 10, 0), new Vector3F(-1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA1EdgeB1_1()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(-1, -10, 0), new Vector3F(-1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA1EdgeB2_0()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(-1, -10, 0), new Vector3F(10, 0, 0), new Vector3F(-1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA1EdgeB2_1()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(-1, 10, 0), new Vector3F(10, 0, 0), new Vector3F(-1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA2EdgeB0_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-1, 10, 0), new Vector3F(-1, -10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA2EdgeB0_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-1, -10, 0), new Vector3F(-1, 10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA2EdgeB1_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(-1, 10, 0), new Vector3F(-1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA2EdgeB1_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(-1, -10, 0), new Vector3F(-1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA2EdgeB2_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-1, -10, 0), new Vector3F(10, 0, 0), new Vector3F(-1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }

    [Test]
    public void EdgeA2EdgeB2_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-1, 10, 0), new Vector3F(10, 0, 0), new Vector3F(-1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }
    #endregion


    //--------------------------------------------------------------
    #region Face separations
    //--------------------------------------------------------------

    [Test]
    public void FaceASeparationUp()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(0, 1, 0), new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void FaceASeparationDown()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(0, -1, 0), new Vector3F(10, -10, 0), new Vector3F(-10, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void FaceBSeparationUp()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(0, 1, 0), new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void FaceBSeparationDown()
    {
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tA = new Triangle(new Vector3F(0, -1, 0), new Vector3F(10, -10, 0), new Vector3F(-10, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }
    #endregion


    //--------------------------------------------------------------
    #region Edge-Edge separations
    //--------------------------------------------------------------

    [Test]
    public void SeparationEdgeA0EdgeB0_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(1, 10, 0), new Vector3F(1, -10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA0EdgeB0_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(1, -10, 0), new Vector3F(1, 10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA0EdgeB1_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(1, 10, 0), new Vector3F(1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA0EdgeB1_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(1, -10, 0), new Vector3F(1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA0EdgeB2_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(1, -10, 0), new Vector3F(10, 0, 0), new Vector3F(1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA0EdgeB2_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0));
      var tB = new Triangle(new Vector3F(1, 10, 0), new Vector3F(10, 0, 0), new Vector3F(1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA1EdgeB0_0()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(1, 10, 0), new Vector3F(1, -10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA1EdgeB0_1()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(1, -10, 0), new Vector3F(1, 10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA1EdgeB1_0()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(1, 10, 0), new Vector3F(1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA1EdgeB1_1()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(1, -10, 0), new Vector3F(1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA1EdgeB2_0()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(1, -10, 0), new Vector3F(10, 0, 0), new Vector3F(1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA1EdgeB2_1()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, -10));
      var tB = new Triangle(new Vector3F(1, 10, 0), new Vector3F(10, 0, 0), new Vector3F(1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA2EdgeB0_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(1, 10, 0), new Vector3F(1, -10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA2EdgeB0_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(1, -10, 0), new Vector3F(1, 10, 0), new Vector3F(10, 0, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA2EdgeB1_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(1, 10, 0), new Vector3F(1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA2EdgeB1_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 0, 0), new Vector3F(1, -10, 0), new Vector3F(1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA2EdgeB2_0()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(1, -10, 0), new Vector3F(10, 0, 0), new Vector3F(1, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }

    [Test]
    public void SeparationEdgeA2EdgeB2_1()
    {
      var tA = new Triangle(new Vector3F(0, 0, -10), new Vector3F(-10, 0, 0), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(1, 10, 0), new Vector3F(10, 0, 0), new Vector3F(1, -10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }
    #endregion


    //--------------------------------------------------------------
    #region Degenerate cases
    //--------------------------------------------------------------

    [Test]
    public void FaceADegenerate()
    {
      var tA = new Triangle(new Vector3F(-1, 0, 0), new Vector3F(10, 0, -0), new Vector3F(10, 0, 0));
      var tB = new Triangle(new Vector3F(0, -10, -10), new Vector3F(0, -10, 10), new Vector3F(0, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }


    [Test]
    public void FaceBDegenerate()
    {
      var tB = new Triangle(new Vector3F(-1, 0, 0), new Vector3F(10, 0, -0), new Vector3F(10, 0, 0));
      var tA = new Triangle(new Vector3F(0, -10, -10), new Vector3F(0, -10, 10), new Vector3F(0, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.5f, 0, 0), p));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), n));
      Assert.IsTrue(Numeric.AreEqual(1, d));
    }


    [Test]
    public void BothFacesDegenerate()
    {
      var tA = new Triangle(new Vector3F(-10, 0, 0), new Vector3F(-10, 0, 0), new Vector3F(10, 0, 0));
      var tB = new Triangle(new Vector3F(0, 0, -10), new Vector3F(0, 0, 10), new Vector3F(0, 0, 10));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }


    [Test]
    public void PointPoint()
    {
      var a = new Vector3F(1, 1, 1);
      var tA = new Triangle(a, a, a);
      var tB = new Triangle(a, a, a);

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(false, haveContact);
    }
    #endregion


    //--------------------------------------------------------------
    #region Vertex-Vertex cases
    //--------------------------------------------------------------

    [Test]
    public void VertexA0VertexB0()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 0, -10), new Vector3F(10, 10, 0), new Vector3F(-10, 10, 0));

      Vector3F p, n;
      float d;
      bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

      Assert.AreEqual(true, haveContact);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(tA.Vertex0, p));
      Assert.IsTrue(Numeric.AreEqual(0, d));
    }
    #endregion


    //--------------------------------------------------------------
    #region Face vs. Planar Edge
    //--------------------------------------------------------------

    [Test]
    public void FaceEdge()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-10, -1, 0), new Vector3F(10, -1, 0), new Vector3F(0, 10, 0));

      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          for (int k = 0; k < 3; k++)
          {
            Vector3F p, n;
            float d;
            bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

            Assert.AreEqual(true, haveContact);
            Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-5, -0.5f, 0), p)
                          || Vector3F.AreNumericallyEqual(new Vector3F(5, -0.5f, 0), p));
            Assert.IsTrue(GeometryHelper.IsOver(tB, p));
            if (i == 0)
              Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n));
            else
              Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
            Assert.IsTrue(Numeric.AreEqual(1, d));

            // "Rotate" triangle to test next edges.
            var oldB = tB;
            tB[0] = oldB[1];
            tB[1] = oldB[2];
            tB[2] = oldB[0];
          }

          // "Rotate" triangle to test next edges.
          var oldA = tA;
          tA[0] = oldA[1];
          tA[1] = oldA[2];
          tA[2] = oldA[0];
        }

        MathHelper.Swap(ref tA, ref tB);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Coplanar Triangles
    //--------------------------------------------------------------

    [Test]
    public void CoplanarAndSeparated()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(10, 0, 20), new Vector3F(-10, 0, 20), new Vector3F(0, 0, 40));

      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          for (int k = 0; k < 3; k++)
          {
            Vector3F p, n;
            float d;
            bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

            Assert.AreEqual(false, haveContact);

            // "Rotate" triangle to test next edges.
            var oldB = tB;
            tB[0] = oldB[1];
            tB[1] = oldB[2];
            tB[2] = oldB[0];
          }

          // "Rotate" triangle to test next edges.
          var oldA = tA;
          tA[0] = oldA[1];
          tA[1] = oldA[2];
          tA[2] = oldA[0];
        }

        MathHelper.Swap(ref tA, ref tB);
      }
    }


    [Test]
    public void CoplanarAndFaceVertex()
    {
      var tA = new Triangle(new Vector3F(10, 0, 10), new Vector3F(0, 0, -10), new Vector3F(-10, 0, 10));
      var tB = new Triangle(new Vector3F(-1, 0, 0), new Vector3F(-10, 0, 20), new Vector3F(10, 0, 20));

      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          for (int k = 0; k < 3; k++)
          {
            Vector3F p, n;
            float d;
            bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

            Assert.AreEqual(true, haveContact);
            Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n)
                          || Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
            Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-1, 0, 0), p));
            Assert.AreEqual(0, d);

            // "Rotate" triangle to test next edges.
            var oldB = tB;
            tB[0] = oldB[1];
            tB[1] = oldB[2];
            tB[2] = oldB[0];
          }

          // "Rotate" triangle to test next edges.
          var oldA = tA;
          tA[0] = oldA[1];
          tA[1] = oldA[2];
          tA[2] = oldA[0];
        }

        MathHelper.Swap(ref tA, ref tB);
      }
    }


    [Test]
    public void CoplanarAndEdge()
    {
      var tA = new Triangle(new Vector3F(10, 0, -10), new Vector3F(-10, 0, -10), new Vector3F(0, 0, 0));
      var tB = new Triangle(new Vector3F(10, 0, -1), new Vector3F(-10, 0, -1), new Vector3F(10, 0, -2));

      for (int a = 0; a < 2; a++)
      {
        for (int b = 0; b < 2; b++)
        {
          for (int i = 0; i < 2; i++)
          {
            for (int j = 0; j < 3; j++)
            {
              for (int k = 0; k < 3; k++)
              {
                Vector3F p, n;
                float d;
                bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

                Assert.AreEqual(true, haveContact);
                Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), n)
                              || Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), n));
                Assert.IsTrue(GeometryHelper.IsOver(tA, p, 0.00001f));
                Assert.IsTrue(GeometryHelper.IsOver(tB, p, 0.00001f));
                //Assert.IsTrue(Vector3F.AreNumericallyEqual(?, p));
                Assert.AreEqual(0, d);

                // "Rotate" triangle to test next edges.
                var oldB = tB;
                tB[0] = oldB[1];
                tB[1] = oldB[2];
                tB[2] = oldB[0];
              }

              // "Rotate" triangle to test next edges.
              var oldA = tA;
              tA[0] = oldA[1];
              tA[1] = oldA[2];
              tA[2] = oldA[0];
            }

            MathHelper.Swap(ref tA, ref tB);
          }

          // Change winding order.
          MathHelper.Swap(ref tB.Vertex0, ref tB.Vertex1);
        }

        // Change winding order.
        MathHelper.Swap(ref tA.Vertex0, ref tA.Vertex1);
      }

      
    }


    [Test]
    public void CoplanarAndDegenerate()
    {
      var tA = new Triangle(new Vector3F(0, 0, 0), new Vector3F(0, 0, 10), new Vector3F(0, 0, 10));
      var tB = new Triangle(new Vector3F(-10, 0, 10), new Vector3F(-10, 0, 20), new Vector3F(10, 0, 20));

      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          for (int k = 0; k < 3; k++)
          {
            Vector3F p, n;
            float d;
            bool haveContact = TriangleTriangleAlgorithm.GetContact(ref tA, ref tB, out p, out n, out d);

            Assert.AreEqual(false, haveContact);

            // "Rotate" triangle to test next edges.
            var oldB = tB;
            tB[0] = oldB[1];
            tB[1] = oldB[2];
            tB[2] = oldB[0];
          }

          // "Rotate" triangle to test next edges.
          var oldA = tA;
          tA[0] = oldA[1];
          tA[1] = oldA[2];
          tA[2] = oldA[0];
        }

        MathHelper.Swap(ref tA, ref tB);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Stochastic Tests
    //--------------------------------------------------------------
    
    [Test]
    public void Test0()
    {
      var tta = new TriangleTriangleAlgorithm(new CollisionDetection());

      int numberOfContacts = 0;
      int numberOfTests = 10000;
      RandomHelper.Random = new Random(1234567);
      for (int i = 0; i < numberOfTests; i++)
      {
        // Create two triangles.
        var tA = new Triangle();
        tA.Vertex0 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));
        tA.Vertex1 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));
        tA.Vertex2 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));

        var tB = new Triangle();
        tB.Vertex0 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));
        tB.Vertex1 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));
        tB.Vertex2 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));

        var ttTest = GeometryHelper.HaveContact(tA, tB);

        if (ttTest)
          numberOfContacts++;

        var coA = new CollisionObject(new GeometricObject(new TriangleShape(tA)));
        var coB = new CollisionObject(new GeometricObject(new TriangleShape(tB)));

        var cs = tta.GetContacts(coA, coB);

        Assert.AreEqual(ttTest, cs.HaveContact);

        if (cs.HaveContact)
        {
          Assert.AreEqual(1, cs.Count);
          var epsilon = 0.01f;
          if (!GeometryHelper.IsOver(tA, cs[0].PositionAWorld, epsilon))
            Debugger.Break();
          if (!GeometryHelper.IsOver(tB, cs[0].PositionBWorld, epsilon))
            Debugger.Break();
          Assert.IsTrue(GeometryHelper.IsOver(tA, cs[0].PositionAWorld, epsilon));
          Assert.IsTrue(GeometryHelper.IsOver(tB, cs[0].PositionBWorld, epsilon));
        }
      }

      //Trace.WriteLine("% hits:" + 100f * numberOfContacts / numberOfTests);
    }


    [Test]
    public void TestCoplanar()
    {
      var tta = new TriangleTriangleAlgorithm(new CollisionDetection());
      var gjk = new Gjk(new CollisionDetection());

      int numberOfContacts = 0;
      int numberOfTests = 1000;
      RandomHelper.Random = new Random(1234567);
      for (int i = 0; i < numberOfTests; i++)
      {
        // Create two triangles.
        var tA = new Triangle();
        tA.Vertex0 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), -290, RandomHelper.Random.NextFloat(-100, 100));
        tA.Vertex1 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), -290, RandomHelper.Random.NextFloat(-100, 100));
        tA.Vertex2 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), -290, RandomHelper.Random.NextFloat(-100, 100));

        var tB = new Triangle();
        tB.Vertex0 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), -290, RandomHelper.Random.NextFloat(-100, 100));
        tB.Vertex1 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), -290, RandomHelper.Random.NextFloat(-100, 100));
        tB.Vertex2 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), -290, RandomHelper.Random.NextFloat(-100, 100));

        var ttTest = GeometryHelper.HaveContact(tA, tB);

        if (ttTest)
          numberOfContacts++;

        var coA = new CollisionObject(new GeometricObject(new TriangleShape(tA)));
        var coB = new CollisionObject(new GeometricObject(new TriangleShape(tB)));

        var csTta = tta.GetContacts(coA, coB);
        var csGjk = gjk.GetClosestPoints(coA, coB);

        if (csTta.HaveContact != csGjk.HaveContact)
          Trace.WriteLine("Test failed: " + i + " GJK: " + csGjk.HaveContact + " TTA: " + csTta);

        Assert.AreEqual(ttTest, csGjk.HaveContact);
        Assert.AreEqual(ttTest, csTta.HaveContact);
      }

      //Trace.WriteLine("% hits:" + 100f * numberOfContacts / numberOfTests);
    }
    #endregion
  }
}
