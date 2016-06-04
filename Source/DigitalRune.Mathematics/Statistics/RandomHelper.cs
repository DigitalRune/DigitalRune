// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// A class to generate random values.
  /// </summary>
  public static class RandomHelper
  {
    /// <summary>
    /// Gets or sets the default random number generator.
    /// </summary>
    /// <value>
    /// The default random number generator. 
    /// </value>
    /// <remarks>
    /// This is a global random number generator. Per default, this property is initialized with a 
    /// new instance of <see cref="System.Random"/> with a time-dependent default seed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// The property is set <see langword="null"/>.
    /// </exception>
    public static Random Random
    {
      get { return _random; }
      set 
      { 
        if (value == null)
          throw new ArgumentNullException("value");

        _random = value; 
      }
    }
    private static Random _random = new Random();


    /// <summary>
    /// Gets a random boolean value.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <returns>A random boolean value.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
    public static bool NextBool(this Random random)
    {
      if (random == null)
        random = Random;

      return random.Next(2) == 0;
    }


    /// <summary>
    /// Gets a random byte value.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <returns>A random byte value.</returns>
    public static int NextByte(this Random random)
    {
      if (random == null)
        random = Random;

      return random.Next(255);
    }


    /// <summary>
    /// Gets a random unit <see cref="QuaternionF"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <returns>A random unit <see cref="QuaternionF"/>.</returns>
    public static QuaternionF NextQuaternionF(this Random random)
    {
      if (random == null)
        random = Random;

      return QuaternionF.CreateRotation(NextVector3F(random, -1, 1), NextFloat(random, 0, ConstantsF.TwoPi));
    }


    /// <summary>
    /// Gets a random unit <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <returns>A random unit <see cref="QuaternionD"/>;.</returns>
    public static QuaternionD NextQuaternionD(this Random random)
    {
      if (random == null)
        random = Random;

      return QuaternionD.CreateRotation(NextVector3D(random, -1, 1), NextDouble(random, 0, 2 * Math.PI));
    }


    /// <summary>
    /// Gets a random <see langword="float"/> value that lies in the interval 
    /// [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value.</param>
    /// <param name="max">The maximal allowed value.</param>
    /// <returns>A random <see langword="float"/> value within the bounds [min, max].</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
    public static float NextFloat(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return (float)random.NextDouble() * (max - min) + min;
    }


    /// <summary>
    /// Gets a random <see langword="double"/> value that lies in the interval 
    /// [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global
    /// random number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value.</param>
    /// <param name="max">The maximal allowed value.</param>
    /// <returns>A random <see langword="double"/> value within the bounds [min, max].</returns>
    public static double NextDouble(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return random.NextDouble() * (max - min) + min;
    }


    /// <summary>
    /// Gets a random integer value that lies in the interval [<paramref name="min"/>, 
    /// <paramref name="max"/>].
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value.</param>
    /// <param name="max">
    /// The maximal allowed value. (Must be less than <see cref="int.MaxValue"/>.)
    /// </param>
    /// <returns>A random integer value within the bounds [min, max].</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "max+1")]
    public static int NextInteger(this Random random, int min, int max)
    {
      if (random == null)
        random = Random;

      return random.Next(min, max + 1);
    }


    /// <summary>
    /// Gets a random <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="Vector2F"/>.</returns>
    public static Vector2F NextVector2F(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return new Vector2F(NextFloat(random, min, max),
                          NextFloat(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Vector2D"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="Vector2D"/>.</returns>
    public static Vector2D NextVector2D(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return new Vector2D(NextDouble(random, min, max),
                          NextDouble(random, min, max));
    }

    
    /// <summary>
    /// Gets a random <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="Vector3F"/>.</returns>
    public static Vector3F NextVector3F(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return new Vector3F(NextFloat(random, min, max),
                          NextFloat(random, min, max),
                          NextFloat(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Vector3D"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="Vector3D"/>.</returns>
    public static Vector3D NextVector3D(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return new Vector3D(NextDouble(random, min, max),
                          NextDouble(random, min, max),
                          NextDouble(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Vector4F"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="Vector4F"/>.</returns>
    public static Vector4F NextVector4F(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return new Vector4F(NextFloat(random, min, max),
                          NextFloat(random, min, max),
                          NextFloat(random, min, max),
                          NextFloat(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Vector4D"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="Vector4D"/>.</returns>
    public static Vector4D NextVector4D(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return new Vector4D(NextDouble(random, min, max),
                          NextDouble(random, min, max),
                          NextDouble(random, min, max),
                          NextDouble(random, min, max));
    }



    /// <summary>
    /// Fills a <see cref="VectorF"/> with random values.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="vector">The vector that should be filled with random values.</param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="VectorF"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void NextVectorF(this Random random, VectorF vector, float min, float max)
    {
      if (random == null)
        random = Random;

      for (int i = 0; i < vector.NumberOfElements; i++)
        vector[i] = NextFloat(random, min, max);
    }


    /// <summary>
    /// Fills a <see cref="VectorD"/> with random values.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="vector">The vector that should be filled with random values.</param>
    /// <param name="min">The minimal allowed value for a vector element.</param>
    /// <param name="max">The maximal allowed value for a vector element.</param>
    /// <returns>A random <see cref="VectorD"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void NextVectorD(this Random random, VectorD vector, double min, double max)
    {
      if (random == null)
        random = Random;

      for (int i = 0; i < vector.NumberOfElements; i++)
        vector[i] = NextDouble(random, min, max);
    }

    /// <summary>
    /// Gets a random <see cref="Matrix22F"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    /// <returns>A random <see cref="Matrix22F"/>.</returns>
    public static Matrix22F NextMatrix22F(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return new Matrix22F(NextFloat(random, min, max), NextFloat(random, min, max),
                           NextFloat(random, min, max), NextFloat(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Matrix22D"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    /// <returns>A random <see cref="Matrix22D"/>.</returns>
    public static Matrix22D NextMatrix22D(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return new Matrix22D(NextDouble(random, min, max), NextDouble(random, min, max),
                           NextDouble(random, min, max), NextDouble(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Matrix33F"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    /// <returns>A random <see cref="Matrix33F"/>.</returns>
    public static Matrix33F NextMatrix33F(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return new Matrix33F(NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max),
                           NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max),
                           NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Matrix33D"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    /// <returns>A random <see cref="Matrix33D"/>.</returns>
    public static Matrix33D NextMatrix33D(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return new Matrix33D(NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max),
                           NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max),
                           NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Matrix44F"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    /// <returns>A random <see cref="Matrix44F"/>.</returns>
    public static Matrix44F NextMatrix44F(this Random random, float min, float max)
    {
      if (random == null)
        random = Random;

      return new Matrix44F(NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max),
                           NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max),
                           NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max),
                           NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max), NextFloat(random, min, max));
    }


    /// <summary>
    /// Gets a random <see cref="Matrix44D"/>.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    /// <returns>A random <see cref="Matrix44D"/>.</returns>
    public static Matrix44D NextMatrix44D(this Random random, double min, double max)
    {
      if (random == null)
        random = Random;

      return new Matrix44D(NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max),
                           NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max),
                           NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max),
                           NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max), NextDouble(random, min, max));
    }


    /// <summary>
    /// Fills a <see cref="MatrixF"/> with random values.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="matrix">The matrix that is filled with random values.</param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void NextMatrixF(this Random random, MatrixF matrix, float min, float max)
    {
      if (random == null)
        random = Random;

      for (int r = 0; r < matrix.NumberOfRows; r++)
        for (int c = 0; c < matrix.NumberOfColumns; c++)
          matrix[r, c] = NextFloat(random, min, max);
    }


    /// <summary>
    /// Fills a <see cref="MatrixD"/> with random values.
    /// </summary>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="matrix">The matrix that is filled with random values.</param>
    /// <param name="min">The minimal allowed value for a matrix element.</param>
    /// <param name="max">The maximal allowed value for a matrix element.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static void NextMatrixD(this Random random, MatrixD matrix, double min, double max)
    {
      if (random == null)
        random = Random;

      for (int r = 0; r < matrix.NumberOfRows; r++)
        for (int c = 0; c < matrix.NumberOfColumns; c++)
          matrix[r, c] = NextDouble(random, min, max);
    }


    /// <summary>
    /// Gets a new random value for the specified probability distribution.
    /// </summary>
    /// <typeparam name="T">The type of the random value.</typeparam>
    /// <param name="random">
    /// The random number generator. If this parameter is <see langword="null"/>, the global random
    /// number generator (see <see cref="RandomHelper.Random"/>) is used.
    /// </param>
    /// <param name="distribution">The probability distribution.</param>
    /// <returns>A random value.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public static T Next<T>(this Random random, Distribution<T> distribution)
    {
      if (random == null)
        random = Random;

      return distribution.Next(random);
    }
  }
}
