// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;
using DigitalRune.Collections;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a collection of <see cref="GraphicsScreen"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// When a new <see cref="GraphicsScreen"/> is added to the collection it is inserted at the end 
  /// of the list. The item at index 0 is considered the backmost graphics screen. The item at index
  /// (<see cref="Collection{T}.Count"/> - 1) is considered the foremost graphics screen. The 
  /// graphics service renders graphics screens back to front.
  /// </para>
  /// <para>
  /// Note: The default enumerator (see <see cref="NotifyingCollection{T}.GetEnumerator"/>) 
  /// iterates the items from back to front.
  /// </para>
  /// </remarks>
  public class GraphicsScreenCollection : NotifyingCollection<GraphicsScreen>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="DigitalRune.Graphics.GraphicsScreen"/> with the specified name.
    /// </summary>
    /// <value>
    /// The graphics screen with the given name, or <see langword="null"/> if no matching graphics
    /// screen is found.
    /// </value>
    public GraphicsScreen this[string name]
    {
      get
      {
        foreach (var screen in this)
          if (screen.Name == name)
            return screen;

        return null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsScreenCollection"/> class.
    /// </summary>
    public GraphicsScreenCollection() : base(false, false)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
