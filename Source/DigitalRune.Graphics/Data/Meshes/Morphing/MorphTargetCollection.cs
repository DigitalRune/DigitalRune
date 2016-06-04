// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a collection of morph targets.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="MorphTargetCollection"/> may only be assigned to one
  /// <see cref="Graphics.Submesh"/>. It cannot be assigned to multiple submeshes simultaneously.
  /// </para>
  /// <para>
  /// Items in this collection must not be <see langword="null"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="MorphTarget"/>
  public class MorphTargetCollection : NamedObjectCollection<MorphTarget>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the submesh that owns the morph targets.
    /// </summary>
    /// <value>The submesh that owns the morph targets.</value>
    internal Submesh Submesh { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MorphTargetCollection"/> class.
    /// </summary>
    public MorphTargetCollection()
      : base(StringComparer.Ordinal, 8)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Clears the morph target names, which are cached by the <see cref="Mesh"/>.
    /// </summary>
    private void InvalidateMorphTargetNames()
    {
      if (Submesh != null)
          Submesh.InvalidateMorphTargetNames();
    }


    /// <summary>
    /// Removes all morph targets from the collection.
    /// </summary>
    protected override void ClearItems()
    {
      base.ClearItems();
      InvalidateMorphTargetNames();
    }


    /// <summary>
    /// Inserts a morph target into the collection at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The morph target to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void InsertItem(int index, MorphTarget item)
    {
      base.InsertItem(index, item);
      InvalidateMorphTargetNames();
    }


    /// <summary>
    /// Removes the morph target at the specified index of the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      base.RemoveItem(index);
      InvalidateMorphTargetNames();
    }


    /// <summary>
    /// Replaces the morph target at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the material to replace.</param>
    /// <param name="item">The new value for the morph target at the specified index.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void SetItem(int index, MorphTarget item)
    {
      base.SetItem(index, item);
      InvalidateMorphTargetNames();
    }
    #endregion
  }
}
