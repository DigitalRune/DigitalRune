// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Physics.Materials
{
  /// <summary>
  /// Computes the material properties for two materials in contact.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Many material properties depend on two objects. For example, the friction of ice on rubber is
  /// different from rubber on rubber. The material property combiner computes the coefficient that
  /// is actually used in the simulation to simulate the physical behavior at a contact.
  /// </para>
  /// <para>
  /// Combining material coefficients can be done in different ways. The accurate way would be to
  /// look up the exact value in a material table. The simple way is to use a mathematical operation
  /// to combine the coefficients, e.g. multiply both coefficients or compute the average. For most
  /// scenarios in games, the latter method is sufficient.
  /// </para>
  /// </remarks>
  public interface IMaterialPropertyCombiner
  {
    /// <summary>
    /// Computes the combined friction coefficient.
    /// </summary>
    /// <param name="frictionA">The first friction coefficient.</param>
    /// <param name="frictionB">The second friction coefficient.</param>
    /// <returns>
    /// The combined friction coefficient.
    /// </returns>
    float CombineFriction(float frictionA, float frictionB);


    /// <summary>
    /// Computes the combined coefficient of restitution.
    /// </summary>
    /// <param name="restitutionA">The first coefficient of restitution.</param>
    /// <param name="restitutionB">The second coefficient of restitution.</param>
    /// <returns>
    /// The combined coefficient of restitution.
    /// </returns>
    float CombineRestitution(float restitutionA, float restitutionB);
  }
}
