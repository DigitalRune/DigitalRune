// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Materials
{
  /// <summary>
  /// Defines the material (friction, bounciness, etc.) of a rigid body.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="IMaterial"/> defines the material properties of a rigid body. In simple cases the
  /// material properties of a rigid body are constant for the whole rigid body. In complex cases
  /// different parts of a single rigid body can have different properties or the material
  /// properties can change depending on the simulation time or other parameters.
  /// </para>
  /// </remarks>
  public interface IMaterial
  {
    /// <summary>
    /// Gets the <see cref="MaterialProperties"/> for the given rigid body, position and shape
    /// feature.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <param name="positionLocal">
    /// The local position on the rigid body for which the material properties should be returned.
    /// </param>
    /// <param name="featureIndex">
    /// The index of the shape feature from which the material properties are needed. For a
    /// <see cref="CompositeShape"/> the feature index is the index of the child of the composite
    /// shape. For a <see cref="TriangleMeshShape"/> the feature index is the index of a triangle.
    /// </param>
    /// <returns>
    /// The <see cref="MaterialProperties"/> of the given rigid body at the given position and 
    /// child feature.
    /// </returns>
    MaterialProperties GetProperties(RigidBody body, Vector3F positionLocal, int featureIndex);
  }
}
