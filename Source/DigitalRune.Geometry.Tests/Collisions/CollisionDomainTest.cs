using System;
using System.Linq;
using DigitalRune.Geometry.Collisions.Algorithms;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class CollisionDomainTest
  {
    //[Test]
    //public void AlgorithmMatrix()
    //{
    //  CollisionDetection cd = new CollisionDetection();
    //  CollisionDomain domain = new CollisionDomain(cd);
    //  Assert.AreEqual(cd.AlgorithmMatrix, domain.AlgorithmMatrix);

    //  domain.AlgorithmMatrix = new CollisionAlgorithmMatrix(cd);
    //  Assert.AreNotEqual(cd.AlgorithmMatrix, domain.AlgorithmMatrix);

    //  domain.AlgorithmMatrix = null;
    //  Assert.AreEqual(cd.AlgorithmMatrix, domain.AlgorithmMatrix);
    //}


    //[Test]
    //public void Filter()
    //{
    //  CollisionDetection cd = new CollisionDetection();
    //  CollisionDomain domain = new CollisionDomain(cd);
    //  Assert.AreEqual(cd.Filter, domain.Filter);

    //  domain.Filter = new CollisionFilter();
    //  Assert.AreNotEqual(cd.Filter, domain.Filter);

    //  domain.Filter = null;
    //  Assert.AreEqual(cd.Filter, domain.Filter);
    //}


    [Test]
    public void Test1()
    {
      Assert.NotNull(new CollisionDomain().CollisionDetection);
      Assert.NotNull(new CollisionDomain(null).CollisionDetection);

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      CollisionObject a = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      //a.Name = "a";

      CollisionObject b = new CollisionObject();
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(4, 0, 2f));
      //b.Name = "b";

      CollisionObject c = new CollisionObject();
      ((GeometricObject)c.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)c.GeometricObject).Pose = new Pose(new Vector3F(6, 2, 2f));
      //c.Name = "c";

      CollisionObject d = new CollisionObject();
      ((GeometricObject)d.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)d.GeometricObject).Pose = new Pose(new Vector3F(8, 3f, 4f));
      //d.Name = "d";

      Assert.AreEqual(0, cd.CollisionObjects.Count);

      cd.CollisionObjects.Add(a);
      Assert.AreEqual(1, cd.CollisionObjects.Count);

      cd.CollisionObjects.Add(b);
      Assert.AreEqual(2, cd.CollisionObjects.Count);

      cd.CollisionObjects.Add(c);
      Assert.AreEqual(3, cd.CollisionObjects.Count);

      cd.CollisionObjects.Add(d);
      Assert.AreEqual(4, cd.CollisionObjects.Count);

      cd.Update(0.01f);
      Assert.AreEqual(0, cd.ContactSets.Count);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(((GeometricObject)a.GeometricObject).Pose.Position + new Vector3F(3.5f, 0, 0.5f));
      cd.Update(0.01f);
      Assert.AreEqual(1, cd.ContactSets.Count);

      ((GeometricObject)c.GeometricObject).Pose = new Pose(((GeometricObject)c.GeometricObject).Pose.Position + new Vector3F(2, 1, 0));
      cd.Update(0.01f);
      Assert.AreEqual(2, cd.ContactSets.Count);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(((GeometricObject)a.GeometricObject).Pose.Position + new Vector3F(3, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(((GeometricObject)b.GeometricObject).Pose.Position + new Vector3F(2, 0, 0));
      cd.Update(0.01f);
      Assert.AreEqual(2, cd.ContactSets.Count);

      ((GeometricObject)d.GeometricObject).Pose = new Pose(((GeometricObject)d.GeometricObject).Pose.Position + new Vector3F(-0.5f, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(((GeometricObject)b.GeometricObject).Pose.Position + new Vector3F(0, 1, 0));
      cd.Update(0.01f);
      Assert.AreEqual(1, cd.ContactSets.Count);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)c.GeometricObject).Pose = new Pose(new Vector3F(1, 0, 1));
      ((GeometricObject)d.GeometricObject).Pose = new Pose(new Vector3F(0, 1, 0));
      cd.Update(0.01f);
      Assert.AreEqual(6, cd.ContactSets.Count);
    }


    [Test]
    public void Filtering()
    {
      CollisionDomain domain = new CollisionDomain(new CollisionDetection());
      domain.CollisionDetection.CollisionFilter = new CollisionFilter();

      CollisionObject a = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      //a.Name = "a";

      CollisionObject b = new CollisionObject();
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(1, 0, 0f));
      //b.Name = "b";

      domain.CollisionObjects.Add(a);
      domain.CollisionObjects.Add(b);
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.ContactSets.Count);

      b.Enabled = false;
      domain.Update(0.01f);
      Assert.AreEqual(0, domain.ContactSets.Count);

      a.Enabled = false;
      b.Enabled = true;
      domain.Update(0.01f);
      Assert.AreEqual(0, domain.ContactSets.Count);

      a.Enabled = true;
      ((CollisionFilter) domain.CollisionDetection.CollisionFilter).Set(a, b, false);
      domain.Update(0.01f);
      Assert.AreEqual(0, domain.ContactSets.Count);

      ((CollisionFilter) domain.CollisionDetection.CollisionFilter).Set(a, b, true);
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.ContactSets.Count);
    }


    [Test]
    public void HaveContact()
    {
      CollisionDomain domain = new CollisionDomain(new CollisionDetection());
      domain.CollisionDetection.CollisionFilter = new CollisionFilter();

      CollisionObject a = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      //a.Name = "a";

      CollisionObject b = new CollisionObject();
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(1, 0, 0f));
      //b.Name = "b";

      CollisionObject c = new CollisionObject();
      ((GeometricObject)c.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)c.GeometricObject).Pose = new Pose(new Vector3F(1, 0, 0f));
      //c.Name = "c";

      domain.CollisionObjects.Add(a);
      domain.CollisionObjects.Add(b);
      domain.Update(0.01f);
      Assert.AreEqual(true, domain.HaveContact(a, b));
      Assert.AreEqual(true, domain.HaveContact(a, c));
      Assert.AreEqual(true, domain.HasContact(a));
      Assert.AreEqual(true, domain.HasContact(b));
      Assert.AreEqual(true, domain.HasContact(c));
      Assert.AreEqual(1, domain.GetContacts(a, b).Count);
      Assert.AreEqual(1, domain.GetContacts(a, c).Count);
      Assert.AreEqual(1, domain.GetContacts(a).Count());
      Assert.AreEqual(2, domain.GetContacts(c).Count());
      Assert.AreEqual(1, domain.ContactSets.Count);

      b.Enabled = false;
      domain.Update(0.01f);
      Assert.AreEqual(false, domain.HaveContact(a, b));
      Assert.AreEqual(true, domain.HaveContact(a, c));
      Assert.AreEqual(false, domain.HasContact(a));
      Assert.AreEqual(false, domain.HasContact(b));
      Assert.AreEqual(true, domain.HasContact(c));
      Assert.AreEqual(null, domain.GetContacts(a, b));
      Assert.AreEqual(1, domain.GetContacts(a, c).Count);
      Assert.AreEqual(0, domain.GetContacts(a).Count());
      Assert.AreEqual(1, domain.GetContacts(c).Count());
      Assert.AreEqual(0, domain.ContactSets.Count);

      a.Enabled = false;
      b.Enabled = true;
      domain.Update(0.01f);
      Assert.AreEqual(false, domain.HaveContact(a, b));
      Assert.AreEqual(false, domain.HaveContact(a, c));
      Assert.AreEqual(false, domain.HasContact(a));
      Assert.AreEqual(false, domain.HasContact(b));
      Assert.AreEqual(true, domain.HasContact(c));
      Assert.AreEqual(null, domain.GetContacts(a, b));
      Assert.AreEqual(null, domain.GetContacts(a, c));
      Assert.AreEqual(0, domain.GetContacts(a).Count());
      Assert.AreEqual(1, domain.GetContacts(c).Count());
      Assert.AreEqual(0, domain.ContactSets.Count);
      
      c.Enabled = false;
      domain.Update(0.01f);
      Assert.AreEqual(false, domain.HaveContact(a, b));
      Assert.AreEqual(false, domain.HaveContact(a, c));
      Assert.AreEqual(false, domain.HasContact(a));
      Assert.AreEqual(false, domain.HasContact(b));
      Assert.AreEqual(false, domain.HasContact(c));
      Assert.AreEqual(null, domain.GetContacts(a, b));
      Assert.AreEqual(null, domain.GetContacts(a, c));
      Assert.AreEqual(0, domain.GetContacts(a).Count());
      Assert.AreEqual(0, domain.GetContacts(c).Count());
      Assert.AreEqual(0, domain.ContactSets.Count);

      a.Enabled = true;
      c.Enabled = true;
      ((CollisionFilter) domain.CollisionDetection.CollisionFilter).Set(a, b, false);
      domain.Update(0.01f);
      Assert.AreEqual(false, domain.HaveContact(a, b));
      Assert.AreEqual(true, domain.HaveContact(a, c));
      Assert.AreEqual(false, domain.HasContact(a));
      Assert.AreEqual(false, domain.HasContact(b));
      Assert.AreEqual(true, domain.HasContact(c));
      Assert.AreEqual(null, domain.GetContacts(a, b));
      Assert.AreEqual(1, domain.GetContacts(a, c).Count);
      Assert.AreEqual(0, domain.GetContacts(a).Count());
      Assert.AreEqual(2, domain.GetContacts(c).Count());
      Assert.AreEqual(0, domain.ContactSets.Count);

      ((CollisionFilter) domain.CollisionDetection.CollisionFilter).Set(a, b, true);
      domain.Update(0.01f);
      Assert.AreEqual(true, domain.HaveContact(a, b));
      Assert.AreEqual(true, domain.HaveContact(a, c));
      Assert.AreEqual(true, domain.HasContact(a));
      Assert.AreEqual(true, domain.HasContact(b));
      Assert.AreEqual(true, domain.HasContact(c));
      Assert.AreEqual(1, domain.GetContacts(a, b).Count);
      Assert.AreEqual(1, domain.GetContacts(a, c).Count);
      Assert.AreEqual(1, domain.GetContacts(a).Count());
      Assert.AreEqual(2, domain.GetContacts(c).Count());
      Assert.AreEqual(1, domain.ContactSets.Count);

      c.Enabled = false;
      domain.Update(0.01f);
      Assert.AreEqual(true, domain.HaveContact(a, b));
      Assert.AreEqual(false, domain.HaveContact(a, c));
      Assert.AreEqual(true, domain.HasContact(a));
      Assert.AreEqual(true, domain.HasContact(b));
      Assert.AreEqual(false, domain.HasContact(c));
      Assert.AreEqual(1, domain.GetContacts(a, b).Count);
      Assert.AreEqual(null, domain.GetContacts(a, c));
      Assert.AreEqual(1, domain.GetContacts(a).Count());
      Assert.AreEqual(0, domain.GetContacts(c).Count());
      Assert.AreEqual(1, domain.ContactSets.Count);
    }


    [Test]
    public void RayCastStopsAtFirstHit()
    {
      CollisionDomain domain = new CollisionDomain(new CollisionDetection());

      CollisionObject ray = new CollisionObject();
      ((GeometricObject)ray.GeometricObject).Shape = new RayShape(new Vector3F(), new Vector3F(1, 0, 0), 100) 
      { 
        StopsAtFirstHit = true,
      };
      //ray.Name = "Ray";

      CollisionObject b = new CollisionObject();
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(-10, 0, 0f));
      //b.Name = "b";

      CollisionObject c = new CollisionObject();
      ((GeometricObject)c.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)c.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0f));
      //c.Name = "c";

      CollisionObject d = new CollisionObject();
      ((GeometricObject)d.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)d.GeometricObject).Pose = new Pose(new Vector3F(10, 0, 0f));
      //d.Name = "d";

      CollisionObject e = new CollisionObject();
      ((GeometricObject)e.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)e.GeometricObject).Pose = new Pose(new Vector3F(20, 0, 0f));
      //e.Name = "e";

      CollisionObject f = new CollisionObject();
      ((GeometricObject)f.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)f.GeometricObject).Pose = new Pose(new Vector3F(110, 0, 0f));
      //f.Name = "f";

      // Positions: b=-10, c=0, d=10, e=20, f=110
      domain.CollisionObjects.Add(ray);
      domain.CollisionObjects.Add(b);
      domain.CollisionObjects.Add(d);
      domain.CollisionObjects.Add(c);            
      domain.CollisionObjects.Add(e);
      domain.CollisionObjects.Add(f);

      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(true, domain.HaveContact(ray, c));

      ((GeometricObject)c.GeometricObject).Pose = new Pose(new Vector3F(30));
      // Positions: b=-10, d=10, e=20, c=30, f=110
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(true, domain.HaveContact(ray, d));

      ((GeometricObject)d.GeometricObject).Pose = new Pose(new Vector3F(40));
      // Positions: b=-10, e=20, c=30, d=40, f=110
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(true, domain.HaveContact(ray, e));

      ((GeometricObject)ray.GeometricObject).Pose = new Pose(((GeometricObject)ray.GeometricObject).Pose.Position, QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      domain.Update(0.01f);
      Assert.AreEqual(0, domain.GetContacts(ray).Count());

      ((GeometricObject)ray.GeometricObject).Pose = new Pose(((GeometricObject)ray.GeometricObject).Pose.Position, QuaternionF.CreateRotationZ(ConstantsF.Pi));
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(true, domain.HaveContact(ray, b));

      ((GeometricObject)ray.GeometricObject).Pose = new Pose(((GeometricObject)ray.GeometricObject).Pose.Position, QuaternionF.Identity);
      domain.Update(0.01f);

      // Positions: b=-10, e=20, c=30, d=40, f=110
      CollisionObject gNotInDomain = new CollisionObject();
      ((GeometricObject)gNotInDomain.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)gNotInDomain.GeometricObject).Pose = new Pose(new Vector3F(10, 0, 0f));
      Assert.AreEqual(true, domain.HaveContact(ray, gNotInDomain));
      Assert.AreEqual(1, domain.GetContacts(gNotInDomain).Count());
      Assert.AreEqual(1, domain.GetContacts(ray, gNotInDomain).Count);
      Assert.AreEqual(true, domain.HasContact(gNotInDomain));
      ((GeometricObject)gNotInDomain.GeometricObject).Pose = new Pose(new Vector3F(25, 0, 0f)); // behind e
      Assert.AreEqual(false, domain.HaveContact(ray, gNotInDomain));
      Assert.AreEqual(false, domain.HaveContact(gNotInDomain, ray));
      Assert.AreEqual(false, domain.HasContact(gNotInDomain));
      Assert.AreEqual(0, domain.GetContacts(gNotInDomain).Count());
      Assert.IsNull(domain.GetContacts(ray, gNotInDomain));
      Assert.IsNull(domain.GetContacts(gNotInDomain, ray));

      // Remove ray from domain.
      domain.CollisionObjects.Remove(ray);
      domain.Update(0.01f);
      Assert.AreEqual(0, domain.ContactSets.Count);

      // Positions: b=-10, e=20, g=25, c=30, d=40, f=110
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(true, domain.HaveContact(ray, e));
      Assert.AreEqual(false, domain.HaveContact(ray, c));
      Assert.AreEqual(false, domain.HaveContact(ray, gNotInDomain));
      Assert.IsNull(domain.GetContacts(ray, gNotInDomain));
    }


    [Test]
    public void RayCastStopsAtFirstHitWhenChangingFilter()
    {
      CollisionDomain domain = new CollisionDomain(new CollisionDetection());
      domain.CollisionDetection.CollisionFilter = new CollisionFilter();

      // 1 ray: at origin shooting into +x
      CollisionObject ray = new CollisionObject();
      ((GeometricObject)ray.GeometricObject).Shape = new RayShape(new Vector3F(), new Vector3F(1, 0, 0), 100)
      {
        StopsAtFirstHit = true,
      };
      //ray.Name = "Ray";

      // 2 spheres: at at x=10, b at x=20
      CollisionObject a = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(10, 0, 0f));
      //a.Name = "b";
      CollisionObject b = new CollisionObject();
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(20, 0, 0f));
      //b.Name = "c";

      domain.CollisionObjects.Add(ray);
      domain.CollisionObjects.Add(a);
      domain.CollisionObjects.Add(b);

      // Ray touches b.
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(true, domain.HaveContact(ray, a));
      Assert.AreEqual(false, domain.HaveContact(ray, b));

      // Disable collisions between ray and a.
      // Then ray must hit b.
      ((CollisionFilter)domain.CollisionDetection.CollisionFilter).Set(ray, a, false);
      domain.Update(0.01f);
      Assert.AreEqual(1, domain.GetContacts(ray).Count());
      Assert.AreEqual(false, domain.HaveContact(ray, a));
      Assert.AreEqual(true, domain.HaveContact(ray, b));
    }


    [Test]
    public void ToStringTest()
    {
      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      cd.CollisionObjects.Add(new CollisionObject());
      cd.CollisionObjects.Add(new CollisionObject());
      Assert.AreEqual("CollisionDomain { Count = 2 }", cd.ToString());

    }


    [Test]
    public void NoValidation0()
    {
      GlobalSettings.ValidationLevel = 0;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      // No exception with validation level 0.
      geometricObject.Pose = new Pose(new Vector3F(float.NaN, 0, 0));
      cd.CollisionObjects.Add(co);
    }


    [Test]
    public void NoValidation1()
    {
      GlobalSettings.ValidationLevel = 0;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      // No exception with validation level 0.
      shape.Radius = float.NaN;
      cd.CollisionObjects.Add(co);
    }


    [Test]
    public void ValidateNewObjectWithInvalidPose()
    {
      GlobalSettings.ValidationLevel = 0xff;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      geometricObject.Pose = new Pose(new Vector3F(float.NaN, 0, 0));
      Assert.Throws<GeometryException>(() => cd.CollisionObjects.Add(co));
    }


    [Test]
    public void ValidateNewObjectWithInvalidScale()
    {
      GlobalSettings.ValidationLevel = 0xff;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      geometricObject.Scale = new Vector3F(1, float.NaN, 1);
      Assert.Throws<GeometryException>(() => cd.CollisionObjects.Add(co));
    }


    [Test]
    public void ValidateNewObjectWithInvalidShape()
    {
      GlobalSettings.ValidationLevel = 0xff;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      shape.Radius = float.NaN;
      Assert.Throws<GeometryException>(() => cd.CollisionObjects.Add(co));
    }


    [Test]
    public void ValidateInvalidPoseChange()
    {
      GlobalSettings.ValidationLevel = 0xff;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      cd.CollisionObjects.Add(co);

      var matrix = Matrix33F.Identity;
      matrix.M11 = float.NaN;
      Assert.Throws<GeometryException>(() => geometricObject.Pose = new Pose(new Vector3F(), matrix));
    }


    [Test]
    public void ValidateInvalidScaleChange()
    {
      GlobalSettings.ValidationLevel = 0xff;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      cd.CollisionObjects.Add(co);
      Assert.Throws<GeometryException>(() => geometricObject.Scale = new Vector3F(1, 1, float.NaN));
    }

    [Test]
    public void ValidateInvalidShapeChange()
    {
      GlobalSettings.ValidationLevel = 0xff;

      CollisionDomain cd = new CollisionDomain(new CollisionDetection());
      var shape = new SphereShape(1);
      var geometricObject = new GeometricObject(shape, Pose.Identity);
      var co = new CollisionObject(geometricObject);

      cd.CollisionObjects.Add(co);

      Assert.Throws<GeometryException>(() => shape.Radius = float.NaN);
    }


    [Test]
    public void TestEnabledDisabled()
    {
      var shape = new SphereShape(1);
      var goA = new GeometricObject(shape, Pose.Identity);
      var coA = new CollisionObject(goA);

      var goB = new GeometricObject(shape, Pose.Identity);
      var coB = new CollisionObject(goB);

      var cd = new CollisionDomain(new CollisionDetection());
      
      cd.CollisionObjects.Add(coA);
      cd.CollisionObjects.Add(coB);

      // not touching
      goB.Pose = new Pose(new Vector3F(3, 3, 0));
      cd.Update(0);
      Assert.AreEqual(0, cd.ContactSets.Count);

      // not touching, disabled
      coB.Enabled = false;
      cd.Update(0);
      Assert.AreEqual(0, cd.ContactSets.Count);

      // touching, disabled
      goB.Pose = new Pose(new Vector3F(1, 1, 1));
      cd.Update(0);
      Assert.AreEqual(0, cd.ContactSets.Count);

      // touching, enabled
      coB.Enabled = true;
      cd.Update(0);
      Assert.AreEqual(1, cd.ContactSets.Count);

      // not touching - but broadphase overlap, enabled
      goB.Pose = new Pose(new Vector3F(1.8f, 1.8f, 0));
      cd.Update(0);
      Assert.AreEqual(0, cd.ContactSets.Count);

      // not touching, disabled
      coB.Enabled = false;
      cd.Update(0);
      Assert.AreEqual(0, cd.ContactSets.Count);

      // touching, disabled
      goB.Pose = new Pose(new Vector3F(1, 1, 1));
      cd.Update(0);
      Assert.AreEqual(0, cd.ContactSets.Count);

      // touching, enabled
      coB.Enabled = true;
      cd.Update(0);
      Assert.AreEqual(1, cd.ContactSets.Count);
    }


    [Test]
    public void TestEnabledDisabledStochastic()
    {
      // Test random Enabled and Pose values.

      int numberOfObjects = 20;
      int numberOfSteps = 1000;

      Shape shape = new SphereShape(1);
      //var meshShape = new TriangleMeshShape(shape.GetMesh(0.01f, 4));
      //meshShape.Partition = new AabbTree<int>();
      //shape = meshShape;

      var geometricObjects = new GeometricObject[numberOfObjects];
      var collisionObjects = new CollisionObject[numberOfObjects];

      var domain = new CollisionDomain(new CollisionDetection());

      for (int i = 0; i < numberOfObjects; i++)
      {
        geometricObjects[i] = new GeometricObject(shape);
        collisionObjects[i] = new CollisionObject(geometricObjects[i]);
        domain.CollisionObjects.Add(collisionObjects[i]);
      }

      for (int i = 0; i < numberOfSteps; i++)
      {
        for (int j = 0; j < numberOfObjects; j++)
        {
          collisionObjects[j].Enabled = RandomHelper.Random.NextBool();

          domain.Update(0);

          if (RandomHelper.Random.NextFloat(0, 1) > 0.5f)
            geometricObjects[j].Pose = new Pose(RandomHelper.Random.NextVector3F(-2, 2));

          domain.Update(0);
        }

        domain.Update(0);
        domain.Update(0);
        domain.Update(0);
        domain.Update(0);
        domain.Update(0);
        domain.Update(0);
        domain.Update(0);

        // Compare result with brute-force check.
        for (int j = 0; j < numberOfObjects; j++)
        {
          for (int k = j + 1; k < numberOfObjects; k++)
          {
            var haveContact = domain.CollisionDetection.HaveContact(collisionObjects[j], collisionObjects[k]);
            Assert.AreEqual(haveContact, domain.ContactSets.GetContacts(collisionObjects[j], collisionObjects[k]) != null);
          }
        }
      }
    }
  }
}

