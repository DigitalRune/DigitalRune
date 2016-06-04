using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Tests
{
  [TestFixture]
  public class ContactHelperTest
  {
    [Test]
    public void CreateContact()
    {
      Contact c = ContactHelper.CreateContact(
        new CollisionObject { GeometricObject = new GeometricObject { Pose = new Pose(new Vector3F(1, 2, 3)) } },
                                 new CollisionObject { GeometricObject = new GeometricObject { Pose = new Pose(new Vector3F(4, 5, 6)) } },
                                 new Vector3F(10, 10, 10),
                                 new Vector3F(0, 0, 1),
                                 10,
                                 false);
      Assert.AreEqual(new Vector3F(10, 10, 10), c.Position);
      Assert.AreEqual(new Vector3F(0, 0, 1), c.Normal);
      Assert.AreEqual(10, c.PenetrationDepth);
      Assert.AreEqual(false, c.IsRayHit);
      Assert.AreEqual(new Vector3F(9, 8, 12), c.PositionALocal);
      Assert.AreEqual(new Vector3F(6, 5, -1), c.PositionBLocal);

      Contact surfaceC = ContactHelper.CreateContact(
          new CollisionObject { GeometricObject = new GeometricObject { Pose = new Pose(new Vector3F(1, 2, 3)) } },
                                 new CollisionObject { GeometricObject = new GeometricObject { Pose = new Pose(new Vector3F(4, 5, 6)) } },
                                 new Vector3F(10, 10, 10),
                                 new Vector3F(0, 0, 1),
                                 10,
                                 true);
      Assert.AreEqual(new Vector3F(10, 10, 10), surfaceC.Position);
      Assert.AreEqual(new Vector3F(0, 0, 1), surfaceC.Normal);
      Assert.AreEqual(10, surfaceC.PenetrationDepth);
      Assert.AreEqual(true, surfaceC.IsRayHit);
      Assert.AreEqual(new Vector3F(9, 8, 7), surfaceC.PositionALocal);
      Assert.AreEqual(new Vector3F(6, 5, 4), surfaceC.PositionBLocal);
    }


    //[Test]
    //public void GetContactIndex()
    //{
    //  ContactSet set1 = ContactSet.Create(new CollisionObject(), new CollisionObject());
    //  Assert.AreEqual(-1, ContactHelper.GetContactIndex(set1, new Vector3F(1, 2, 3), 0.1f));

    //  set1.Add(new Contact { Position = new Vector3F(0, 0, 0) });
    //  set1.Add(new Contact { Position = new Vector3F(1, 0, 0) });
    //  set1.Add(new Contact { Position = new Vector3F(0, 1, 0) });
    //  Assert.AreEqual(-1, ContactHelper.GetContactIndex(set1, new Vector3F(1, 2, 3), 0.1f));
    //  Assert.AreEqual(0, ContactHelper.GetContactIndex(set1, new Vector3F(0, 0, 0.05f), 0.1f));
    //  Assert.AreEqual(2, ContactHelper.GetContactIndex(set1, new Vector3F(0.01f, 1, 0.05f), 0.1f));
    //  Assert.AreEqual(2, ContactHelper.GetContactIndex(set1, new Vector3F(0, 2, 0), 10f));
    //}


    [Test]
    public void Merge1()
    {
      ContactSet set = ContactSet.Create(new CollisionObject(), new CollisionObject());

      // Separated contact is not merged      
      Contact contact = Contact.Create();
      contact.Position = new Vector3F();
      contact.PenetrationDepth = -1;
      ContactHelper.Merge(set, contact, CollisionQueryType.Contacts, 0.1f);
      Assert.AreEqual(0, set.Count);

      // Merge first contact.
      contact = Contact.Create();
      //contact.ApplicationData = "AppData";
      contact.Lifetime = 10;
      contact.Position = new Vector3F();
      contact.PenetrationDepth = 1;
      contact.UserData = "UserData";
      ContactHelper.Merge(
        set,
        contact,
        CollisionQueryType.Contacts,
        0.1f);
      Assert.AreEqual(1, set.Count);

      // Merge next contact.
      contact = Contact.Create();
      contact.Position = new Vector3F();
      contact.PenetrationDepth = 1;
      ContactHelper.Merge(
        set,
        contact,
        CollisionQueryType.Contacts,
        0.1f);
      Assert.AreEqual(1, set.Count);
      //Assert.AreEqual("AppData", set[0].ApplicationData);
      Assert.AreEqual(10, set[0].Lifetime);
      Assert.AreEqual("UserData", set[0].UserData);

      // TODO: This functionality was replaced. Write new tests.
      //// Test ray casts.
      //set.Clear();
      //((DefaultGeometry)set.ObjectA.GeometricObject).Shape = new RayShape();
      //Contact newContact = new Contact
      //                     {
      //                       Position = new Vector3F(),
      //                       PenetrationDepth = 1,
      //                       IsRayHit = true
      //                     };
      //ContactHelper.Merge(set, newContact, CollisionQueryType.ClosestPoints, 0.1f);
      //Assert.AreEqual(1, set.Count);

      //newContact = new Contact
      //             {
      //               Position = new Vector3F(1, 2, 3),
      //               PenetrationDepth = -1,
      //               IsRayHit = false
      //             };
      //ContactHelper.Merge(set, newContact, CollisionQueryType.ClosestPoints, 0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(1f, set[0].PenetrationDepth);
      //Assert.AreEqual(new Vector3F(), set[0].Position);

      //newContact = new Contact
      //             {
      //               Position = new Vector3F(1, 2, 3),
      //               PenetrationDepth = 0.5f,
      //               IsRayHit = true
      //             };
      //ContactHelper.Merge(set, newContact, CollisionQueryType.ClosestPoints, 0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(0.5f, set[0].PenetrationDepth);
      //Assert.AreEqual(new Vector3F(1, 2, 3), set[0].Position);

      //set.Clear();
      //set.Add(
      //  new Contact
      //        {
      //          Position = new Vector3F(0, 0, 0),
      //          PenetrationDepth = -1,
      //          IsRayHit = false,
      //        });
      //newContact = new Contact
      //             {
      //               Position = new Vector3F(1, 2, 3), 
      //               PenetrationDepth = -2, 
      //               IsRayHit = false
      //             };
      //ContactHelper.Merge(set, newContact, CollisionQueryType.ClosestPoints, 0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(-1f, set[0].PenetrationDepth);
      //Assert.AreEqual(new Vector3F(0, 0, 0), set[0].Position);

      //newContact = new Contact
      //             {
      //               Position = new Vector3F(1, 2, 3),
      //               PenetrationDepth = -0.5f,
      //               IsRayHit = false,
      //             };
      //ContactHelper.Merge(set, newContact, CollisionQueryType.ClosestPoints, 0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(-0.5f, set[0].PenetrationDepth);
      //Assert.AreEqual(new Vector3F(1, 2, 3), set[0].Position);

      //newContact = new Contact
      //             {
      //               Position = new Vector3F(3, 3, 3),
      //               PenetrationDepth = 1,
      //               IsRayHit = true
      //             };
      //ContactHelper.Merge(set, newContact, CollisionQueryType.ClosestPoints, 0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(1f, set[0].PenetrationDepth);
      //Assert.AreEqual(new Vector3F(3, 3, 3), set[0].Position);

      //((DefaultGeometry)set.ObjectA.GeometricObject).Shape = new Sphere();

      //// Test closest points.
      //set.Clear();

      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(), PenetrationDepth = 1 },
      //  CollisionQueryType.ClosestPoints,
      //  0.1f);
      //Assert.AreEqual(1, set.Count);
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(1, 2, 3), PenetrationDepth = 0 },
      //  CollisionQueryType.ClosestPoints,
      //  0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(1, set[0].PenetrationDepth);
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(1, 2, 3), PenetrationDepth = 1.1f },
      //  CollisionQueryType.ClosestPoints,
      //  0.1f);
      //Assert.AreEqual(1, set.Count);
      //Assert.AreEqual(1.1f, set[0].PenetrationDepth);
      //Assert.AreEqual(new Vector3F(1, 2, 3), set[0].Position);

      //// Test default case with automatic reduction.
      //set.Clear();
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(), PenetrationDepth = 1 },
      //  CollisionQueryType.Contacts,
      //  0.1f);
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(1, 0, 0), PenetrationDepth = 1 },
      //  CollisionQueryType.Contacts,
      //  0.1f);
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(0, 1, 0), PenetrationDepth = 1 },
      //  CollisionQueryType.Contacts,
      //  0.1f);
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(0, 0, 1), PenetrationDepth = 1 },
      //  CollisionQueryType.Contacts,
      //  0.1f);
      //ContactHelper.Merge(
      //  set,
      //  new Contact { Position = new Vector3F(2, 0, 0), PenetrationDepth = 1 },
      //  CollisionQueryType.Contacts,
      //  0.1f);
      //Assert.AreEqual(4, set.Count);  // Reduced to 4 instead of 5.
    }


    [Test]
    public void Merge2()
    {
      ContactSet set1 = ContactSet.Create(new CollisionObject(), new CollisionObject());
      ContactSet set2 = ContactSet.Create(set1.ObjectA, set1.ObjectB);

      Contact contact = Contact.Create();
      contact.Position = new Vector3F(1, 2, 3);
      set2.Add(contact);

      ContactHelper.Merge(set2, set1, CollisionQueryType.Contacts, 0.01f);
      Assert.AreEqual(1, set2.Count);

      contact = Contact.Create();
      contact.Position = new Vector3F(1, 2, 3);
      set1.Add(contact);

      contact = Contact.Create();
      contact.Position = new Vector3F(2, 2, 3);
      set1.Add(contact);

      contact = Contact.Create();
      contact.Position = new Vector3F(3, 2, 3);
      set1.Add(contact);

      ContactHelper.Merge(set2, set1, CollisionQueryType.Contacts, 0.01f);
      Assert.AreEqual(3, set2.Count);
    }


    //[Test]
    //public void Reduce()
    //{
    //  ContactSet set1 = ContactSet.Create(new CollisionObject(), new CollisionObject());
    //  set1.Add(new Contact { Position = new Vector3F(0, 0, 0) });
    //  set1.Add(new Contact { Position = new Vector3F(1, 0, 0) });
    //  set1.Add(new Contact { Position = new Vector3F(0.1f, 0.1f, 0) });
    //  set1.Add(new Contact { Position = new Vector3F(0, 1, 0) });
    //  set1.Add(new Contact { Position = new Vector3F(1, 1, 0) });

    //  ContactHelper.ReduceContacts(set1);
    //  Assert.AreEqual(4, set1.Count);
    //  Assert.AreEqual(new Vector3F(0, 0, 0), set1[0].Position);
    //  Assert.AreEqual(new Vector3F(1, 0, 0), set1[1].Position);
    //  Assert.AreEqual(new Vector3F(0, 1, 0), set1[2].Position);
    //  Assert.AreEqual(new Vector3F(1, 1, 0), set1[3].Position);

    //  set1.Insert(1, new Contact { Position = new Vector3F(0.1f, 0.1f, 0) });
    //  ContactHelper.ReduceContacts(set1);
    //  Assert.AreEqual(4, set1.Count);
    //  Assert.AreEqual(new Vector3F(0, 0, 0), set1[0].Position);
    //  Assert.AreEqual(new Vector3F(1, 0, 0), set1[1].Position);
    //  Assert.AreEqual(new Vector3F(0, 1, 0), set1[2].Position);
    //  Assert.AreEqual(new Vector3F(1, 1, 0), set1[3].Position);

    //  set1.Insert(3, new Contact { Position = new Vector3F(0.1f, 0.1f, 0) });
    //  ContactHelper.ReduceContacts(set1);
    //  Assert.AreEqual(4, set1.Count);
    //  Assert.AreEqual(new Vector3F(0, 0, 0), set1[0].Position);
    //  Assert.AreEqual(new Vector3F(1, 0, 0), set1[1].Position);
    //  Assert.AreEqual(new Vector3F(0, 1, 0), set1[2].Position);
    //  Assert.AreEqual(new Vector3F(1, 1, 0), set1[3].Position);
    //}


    [Test]
    public void UpdateContacts()
    {
      CollisionObject a = new CollisionObject { GeometricObject = new GeometricObject { Shape = new SphereShape(1) } };
      CollisionObject b = new CollisionObject(new GeometricObject(new SphereShape(2), new Pose(new Vector3F(2, 0, 0))));
      a.Changed = false;
      b.Changed = false;
      ContactSet set = ContactSet.Create(a, b);
      ContactHelper.UpdateContacts(set, 0.01f, 0.1f);

      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 0, 0), new Vector3F(1, 0, 0), 0, false));
      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 1, 0), new Vector3F(0, 0, 1), 0, false));
      ContactHelper.UpdateContacts(set, 0.01f, 0.1f);
      Assert.AreEqual(0.01f, set[0].Lifetime);
      Assert.AreEqual(0.01f, set[1].Lifetime);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(1.8f, 0.1f, -0.1f));
      ContactHelper.UpdateContacts(set, 0.01f, 0.3f);
      Assert.AreEqual(0.02f, set[0].Lifetime);
      Assert.AreEqual(0.02f, set[1].Lifetime);
      Assert.IsTrue(Numeric.AreEqual(0.2f, set[0].PenetrationDepth));
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[1].PenetrationDepth));

      // Remove first contact because it separates.
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(2.1f, 0, -0.1f));
      ContactHelper.UpdateContacts(set, 0.01f, 0.4f);
      Assert.AreEqual(0.03f, set[0].Lifetime);
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[0].PenetrationDepth));

      // Remove second contact because of horizontal drift.
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(2.2f, 0.0f, -0.1f));
      ContactHelper.UpdateContacts(set, 0.01f, 0.1f);
      Assert.AreEqual(0, set.Count);

      // Test surface contacts (ray casts).
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(2, 0, 0));
      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 0, 0), new Vector3F(1, 0, 0), 0, true));
      set.Add(ContactHelper.CreateContact(set, new Vector3F(1, 0, 0), new Vector3F(1, 0, 0), 0, false));

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(1.9f, 0, 0));
      ContactHelper.UpdateContacts(set, 0.01f, 0.2f);
      Assert.AreEqual(2, set.Count);
      Assert.IsTrue(Numeric.AreEqual(0f, set[0].PenetrationDepth));
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[1].PenetrationDepth));

      ContactHelper.UpdateContacts(set, 0.01f, 0.1f);
      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[0].PenetrationDepth));
    }
  }
}