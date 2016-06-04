// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Removes contacts if a <see cref="ContactSet"/> contains more than 4 <see cref="Contact"/>s.
  /// </summary>
  /// <remarks>
  /// If the <see cref="ContactSet"/> contains 4 or less <see cref="Contact"/>s this filter does
  /// nothing. If the <see cref="ContactSet"/> contains more than 4 <see cref="Contact"/>s, the 
  /// <see cref="Contact"/> with the deepest penetration depth is kept and 3 more 
  /// <see cref="Contact"/>s that enclose a large area. The order of the <see cref="Contact"/>s in
  /// the <see cref="ContactSet"/> is modified. 
  /// </remarks>
  public class ContactReducer : IContactFilter
  {
    // TODO: Make the max. number of contact points configurable or experiment to find out the optimal number.
    // TODO: We could also keep the convex hull of contact points in the 2D plane defined by the (average) normal vector.

    /// <summary>
    /// Filters the specified contact set.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    public void Filter(ContactSet contactSet)
    {
      if (contactSet == null)
        return;

      int numberOfContacts = contactSet.Count;

      // Nothing to do if we have 4 or less contacts.
      if (numberOfContacts <= 4)
        return;

      // First of all we keep the best contact: This is the contact with the deepest penetration.
      // Note: We could also argue that the newest contact is the best, but then following 
      // problem occurs: Two composite objects touch with 5 contacts. 1 is removed. In
      // the next step the same 5 contacts are found. The one that was removed in the last
      // frame is now the newest contact, so another contact is removed. --> 1 Contact will
      // always toggle and is not persistent.

      // Find the deepest contact
      int deepestContactIndex = 0;
      float deepestPenetrationDepth = contactSet[0].PenetrationDepth;
      for (int i = 1; i < numberOfContacts; i++)
      {
        Contact contact = contactSet[i];
        if (contact.PenetrationDepth > deepestPenetrationDepth)
        {
          deepestPenetrationDepth = contact.PenetrationDepth;
          deepestContactIndex = i;
        }
      }

      // Move deepest contact to index 0.
      if (deepestContactIndex != 0)
      {
        Contact deepest = contactSet[deepestContactIndex];
        contactSet[deepestContactIndex] = contactSet[0];
        contactSet[0] = deepest;
      }

      // Iteratively, remove single contacts until 4 are left.
      while (contactSet.Count > 4)
        Reduce(contactSet);
    }


    /// <summary>
    /// Removes 1 contact.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <remarks>
    /// <c>contactSet[0]</c> is not changed. <c>contactSet[1]</c> to <c>contactSet[4]</c> are
    /// tested. The contacts with the largest area are kept. The other one is removed.
    /// </remarks>
    private static void Reduce(ContactSet contactSet)
    {
      Debug.Assert(contactSet.Count > 4);

      // Remember: The magnitude of the cross product of two vectors is equal to the area of the 
      // defined parallelogram. The parallelogram is twice as large as the triangle area defined 
      // between the three points. 
      Vector3F edge12 = contactSet[2].Position - contactSet[1].Position;
      Vector3F edge13 = contactSet[3].Position - contactSet[1].Position;
      Vector3F edge14 = contactSet[4].Position - contactSet[1].Position;
      Vector3F edge23 = contactSet[3].Position - contactSet[2].Position;
      Vector3F edge24 = contactSet[4].Position - contactSet[2].Position;

      // Check 4 parallelograms.
      float area = Vector3F.Cross(edge12, edge13).LengthSquared;
      float maxArea = area;
      int contactToDelete = 4;

      area = Vector3F.Cross(edge12, edge14).LengthSquared;
      if (area > maxArea)
      {
        maxArea = area;
        contactToDelete = 3;
      }

      area = Vector3F.Cross(edge13, edge14).LengthSquared;
      if (area > maxArea)
      {
        maxArea = area;
        contactToDelete = 2;
      }

      area = Vector3F.Cross(edge23, edge24).LengthSquared;
      if (area > maxArea)
      {
        // maxArea = area;
        contactToDelete = 1;
      }

      contactSet[contactToDelete].Recycle();

      // Remove 1 contact by moving the last contact in the contact set to its index. 
      // Unless the contact to delete is already the last one.
      int maxIndex = contactSet.Count - 1;
      if (contactToDelete != maxIndex)
        contactSet[contactToDelete] = contactSet[maxIndex];

      contactSet.RemoveAt(maxIndex);
    }
  }
}
