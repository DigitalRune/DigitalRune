// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Physics.Materials
{
  /// <summary>
  /// Computes the material properties for two materials in contact using simple mathematical
  /// operations.
  /// </summary>
  /// <remarks>
  /// Two given coefficients are combined using a simple mathematical operation 
  /// (see <see cref="FrictionMode"/>, <see cref="RestitutionMode"/>) and the result is clamped. 
  /// Friction is clamped to the range [<see cref="MinFriction"/>, <see cref="MaxFriction"/>]. 
  /// Restitution is clamped to the range [0, <see cref="MaxRestitution"/>].
  /// </remarks>
  public class MaterialPropertyCombiner : IMaterialPropertyCombiner
  {
    // Notes: Others use GeometricMean for friction and AverageMean for restitution and CFM.

    /// <summary>
    /// Defines how two coefficients are combined.
    /// </summary>
    public enum Mode
    {
      /// <summary>Use the sum of the two coefficients.</summary>
      Add,
      /// <summary>Use the minimum of the two coefficients.</summary>
      Min,
      /// <summary>Use the maximum of the two coefficients.</summary>
      Max,
      /// <summary>Use the arithmetic mean (average) of the two coefficients.</summary>
      ArithmeticMean,
      /// <summary>Use the geometric mean of the two coefficients.</summary>
      GeometricMean,
      /// <summary>Use the product of the two coefficients.</summary>
      Multiply,
    }


    /// <summary>
    /// Gets or sets the mode that is used to combine friction coefficients.
    /// </summary>
    /// <value>
    /// The mode that is used to combine friction coefficients.
    /// The default is <see cref="Mode.GeometricMean"/>.
    /// </value>
    public Mode FrictionMode { get; set; }


    /// <summary>
    /// Gets or sets the minimal friction value.
    /// </summary>
    /// <value>The minimal friction value. The default value is 0.</value>
    public float MinFriction { get; set; }


    /// <summary>
    /// Gets or sets the maximal friction value.
    /// </summary>
    /// <value>The maximal friction value. The default value is 10.</value>
    public float MaxFriction { get; set; }


    /// <summary>
    /// Gets or sets the mode that is used to combine coefficients of restitution.
    /// </summary>
    /// <value>
    /// The mode that is used to combine coefficients of restitution.
    /// The default is <see cref="Mode.Multiply"/>.
    /// </value>
    public Mode RestitutionMode { get; set; }


    /// <summary>
    /// Gets or sets the maximal restitution value.
    /// </summary>
    /// <value>The maximal restitution value. The default value is 1.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxRestitution
    {
      get { return _maxRestitution; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxRestitution must be greater than or equal to 0.");

        _maxRestitution = value;
      }
    }
    private float _maxRestitution;


    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialPropertyCombiner"/> class.
    /// </summary>
    public MaterialPropertyCombiner()
    {
      FrictionMode = Mode.GeometricMean;
      MinFriction = 0;
      MaxFriction = 10;

      RestitutionMode = Mode.Multiply;
      MaxRestitution = 1;
    }


    /// <summary>
    /// Computes the combined friction coefficient.
    /// </summary>
    /// <param name="frictionA">The first friction coefficient.</param>
    /// <param name="frictionB">The second friction coefficient.</param>
    /// <returns>
    /// The combined friction coefficient.
    /// </returns>
    public float CombineFriction(float frictionA, float frictionB)
    {
      float combinedFriction;
      switch(FrictionMode)
      {
        case Mode.Add:
          combinedFriction = frictionA + frictionB;
          break;
        case Mode.Min:
          combinedFriction = Math.Min(frictionA, frictionB);
          break;
        case Mode.Max:
          combinedFriction = Math.Max(frictionA, frictionB);
          break;
        case Mode.Multiply:
          combinedFriction = frictionA * frictionB;
          break;
        case Mode.ArithmeticMean:
          combinedFriction = 0.5f * (frictionA + frictionB);
          break;
        case Mode.GeometricMean:
          combinedFriction = (float)Math.Sqrt(frictionA * frictionB);
          break;
        default:
          throw new InvalidOperationException("Unhandled case in switch statement.");
      }

      return MathHelper.Clamp(combinedFriction, MinFriction, MaxFriction);
    }


    /// <summary>
    /// Computes the combined coefficient of restitution.
    /// </summary>
    /// <param name="restitutionA">The first coefficient of restitution.</param>
    /// <param name="restitutionB">The second coefficient of restitution.</param>
    /// <returns>
    /// The combined coefficient of restitution.
    /// </returns>
    public float CombineRestitution(float restitutionA, float restitutionB)
    {
      float combinedRestitution;
      switch (RestitutionMode)
      {
        case Mode.Add:
          combinedRestitution = restitutionA + restitutionB;
          break;
        case Mode.Min:
          combinedRestitution = Math.Min(restitutionA, restitutionB);
          break;
        case Mode.Max:
          combinedRestitution = Math.Max(restitutionA, restitutionB);
          break;
        case Mode.Multiply:
          combinedRestitution = restitutionA * restitutionB;
          break;
        case Mode.ArithmeticMean:
          combinedRestitution = 0.5f * (restitutionA + restitutionB);
          break;
        case Mode.GeometricMean:
          combinedRestitution = (float)Math.Sqrt(restitutionA * restitutionB);
          break;
        default:
          throw new InvalidOperationException("Unhandled case in switch statement.");
      }

      return MathHelper.Clamp(combinedRestitution, 0, MaxRestitution);
    }
  }
}
