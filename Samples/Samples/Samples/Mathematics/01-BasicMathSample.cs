using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Analysis;

// Both XNA and DigitalRune have a class called MathHelper. To avoid compiler errors
// we need to define which MathHelper we want to use.
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Mathematics
{
  [Sample(SampleCategory.Mathematics,
    @"A few examples that demonstrate the use of DigitalRune Mathematics.",
    @"This samples shows:
- How to use Numeric to compare floating-point numbers.
- How to compare vectors.
- How to rotate vectors using quaternions or matrices.
- How to solve a linear system of equations.
- How to find an approximate solution for an overdetermined linear system of equations 
  (using the least-squares method).
- How to use root-finding to solve a non-linear equation.",
    1)]
  public class BasicMathSample : BasicSample
  {
    public BasicMathSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;

      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("\n\n");

      CompareFloats();
      RotateVector();
      CompareVectors();
      SolveLinearSystems();
      FindRoot();
    }


    // In general, floating-point numbers should not be compared using "==".
    // The class Numeric provides methods for comparing floating-point numbers.
    private void CompareFloats()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("----- CompareFloats Example:");

      // Lets do some arbitrary calculations.
      float a = (float)Math.PI;
      float b = (float)Math.Sqrt(a);
      float c = (float)Math.Sqrt(b);
      float d = c * c * c * c;

      // d should be equal to a, but because of limited precision and numerical 
      // errors a != d.
      if (a == d)
        debugRenderer.DrawText("a == d");
      else
        debugRenderer.DrawText("a != d");   // This message is written. :-(

      // The Numeric class has tolerant comparison methods that can be used.
      if (Numeric.AreEqual(a, d))
        debugRenderer.DrawText("Numeric.AreEqual(a, d) is true.\n");  // This message is written. :-)
      else
        debugRenderer.DrawText("Numeric.AreEqual(a, d) is false.\n");

      // The class Numeric contains several other methods for handling floating-point 
      // numbers (float and double).
    }


    // In this method, a vector is rotated with a quaternion and a matrix. The result 
    // of the two vector rotations are compared.
    private void RotateVector()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("----- RotateVector Example:");

      // Create a vector. We will rotate this vector.
      Vector3F v = new Vector3F(1, 2, 3);

      // Create another vector which defines the axis of a rotation.
      Vector3F rotationAxis = Vector3F.UnitZ;

      // The rotation angle in radians. We want to rotate 50°.
      float rotationAngle = MathHelper.ToRadians(50);

      // ----- Part 1: Rotate a vector with a quaternion.

      // Create a quaternion that represents a 50° rotation around the axis given
      // by rotationAxis.
      QuaternionF rotation = QuaternionF.CreateRotation(rotationAxis, rotationAngle);

      // Rotate the vector v using the rotation quaternion.
      Vector3F vRotated = rotation.Rotate(v);

      // ----- Part 2: Rotate a vector with a matrix.

      // Create a matrix that represents a 50° rotation around the axis given by
      // rotationAxis.
      Matrix33F rotationMatrix = Matrix33F.CreateRotation(rotationAxis, rotationAngle);

      // Rotate the vector v using the rotation matrix.
      Vector3F vRotated2 = rotationMatrix * v;

      // ----- Part 3: Compare the results.
      // The result of both rotations should be identical. 
      // Because of numerical errors there can be minor differences in the results.
      // Therefore we use Vector3F.AreNumericallyEqual() two check if the results
      // are equal (within a sensible numerical tolerance).
      if (Vector3F.AreNumericallyEqual(vRotated, vRotated2))
        debugRenderer.DrawText("Vectors are equal.\n");   // This message is written.
      else
        debugRenderer.DrawText("Vectors are not equal.\n");
    }


    // This method shows how to safely compare vectors.
    private void CompareVectors()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("----- CompareVectors Example:");

      // Define a vector.
      Vector3F v0 = new Vector3F(1000, 2000, 3000);

      // Define a rotation quaternion that rotates 360° around the x axis.
      QuaternionF rotation = QuaternionF.CreateRotationX(MathHelper.ToRadians(360));

      // Rotate v0.
      Vector3F v1 = rotation.Rotate(v0);

      // The rotated vector v1 should be identical to v0 because a 360° rotation 
      // should not change the vector. - But due to numerical errors v0 and v1 are 
      // not equal.
      if (v0 == v1)
        debugRenderer.DrawText("Vectors are equal.");
      else
        debugRenderer.DrawText("Vectors are not equal.");   // This message is written. 

      // With Vector3F.AreNumericallyEqual() we can check if two vectors are equal 
      // when we allow a small numeric tolerance. The tolerance that is applied is 
      // Numeric.EpsilonF, e.g. 10^-5.
      if (Vector3F.AreNumericallyEqual(v0, v1))
        debugRenderer.DrawText("Vectors are numerically equal.\n");   // This message is written.
      else
        debugRenderer.DrawText("Vectors are not numerically equal.\n");
    }


    // This method shows how to solve linear systems of equations.
    private void SolveLinearSystems()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("----- SolveLinearSystems Example:");

      // ----- Part 1: We want to solve following system of equations:
      {
        //  3x + 4y - 10z =  6
        // -7x + 9y       =  0
        //   x - 2y + 3z  = -8

        // We will represent this linear system using matrices and vectors: 
        //   A * x = b
        // where the vector x contains the unknown variables.

        // Define the coefficient matrix A:
        Matrix33F A = new Matrix33F(3, 4, -10,
                                    -7, 9, 0,
                                     1, -2, 3);

        // Define the result vector b:
        Vector3F b = new Vector3F(6, 0, -8);

        // x can be computed with x = A^-1 * b.
        Vector3F x = A.Inverse * b;
        // Note: A.Inverse will throw an exception if A is not invertible.

        // Check the result.
        if (Vector3F.AreNumericallyEqual(A * x, b))
          debugRenderer.DrawText("Solution is correct.\n");   // This message is written. 
        else
          debugRenderer.DrawText("Solution is incorrect.\n");
      }

      // ----- Part 2: We want to solve following system of equations:
      {
        //  3x + 4y =  6
        // -7x + 9y =  1
        //   x - 2y = -1
        // This linear system is overdetermined - there are more equations than unknown.
        // We can use MatrixF.SolveLinearEquations() to find an approximate solution 
        // using the least-squares method.

        // Define the coefficient matrix A:
        MatrixF A = new MatrixF(
          new float[3, 2]
          {
            {  3,  4 },
            { -7,  9 },
            {  1, -2 }
          });

        // Define the result vector b. 
        // (MatrixF.SolveLinearEquations() takes b as MatrixF not VectorF!)
        MatrixF b = new MatrixF(
          new float[3, 1]
          {
            {  6 }, 
            {  1 }, 
            { -1 }
          });

        // Next we compute x.
        // MatrixF.SolveLinearEquations() computes the exact result if possible. 
        // For overdetermined systems (A has more rows than columns) the least-
        // squares solution is computed.
        MatrixF x = MatrixF.SolveLinearEquations(A, b);

        // The result x is an approximate solution for the above overdetermined linear system.
        debugRenderer.DrawText("Result: x = " + x[0, 0] + ", y = " + x[1, 0] + "\n");
      }
    }


    // This method defines a simple non-linear function of x.
    private static float Foo(float x)
    {
      // Foo(x) = 4x³ - 5x² + 3x - 4
      return 4 * x * x * x - 5 * x * x + 3 * x - 4;
    }


    // This method defines the first-order derivative of Foo(x).
    private static float FooDerived(float x)
    {
      // Foo'(x) = 12x² - 10x + 3
      return 12 * x * x - 10 * x + 3;
    }


    // This method shows how to use root finding.
    // A method y = Foo(x) is given and we want to find x such that Foo(x) = 7.
    private void FindRoot()
    {
      var debugRenderer = GraphicsScreen.DebugRenderer2D;
      debugRenderer.DrawText("----- RootFinding Example:");

      // Create a new instance of a root finder class.
      // The Newton-Raphson root finding method uses the function and the first-order 
      // derivative of the function where we want to find solutions. If the derivate 
      // is not known, other root finding classes can be used, like the BisectionMethod 
      // or RegulaFalsiMethod classes.
      var rootFinder = new ImprovedNewtonRaphsonMethodF(Foo, FooDerived);

      // Several x could result in Foo(x) = 7. So we need to define an x interval 
      // that contains the desired x solution. 
      // We decide to look for a solution near x = 0 and let the rootFinder approximate 
      // an x interval that contains the result.
      float x0 = 0;
      float x1 = 0;
      // Find an interval [x0, x1] that contains an x such that Foo(x) = 7.
      rootFinder.ExpandBracket(ref x0, ref x1, 7);

      // Now we use the root finder to find an x within [x0, x1] such that Foo(x) = 7.
      float x = rootFinder.FindRoot(x0, x1, 7);

      // Lets check the result. 
      float y = Foo(x);

      // Important note: We use Numeric.AreEqual() to safely compare floating-point numbers.
      if (Numeric.AreEqual(y, 7))
        debugRenderer.DrawText("Solution is correct.\n");
      else
        debugRenderer.DrawText("Solution is wrong.\n");
    }
  }
}
