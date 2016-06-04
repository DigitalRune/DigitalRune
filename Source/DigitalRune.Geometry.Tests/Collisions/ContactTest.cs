using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class ContactTest
  {
    [Test]
    public void WorldPositions()
    {
      Contact c = Contact.Create();
      c.Position = new Vector3F(1, 2, 3);
      c.Normal = new Vector3F(0, 0, 1);
      c.PenetrationDepth = 10;

      Assert.AreEqual(new Vector3F(1, 2, 3 + 5), c.PositionAWorld);
      Assert.AreEqual(new Vector3F(1, 2, 3 - 5), c.PositionBWorld);

      // Separation
      c.PenetrationDepth *= -1;
      Assert.AreEqual(new Vector3F(1, 2, 3 - 5), c.PositionAWorld);
      Assert.AreEqual(new Vector3F(1, 2, 3 + 5), c.PositionBWorld);

      // Surface contact (ray cast)
      c.IsRayHit = true;
      c.PenetrationDepth = 10;
      Assert.AreEqual(new Vector3F(1, 2, 3), c.PositionAWorld);
      Assert.AreEqual(new Vector3F(1, 2, 3), c.PositionBWorld);
    }


    [Test]
    public void Swapped()
    {
      Contact c = Contact.Create();
      c.Position = new Vector3F(1, 2, 3);
      c.Normal = new Vector3F(0, 0, 1);
      c.PenetrationDepth = 10;
      c.PositionALocal = new Vector3F(1, 1, 1);
      c.PositionBLocal = new Vector3F(2, 2, 2);
      c.UserData = "userData";
      c.FeatureA = 1;
      c.FeatureB = 2;
      c.IsRayHit = true;
      c.Lifetime = 100;

      Contact swapped = c.Swapped;
      Assert.AreEqual(new Vector3F(1, 2, 3), swapped.Position);
      Assert.AreEqual(new Vector3F(0, 0, -1), swapped.Normal);
      Assert.AreEqual(10, swapped.PenetrationDepth);
      Assert.AreEqual(new Vector3F(1, 1, 1), swapped.PositionBLocal);
      Assert.AreEqual(new Vector3F(2, 2, 2), swapped.PositionALocal);
      //Assert.AreEqual("appData", swapped.ApplicationData);
      Assert.AreEqual("userData", swapped.UserData);
      Assert.AreEqual(1, swapped.FeatureB);
      Assert.AreEqual(2, swapped.FeatureA);
      Assert.AreEqual(true, swapped.IsRayHit);
      Assert.AreEqual(100, swapped.Lifetime);
    }


    [Test]
    public void ToStringTest()
    {
      Contact c = Contact.Create();
      c.Position = new Vector3F(1, 2, 3);
      c.Normal = new Vector3F(0, 0, 1);
      c.PenetrationDepth = 10;
      Assert.AreEqual("Contact { Position = (1; 2; 3), Normal = (0; 0; 1), PenetrationDepth = 10, Lifetime = 0 }",
                      c.ToString());
    }
  }
}


