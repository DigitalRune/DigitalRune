// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using DigitalRune.Geometry.Shapes;
//using DigitalRune.Mathematics;
//using DigitalRune.Mathematics.Algebra;


//namespace DigitalRune.Geometry.CollisionDetection.Algorithms
//{
//  /// <summary>
//  /// Answers different geometric queries between a point and a plane.
//  /// </summary>
//  public class PlanePointAlgorithm //: CollisionAlgorithm
//  {
//    //protected override bool ComputeCollision(ContactSet contactSet, CollisionQueryType type)
//    //{
//    //  // Sphere vs. plane has at max. 1 contact.
//    //  Debug.Assert(contactSet.Count <= 1);

//    //  CollisionObject a = contactSet.ObjectA;
//    //  CollisionObject b = contactSet.ObjectB;

//    //  // A should be the sphere, swap objects if necessary.
//    //  bool swapped = false;
//    //  if (a.Shape.ShapeType != ShapeType.Sphere)
//    //  {
//    //    swapped = true;
//    //    CollisionObject dummy = a;
//    //    a = b; b = dummy;
//    //  }

//    //  // Check if collision objects shapes are correct.
//    //  if (a.Shape.ShapeType != ShapeType.Sphere || b.Shape.ShapeType != ShapeType.Plane)
//    //    throw new ArgumentException("The shapes must be a sphere (object A) and a plane (object B).", "contactSet");

//    //  Sphere sphereA = (Sphere)a.Shape;
//    //  Plane planeB = (Plane)b.Shape;

//    //  Vector3F planeWorldNormal = b.Pose.ToWorldDirection(planeB.Normal);
//    //  float planeDistanceFromWorldOrigin = Vector3F.Dot(b.Pose.Position + planeWorldNormal * planeB.DistanceFromOrigin, planeWorldNormal);
//    //  float planeToSphereDistance = Vector3F.Dot(a.Pose.Position, planeWorldNormal) - sphereA.Radius - planeDistanceFromWorldOrigin;

//    //  // HaveContact queries can stop here.
//    //  if (type == CollisionQueryType.Boolean)
//    //    return Numeric.Compare(planeToSphereDistance, 0) <= 0;

//    //  // GetContacts queries can stop here if we don't have contact.
//    //  if (type == CollisionQueryType.Contacts && Numeric.Compare(planeToSphereDistance, 0) > 0)
//    //  {
//    //    contactSet.Clear();
//    //    return false;
//    //  }

//    //  // Compute contact details.
//    //  float penetrationDepth = -planeToSphereDistance;
//    //  Vector3F position = a.Pose.Position - planeWorldNormal * (sphereA.Radius - penetrationDepth / 2);

//    //  Contact contact;
//    //  if (swapped == false)
//    //    contact = new Contact(contactSet, position, -planeWorldNormal, penetrationDepth);
//    //  else
//    //    contact = new Contact(contactSet, position, planeWorldNormal, penetrationDepth);

//    //  // Update contact set.
//    //  if (contactSet.Count > 0)
//    //    contactSet.Replace(contact, 0);
//    //  else
//    //    contactSet.Add(contact);

//    //  return true;
//    //}


//    // see Ericson: "Real-Time Collision Detection", p.144
//  }
//}
