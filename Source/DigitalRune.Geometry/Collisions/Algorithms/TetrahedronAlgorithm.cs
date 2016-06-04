// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace DigitalRune.Geometry.CollisionDetection.Algorithms
//{
//  class TetrahedronAlgorithm
//  {
//    /// <summary>
//    /// Updates the cached closest point info from the given vertex array.
//    /// </summary>
//    /// <param name="vertices">The vertex array.</param>
//    /// <returns><c>true</c> if the point could be computed; <c>false</c> if the 
//    /// triangle is degenerated.</returns>
//    public bool UpdateClosestPointToOrigin(List<Vector3F> vertices)
//    {
//      // Lets compute lambda1 and lambda2 such that the closest point to the origin x of Triangle (a, b, c)
//      // is:  a + lambda1 * ab + lambda2 * ac = x
//      // If x is the closest point to the origin, then the vector x is normal to the triangle. Hence,
//      // it is also normal to the edges:
//      // ab * x = 0
//      // ac * x = 0
//      // Now we substitute x with a + lambda1 * ab + lambda2 * ac.
//      // We solve the linear system of equations with Cramer's rule... -->
//      // det = ab²*ac² - (ab*ac)²
//      // lambda1 = ((a*ac)(ab*ac) - (a*ab)*ac²) / det
//      // lambda2 = ((ab*ac)(a*ab) - ab²*(a*ac)) / det
//      // The closest point is in the triangle if lambda1 + lambda2 <= 1
//      // To avoid the division by det we store det and do not divide the lambdas by det.
//      // Then the closest point is in the triangle if lambda1 + lambda2 <= det.

//      Vector3F a = vertices[_indices[0]];
//      Vector3F b = vertices[_indices[1]];
//      Vector3F c = vertices[_indices[2]];
//      Vector3F ab = b - a;
//      Vector3F ac = c - a;
//      float ab2 = ab.LengthSquared;
//      float ac2 = ac.LengthSquared;
//      float aDotAb = Vector3F.Dot(a, ab);
//      float aDotAc = Vector3F.Dot(a, ac);
//      float abDotAc = Vector3F.Dot(ab, ac);

//      _det = ab2 * ac2 - abDotAc * abDotAc;
//      _lambda1 = aDotAc * abDotAc - aDotAb * ac2;
//      _lambda2 = abDotAc * aDotAb - ab2 * aDotAc;

//      if (_det > Numeric.EpsilonF)
//      {
//        _closestPointToOrigin = a + (_lambda1 * ab + _lambda2 * ac) / _det;
//        _distanceToOriginSquared = _closestPointToOrigin.LengthSquared;
//        return true;
//      }

//      return false;
//    }

// More in the GjkSimplexSolver...


//  }
//}
