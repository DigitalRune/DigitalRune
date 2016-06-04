// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that creates a bouncing effect.
  /// </summary>
  public class BounceEase : EasingFunction
  {
    /// <summary>
    /// Gets or sets the number of bounces before the final bounce to the target value.
    /// </summary>
    /// <value>
    /// The number of bounces before the final bounce to the target value. The value must be greater
    /// than or equal to 0. Negative values are internally clamped to 0. The default value is 3.
    /// </value>
    public int Bounces { get; set; }


    /// <summary>
    /// Gets or sets the bounciness of the animation.
    /// </summary>
    /// <value>
    /// <para>
    /// The bounciness of the animation. This value is a factor that defines how the size of the
    /// bounces increases. For example, a value of 2 means that the subsequent bounce is two times 
    /// as high as the previous bounce.
    /// </para>
    /// <para>
    /// The value must be greater than 1. The default value is 2.
    /// </para>
    /// </value>
    public float Bounciness { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BounceEase"/> class.
    /// </summary>
    public BounceEase()
    {
      Bounces = 3;
      Bounciness = 2.0f;
    }


    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = MathHelper.Clamp(normalizedTime, 0.0f, 1.0f);

      // We assume that a single bounce is a simple parabola: 
      //   f(x) = a (1 - x²)
      // where a is the height (amplitude) of the bounce.

      // Lets start with the following values:
      //   b ... bounciness
      //   n ... number of bounces (not counting the last half bounce)
      double n = Math.Max(0.0, Bounces); // Use double to avoid cast below.
      float b = Bounciness;

      // Ensure that b > 1, otherwise the bounces can't increase in size.
      if (Numeric.IsLessOrEqual(b, 1.0f))
        b = 1.01f;

      // When the bounciness is b the next bounce is b-times as long and high as previous.
      // Let's assume we start with a bounce with a size of 1 then the length of the bounces is a 
      // geometric sequence:
      //   1, b, b², b³, ..., b^(n-1)
      // The total length of all bounces is a geometric series:
      //   1 + b + b² + b³ + ... + b^(n-1)
      // The sum of the first n bounces is 
      //   (1-b^n) / (1-b)
      // The size of the last half bounce is
      //   b^n / 2
      // Then the sum of all bounces is 
      //   (1-b^n) / (1-b) + b^n / 2
      float bn = (float)Math.Pow(b, n);
      float oneMinusBn = 1.0f - b;
      float sum = ((1.0f - bn) / oneMinusBn) + (bn * 0.5f);

      // Get the current position along the bounces given by the normalized time.
      float x = t * sum;

      // Lets get the zero-based index of the current bounce.
      // We can get the index from the exponent of the current bounce.
      // We know that the sum of the first n bounces is:
      //   sum = (1-b^n) / (1-b)
      // We can generalize
      //   x = (1-b^e) / (1-b)
      // and solve for the exponent e
      //   x (1-b) = (1-b^e)
      //   x (1-b) - 1 = -b^e
      //   -x (1-b) + 1 = b^e
      //   e = log(b, -x (1-b) + 1)
      // The zero-based index of the current bounce is the exponent e rounded down.
      //   i = floor(e)
      float i = (float)Math.Floor(Math.Log((-x * (1.0f - b)) + 1.0f, b));

      // Get the start and end of the bounce:
      //   xs = (1-b^i) / (1-b)
      //   xe = (1-b^(i+1)) / (1-b)
      float xStart = (1.0f - (float)Math.Pow(b, i)) / oneMinusBn;
      float xEnd = (1.0f - (float)Math.Pow(b, i + 1.0f)) / oneMinusBn;

      // The vertex is in the middle of the bounce parabola:
      float xVertex = (xStart + xEnd) / 2.0f;
      
      Debug.Assert(xStart <= x && x <= xEnd, "The current position should be within the current bounce.");

      // The position relative to the vertex of the bounce is:
      float xRel = x - xVertex;

      // We need to normalize the value to the range [0, 1]:
      float xNormalized = xRel / (xEnd - xVertex);

      // The amplitude of the bounce i is
      //   b^i
      // The amplitude of the last half bounce is
      //   b^n
      // Hence the normalized amplitude of the bounce is
      //   b^i / b^n = 1 / b^(n-i)
      float aNormalized = (float)Math.Pow(1.0f / b, n - i);

      // Finally we can evaluate the parabola equation:
      return aNormalized * (1 - xNormalized * xNormalized);
    }
  }
}
