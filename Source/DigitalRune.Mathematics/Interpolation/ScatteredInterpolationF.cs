// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Base class for scattered interpolation methods (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// Scattered interpolation solves the following problem: Several data pairs have been observed: 
  /// (x1, y1), (x2, y2), (x3, y3), ... These are the reference data values. Now a new x is measured
  /// and the y value for the given x is wanted.
  /// </para>
  /// <para>
  /// The x values are m-dimensional vectors. The y values are n-dimensional vectors. All x vectors
  /// must have equal dimensions and all y vectors must have equal dimensions. But the dimension of 
  /// the x vectors are usually different than the dimension of the y vectors.
  /// </para>
  /// <para>
  /// <strong>How to use this class:</strong>
  /// <list type="number">
  /// <item>
  /// <description>
  /// First the reference data pairs have to be created using the class 
  /// <see cref="Pair{TFirst,TSecond}"/> and registered by calling <see cref="Collection{T}.Add"/>.
  /// The first item of each data pair is the x vector, the second item is the y vector. For
  /// example: 
  /// <code lang="csharp">
  /// <![CDATA[
  /// scatteredInterpolation.Add(new Pair<VectorF, VectorF>(x, y));
  /// ]]>
  /// </code>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// After all reference data pairs are added, <see cref="Setup"/> has to be called.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Then <see cref="Compute"/> can be called to compute y values for any given x values. If the 
  /// reference data pairs are changed, it is necessary to call <see cref="Setup"/> before the next 
  /// call of <see cref="Compute"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Notes to Inheritors:</strong> Derived types need to implement the methods 
  /// <see cref="OnSetup"/> and <see cref="OnCompute"/>. 
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public abstract class ScatteredInterpolationF : Collection<Pair<VectorF, VectorF>>
  {
    // TODO: Manage data pairs in a Collection similar to the Curve class design.

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _prepared;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all elements from the <see cref="Collection{T}"/>.
    /// </summary>
    protected override void ClearItems()
    {
      base.ClearItems();
      _prepared = false;
    }


    /// <summary>
    /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">
    /// The object to insert. The value can be null for reference types.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="MathematicsException">
    /// The vector dimension of a newly added vector is different from the dimensions of the already
    /// registered vectors.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// The x or y vector in <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void InsertItem(int index, Pair<VectorF, VectorF> item)
    {
      ValidatePair(item);
      base.InsertItem(index, item);
      _prepared = false;
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    protected override void RemoveItem(int index)
    {
      base.RemoveItem(index);
      _prepared = false;
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">
    /// The new value for the element at the specified index. The value can be 
    /// <see langword="null"/> for reference types.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="MathematicsException">
    /// The vector dimension of a newly added vector is different from the dimensions of the already
    /// registered vectors.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// The x or y vector in <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void SetItem(int index, Pair<VectorF, VectorF> item)
    {
      ValidatePair(item);
      base.SetItem(index, item);
      _prepared = false;
    }


    private void ValidatePair(Pair<VectorF, VectorF> item)
    {
      if (item.First == null)
        throw new ArgumentNullException("item", "The first vector of the data pair (the x vector) must not be null.");
      if (item.Second == null)
        throw new ArgumentNullException("item", "The second vector of the data pair (the y vector) must not be null.");

      if (Count > 0)
      {
        if (this[0].First.NumberOfElements != item.First.NumberOfElements)
          throw new MathematicsException("The dimension of the new x vector is different from the dimensions of the already registered x vectors.");

        if (this[0].Second.NumberOfElements != item.Second.NumberOfElements)
          throw new MathematicsException("The dimension of the new y vector is different from the dimensions of the already registered y vectors.");
      }
    }


    internal VectorF GetX(int index)
    {
      return this[index].First;
    }


    internal VectorF GetY(int index)
    {
      return this[index].Second;
    }


    /// <summary>
    /// Prepares the scattered interpolation.
    /// </summary>
    /// <remarks>
    /// This method has to be called prior to <see cref="Compute"/> every time the reference data
    /// pairs are changed.
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// No reference data pairs were added.
    /// </exception>
    public void Setup()
    {
      if (Count == 0)
        throw new MathematicsException("No reference data pairs were added.");

      OnSetup();
      _prepared = true;
    }


    /// <summary>
    /// Computes a y value for the specified x value using scattered interpolation.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <returns>The y value.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="x"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="MathematicsException">
    /// No reference data pairs were added.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public VectorF Compute(VectorF x)
    {
      if (x == null)
        throw new ArgumentNullException("x");

      if (_prepared == false)
        Setup();

      return OnCompute(x);
    }


    /// <summary>
    /// Called when <see cref="ScatteredInterpolationF.Setup"/> is called.
    /// </summary>
    /// <remarks>
    /// Here internal values can be computed from the registered reference pairs if required. It is
    /// assured that the reference data pairs have valid dimensions: All x values have the same
    /// number of elements and all y values have the same number of elements. All reference data
    /// values are not <see langword="null"/>. And there is at least 1 reference data pair.
    /// </remarks>
    protected abstract void OnSetup();


    /// <summary>
    /// Called when <see cref="ScatteredInterpolationF.Compute"/> is called.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <returns>The y value.</returns>
    /// <remarks>
    /// When this method is called, <see cref="ScatteredInterpolationF.Setup"/> has already been
    /// executed. The parameter <paramref name="x"/> is not <see langword="null"/> and there is at 
    /// least 1 reference data pair.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    protected abstract VectorF OnCompute(VectorF x);
    #endregion
  }
}
