// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Filters item pairs with the help of a user-defined callback method.
  /// </summary>
  /// <typeparam name="T">The type of the items.</typeparam>
  /// <remarks>
  /// <para>
  /// Per default, <see cref="Filter"/> returns <see langword="true"/> for all item pairs. If a 
  /// <see cref="FilterCallback"/> is set, <see cref="Filter"/> calls the 
  /// <see cref="FilterCallback"/> and return its result.
  /// </para>
  /// <para>
  /// If the <see cref="FilterCallback"/> is changed, the <see cref="Changed"/> event is raised
  /// automatically. If the <see cref="FilterCallback"/> does not change but the filter rules
  /// change, <see cref="RaiseChanged"/> must be called to raise the <see cref="Changed"/> event
  /// manually.
  /// </para>
  /// </remarks>
  public class DelegatePairFilter<T> : IPairFilter<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the filter callback.
    /// </summary>
    /// <value>The filter callback. Per default, no callback method is set.</value>
    /// <remarks>
    /// <para>
    /// This method is called in <see cref="Filter"/> to compute the filter result.
    /// </para>
    /// <para>
    /// The <see cref="Changed"/> event is automatically raised if the filter callback is changed.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public Func<Pair<T>, bool> FilterCallback
    {
      get { return _filterCallback; }
      set
      {
        if (_filterCallback != value)
        {
          _filterCallback = value;
          RaiseChanged();
        }
      }
    }
    private Func<Pair<T>, bool> _filterCallback;


    /// <summary>
    /// Occurs when the filter rules were changed.
    /// </summary>
    public event EventHandler<EventArgs> Changed;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatePairFilter{T}"/> class.
    /// </summary>
    /// <param name="filterCallback">The filter callback (can be <see langword="null"/>).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DelegatePairFilter(Func<Pair<T>, bool> filterCallback)
    {
      _filterCallback = filterCallback;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Filters the specified item pair.
    /// </summary>
    /// <param name="pair">The pair.</param>
    /// <returns>
    /// <see langword="true"/> if the pair should be processed (pair is accepted); otherwise,
    /// <see langword="false"/> if the pair should not be processed (pair is rejected).
    /// </returns>
    /// <remarks>
    /// This method returns <see langword="true"/> if no <see cref="FilterCallback"/> is set;
    /// otherwise the filter callback is called and its result is returned.
    /// </remarks>
    public bool Filter(Pair<T> pair)
    {
      if (FilterCallback != null)
        return FilterCallback(pair);

      return true;
    }


    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <remarks>
    /// This method must be called if the filter rules are changed. If the 
    /// <see cref="FilterCallback"/> is changed, this method is called automatically.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    public void RaiseChanged()
    {
      OnChanged(EventArgs.Empty);
    }

    
    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnChanged"/> in a derived
    /// class, be sure to call the base class's <see cref="OnChanged"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnChanged(EventArgs eventArgs)
    {
      var handler = Changed;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
