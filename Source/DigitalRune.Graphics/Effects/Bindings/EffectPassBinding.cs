// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects the effect passes and determines the order in which they need to be applied.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Irrelevant for IEnumerable.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public struct EffectPassBinding : IEnumerable<EffectPass>, IEquatable<EffectPassBinding>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly EffectTechniqueBinding _techniqueBinding;
    private readonly EffectTechnique _technique;
    private readonly RenderContext _context;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectPassBinding"/> struct.
    /// </summary>
    /// <param name="techniqueBinding">The effect technique binding.</param>
    /// <param name="technique">The effect technique.</param>
    /// <param name="context">The render context.</param>
    internal EffectPassBinding(EffectTechniqueBinding techniqueBinding, EffectTechnique technique, RenderContext context)
    {
      _techniqueBinding = techniqueBinding;
      _technique = technique;
      _context = context;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through all effect passes of the current effect 
    /// technique.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> that can be used to iterate the effect passes of the 
    /// current effect technique.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="EffectPassBinding"/> is invalid.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public EffectPassEnumerator GetEnumerator()
    {
      if (_techniqueBinding == null)
      {
        // The EffectPassBinding is invalid (e.g. created via default constructor).
        throw new InvalidOperationException("Invalid EffectPassBinding.");
      }

      return new EffectPassEnumerator(_techniqueBinding, _technique, _context);
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<EffectPass> IEnumerable<EffectPass>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" /> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(EffectPassBinding other)
    {
      return Equals(_techniqueBinding, other._techniqueBinding) 
             && Equals(_technique, other._technique) 
             && Equals(_context, other._context);
    }


    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="System.Object" /> is equal to this 
    /// instance; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is EffectPassBinding && Equals((EffectPassBinding)obj);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures 
    /// like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (_techniqueBinding != null ? _techniqueBinding.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (_technique != null ? _technique.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (_context != null ? _context.GetHashCode() : 0);
        return hashCode;
      }
    }


    /// <summary>
    /// Compares two <see cref="EffectPassBinding"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first effect pass binding.</param>
    /// <param name="right">The second effect pass binding.</param>
    /// <returns>
    /// <see langword="true"/> if the effect pass bindings are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(EffectPassBinding left, EffectPassBinding right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="EffectPassBinding"/>s to determine whether they are different.
    /// </summary>
    /// <param name="left">The first effect pass binding.</param>
    /// <param name="right">The second effect pass binding.</param>
    /// <returns>
    /// <see langword="true"/> if the effect pass bindings are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(EffectPassBinding left, EffectPassBinding right)
    {
      return !left.Equals(right);
    }
    #endregion
  }
}
