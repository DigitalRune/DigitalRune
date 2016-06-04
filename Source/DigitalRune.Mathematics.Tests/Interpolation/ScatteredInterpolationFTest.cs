using System;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class ScatteredInterpolationFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddPairException()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(null, new VectorF(1, 2)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddPairException2()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), null));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void AddPairException3()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(1, 3)));
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(2, 3))); // Invalid dimension.
    }

    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void AddPairException4()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(1, 3)));
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(3, 2), new VectorF(1, 3))); // Invalid dimension.
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetXException()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(1, 3)));
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 4), new VectorF(1, 5))); // Invalid dimension.
      shepard.GetX(-1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetXException2()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(1, 3)));
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 4), new VectorF(1, 5))); // Invalid dimension.
      shepard.GetX(2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetYException()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(1, 3)));
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 4), new VectorF(1, 5))); // Invalid dimension.
      shepard.GetY(-1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetYException2()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 2), new VectorF(1, 3)));
      shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, 4), new VectorF(1, 5))); // Invalid dimension.
      shepard.GetY(2);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SetupException()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Setup(); // No data.
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeException()
    {
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      shepard.Compute(null); // No data.
    }
  }
}
