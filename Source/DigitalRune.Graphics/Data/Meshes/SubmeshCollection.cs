// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a collection of <see cref="Submesh"/>es. 
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class SubmeshCollection 
    : ChildCollection<Mesh, Submesh>, 
      ICollection<Submesh>  // The interface is necessary for the VS class diagrams!
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SubmeshCollection"/> class.
    /// </summary>
    /// <param name="owner">The <see cref="Mesh"/> that owns this collection.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="owner"/> is <see langword="null"/>.
    /// </exception>
    public SubmeshCollection(Mesh owner) : base(owner)
    {
    }


    /// <summary>
    /// Gets the parent of an object.
    /// </summary>
    /// <param name="child">The child object.</param>
    /// <returns>The parent of <paramref name="child"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Mesh GetParent(Submesh child)
    {
      return child.Mesh;
    }


    /// <summary>
    /// Sets the parent of the given object.
    /// </summary>
    /// <param name="child">The child object.</param>
    /// <param name="parent">The parent to set.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void SetParent(Submesh child, Mesh parent)
    {
      child.Mesh = parent;
    }
  }
}
