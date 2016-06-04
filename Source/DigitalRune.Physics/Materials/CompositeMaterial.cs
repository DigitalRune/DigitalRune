// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Materials
{
  /// <summary>
  /// Defines a material with different materials for each shape feature of a rigid body.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This material can be used with rigid bodies that have a complex shape, e.g. a 
  /// <see cref="CompositeShape"/> or a <see cref="TriangleMeshShape"/>. The features of a 
  /// <see cref="CompositeShape"/> are the <see cref="CompositeShape.Children"/> of the composite
  /// shape. The features of a <see cref="TriangleMeshShape"/> are the triangles of the mesh. 
  /// </para>
  /// <para>
  /// The <see cref="Materials"/> list can store a material for each feature of a complex shape. 
  /// </para>
  /// <para>
  /// To determine the material for a shape feature, the <see cref="CompositeMaterial"/> first 
  /// checks if the <see cref="Materials"/> list has an entry for this shape. If the list has less 
  /// elements than the shape has features or the material is <see langword="null"/>, the 
  /// <see cref="CompositeMaterial"/> checks whether the shape of the rigid body is a 
  /// <see cref="CompositeShape"/> and the given shape features is of type <see cref="RigidBody"/>.
  /// If the shape feature is a rigid body instance, the material of this rigid body instance is 
  /// used. In all other cases the <see cref="DefaultMaterial"/> is used.
  /// </para>
  /// </remarks>
  public class CompositeMaterial : IMaterial
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the default material that is used for shape features that have no entry
    /// in <see cref="Materials"/>.
    /// </summary>
    /// <value>
    /// The default material. The default value is a new instance of <see cref="UniformMaterial"/>.
    /// </value>
    public UniformMaterial DefaultMaterial { get; private set; }


    /// <summary>
    /// Gets the list of materials for the shape features.
    /// </summary>
    /// <value>The materials for the shape features.</value>
    /// <remarks>
    /// Example: If this material is used for a rigid body with a <see cref="TriangleMeshShape"/>,
    /// the 9th item in this list defines the material of the 9th triangle in the triangle mesh. If
    /// this list has less items than the number of triangles or the item is <see langword="null"/>,
    /// the <see cref="DefaultMaterial"/> is used for the other triangles.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Breaking change. Fix in next version.")]
    public List<UniformMaterial> Materials { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMaterial"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMaterial"/> class.
    /// </summary>
    public CompositeMaterial()
      : this(new UniformMaterial())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeMaterial"/> class.
    /// </summary>
    /// <param name="defaultMaterial">The default material.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="defaultMaterial"/> is <see langword="null"/>.
    /// </exception>
    public CompositeMaterial(UniformMaterial defaultMaterial)
    {
      if (defaultMaterial == null)
        throw new ArgumentNullException("defaultMaterial");

      DefaultMaterial = defaultMaterial;
      Materials = new List<UniformMaterial>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    public MaterialProperties GetProperties(RigidBody body, Vector3F positionLocal, int featureIndex)
    {
      if (body == null)
        throw new ArgumentNullException("body");

      // No child feature - use default material.
      if (featureIndex < 0)
        return DefaultMaterial.GetProperties(body, positionLocal, featureIndex);

      IMaterial material = null;

      // Try to find entry in list.
      if (featureIndex < Materials.Count)
        material = Materials[featureIndex];

      if (material == null)
      {
        // Check if feature is a rigid body.
        var compositeShape = body.Shape as CompositeShape;
        if (compositeShape != null && featureIndex < compositeShape.Children.Count)
        {
          var childBody = compositeShape.Children[featureIndex] as RigidBody;
          if (childBody != null)
            material = childBody.Material;
        }
      }

      if (material == null)
      {
        // Fallback: Use DefaultMaterial.
        material = DefaultMaterial;
      }

      return material.GetProperties(body, positionLocal, featureIndex);
    }
    #endregion
  }
}
