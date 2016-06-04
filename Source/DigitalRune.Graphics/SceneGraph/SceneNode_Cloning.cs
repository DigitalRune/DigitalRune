// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry;


namespace DigitalRune.Graphics.SceneGraph
{
  partial class SceneNode
  {
    /// <inheritdoc/>
    IGeometricObject IGeometricObject.Clone()
    {
      return Clone();
    }


    /// <summary>
    /// Creates a new <see cref="SceneNode"/> that is a clone of the current instance (incl. all 
    /// children).
    /// </summary>
    /// <returns>
    /// A new <see cref="SceneNode"/> that is a clone of the current instance (incl. all children).
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="SceneNode"/> (section "Cloning") for more information.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="SceneNode"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="SceneNode"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public SceneNode Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNode"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="SceneNode"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private SceneNode CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone SceneNode. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="SceneNode"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="SceneNode"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="SceneNode"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual SceneNode CreateInstanceCore()
    {
      return new SceneNode();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="SceneNode"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="SceneNode"/> derived class must 
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to 
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(SceneNode source)
    {
      _flags = source._flags;
      Name = source.Name;
      ScaleLocal = source.ScaleLocal;
      PoseLocal = source.PoseLocal;
      LastScaleWorld = source.LastScaleWorld;
      LastPoseWorld = source.LastPoseWorld;
      Shape = source.Shape;     // Shallow copy.
      MaxDistance = source.MaxDistance;
      SortTag = source.SortTag;
      UserData = source.UserData;
      // Do not clone: RenderData, SceneData

      if (source.Children != null)
      {
        if (Children == null)
          Children = new SceneNodeCollection();
        else
          Children.Clear();

        foreach (var child in source.Children)
          Children.Add(child.Clone());
      }
    }
  }
}
