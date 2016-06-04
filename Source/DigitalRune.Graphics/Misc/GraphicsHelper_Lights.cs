// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  partial class GraphicsHelper
  {
    /// <summary>
    /// Computes the light attenuation factor for a given distance.
    /// </summary>
    /// <param name="distance">The distance to the light's origin.</param>
    /// <param name="range">The range of the light.</param>
    /// <param name="exponent">The falloff exponent.</param>
    /// <returns>The light attenuation factor.</returns>
    /// <remarks>
    /// <para>
    /// The intensity of the light continually decreases from the origin up to range. At a distance 
    /// of range the light intensity is 0. This method computes the attenuation factor at a given 
    /// distance. 
    /// </para>
    /// <para>
    /// The attenuation factor is computed as follows:
    /// <list type="table">
    /// <listheader>
    /// <term>Distance</term>
    /// <description>Attenuation Factor</description>
    /// </listheader>
    /// <item>
    /// <term>distance ≤ 0</term>
    /// <description>1</description>
    /// </item>
    /// <item>
    /// <term>0 &lt; distance &lt; 1</term>
    /// <description>1 - (distance / range)<sup>exponent</sup></description>
    /// </item>
    /// <item>
    /// <term>distance ≥ 1</term>
    /// <description>0</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public static float GetDistanceAttenuation(float distance, float range, float exponent)
    {
      if (distance <= 0)
        return 1.0f;

      if (distance >= range)
        return 0.0f;

      float relativeDistance = distance / range;
      return 1 - (float)Math.Pow(relativeDistance, exponent);
    }


    /// <summary>
    /// Computes the angular attenuation (spotlight falloff) for a given angle.
    /// </summary>
    /// <param name="angle">The angle relative to the main light direction in radians.</param>
    /// <param name="falloffAngle">The falloff angle.</param>
    /// <param name="cutoffAngle">The cutoff angle.</param>
    /// <returns>
    /// The angular attenuation of the light intensity. (1 when <paramref name="angle"/> is less 
    /// than or equal to <paramref name="falloffAngle"/>. 0 when <paramref name="angle"/> is 
    /// greater than or equal to <paramref name="cutoffAngle"/>.)
    /// </returns>
    /// <remarks>
    /// <para>
    /// The falloff between <paramref name="falloffAngle"/> and <paramref name="cutoffAngle"/> is 
    /// computed using a smoothstep function (see 
    /// <see cref="InterpolationHelper.HermiteSmoothStep(float)"/>).
    /// </para>
    /// <para>
    /// <i>angularAttenuation</i> = smoothstep((cos(<i>angle</i>) - cos(<i>cutoffAngle</i>)) /
    /// (cos(<i>falloffAngle</i>) - cos(<i>cutoffAngle</i>)))
    /// </para>
    /// </remarks>
    public static float GetAngularAttenuation(float angle, float falloffAngle, float cutoffAngle)
    {
      if (angle < 0)
        angle = -angle;

      if (angle <= falloffAngle)
        return 1.0f;

      if (angle >= cutoffAngle)
        return 0.0f;

      float cosOuterCone = (float)Math.Cos(cutoffAngle);
      float cosInnerCone = (float)Math.Cos(falloffAngle);
      float cosAngle = (float)Math.Cos(angle);
      float cosDiff = cosInnerCone - cosOuterCone;
      if (Numeric.IsZero(cosDiff))
        return 0.0f;

      float x = (cosAngle - cosOuterCone) / cosDiff;
      return InterpolationHelper.HermiteSmoothStep(x);
    }


    /// <summary>
    /// Gets a factor that is an approximation of the perceived light contribution of the given 
    /// light falling on an object at the given world space position.
    /// </summary>
    /// <param name="lightNode">The light node.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="chromacityWeight">
    /// The weight that determines how important chromacity is compared to the uncolored light 
    /// intensity, ranging from 0 (not important) to 1 very important. Chromacity is the color bias 
    /// of a light. For example, 0.7 is a good value for this parameter.
    /// </param>
    /// <returns>
    /// A value that is proportional to the perceived contribution of the light. If the value is
    /// high, then the light node is important for the scene.
    /// </returns>
    /// <remarks>
    /// This method computes an approximation which can be use to sort lights by importance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="lightNode"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "chromacity")]
    public static float GetLightContribution(this LightNode lightNode, Vector3F position, float chromacityWeight)
    {
      if (lightNode == null)
        throw new ArgumentNullException("lightNode");

      var distance = (position - lightNode.PoseWorld.Position).Length;
      Vector3F intensity = lightNode.Light.GetIntensity(distance);

      // Following formula is from 
      //   Shader X3, Reduction of Lighting Calculations Using Spherical Harmonics.

      float intensityFactor = intensity.X + intensity.Y + intensity.Z;

      // ReSharper disable InconsistentNaming
      float deltaRG = Math.Abs(intensity.X - intensity.Y);
      float deltaGB = Math.Abs(intensity.Y - intensity.Z);
      float deltaRB = Math.Abs(intensity.X - intensity.Z);
      // ReSharper restore InconsistentNaming

      float chromacityFactor = Math.Max(deltaRG, Math.Max(deltaGB, deltaRB));
      return chromacityFactor * chromacityWeight + intensityFactor * (1 - chromacityWeight);
    }
  }
}
